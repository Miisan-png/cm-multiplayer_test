using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BoatController_Player : BoatController
{
    //child script - 以下为 子类 文件
    [SerializeField] private CinemachineImpulseSource CamShake;
    public Action<int> SwitchBkCam;
    public Action<int> SwitchFrtCam;

    [SerializeField] private BoatCamera playerCamera;
    public BoatCamera playercamera => playerCamera;

    public Action OnEnterTunnel;
    public Action OnExitTunnel;

    public bool OnTurnLine;

    [Header("Raycast 检测设置")]
    public bool leftBlocked; // 左侧是否被阻挡
    public bool rightBlocked; // 右侧是否被阻挡
    public float sideRayLength = 2f; // 射线长度，可调整
    public LayerMask sideRayMask; // 检测层级，可在Inspector设置

    // ------------------ 前方障碍物Raycast检测与延迟回传 ------------------
    public bool isFrontBlocked = false;
    public bool IsFrontBlocked => isFrontBlocked;

    // private float frontBlockTimer = 0f;
    // private float frontBlockDelay = 1f; // 1秒延迟

    private float baseThrottlePower; // 记录初始推进力
    private float minSideThrottle = 1.5f; // 碰撞时最低保持的推进力比例

    protected override void Start()
    {
        //玩家控制船只
        base.Start();
        baseThrottlePower = throttlePower; // 记录初始推进力
        InputManager.Instance.OnMoveBoat += HandleMovement;
        InputManager.Instance.OnBoatDrift += HandleonBoatDrift;
        InputManager.Instance.OnDriftRelease += ReleaseDrift;
        InputManager.Instance.OnThrottle += throttle;

        //玩家相机状态
        InputManager.Instance.OnSwitchFrontCam += handleSwitchCam;
        onStunStart += CamShake.GenerateImpulse;
        OnEnterTunnel += () => { playerCamera.HandleOnTunnelEntry(); };
        OnExitTunnel += () => { playerCamera.HandleOnTunnelExit(); };

        

    }

    public override void UnAssignCurrentCorner()
    {
        base.UnAssignCurrentCorner();
        OnTurnLine = false;
    }

    //Boat Drift --------------------------------------------------------------------
    protected override void ReleaseDrift()
    {
        base.ReleaseDrift();
        OnTurnLine = false;
    }

    private void HandleonBoatDrift()
    {
        if (OnTurnLine)
        {
            base.TurnBoat(base.NextTurnDirection);
            OnTurnLine = false;
        }
        else
        {
            StartDrift();
        }
        
        // 检查玩家是否当前处于stamina boost状态
        if (base.inSpeedBoost == true)
        {
            // 如果是，则停止stamina boost 
            Debug.Log("Stopping Stamina Boost due to drift");
            StopStaminaBoost();
            
        }
    }

    private bool SwitchCameraCondition()
    {
        return Minigame_BoatRace.Instance.state == MinigameState.Active;
    }

    private void handleSwitchCam(bool frontcam)
    {
        if (!SwitchCameraCondition()) return;
        if(frontcam)
        {
            SwitchFrtCam?.Invoke(playernumber);
        }
        else
        {
            SwitchBkCam?.Invoke(playernumber);
        }
    }

    // 记录上一次的射线检测结果，用于可视化
    private Vector3[] lastSideRayOrigins;
    private Vector3[] lastSideRayDirs;
    private float[] lastSideRayLengths;
    private int lastSideRayResolution = 10;

    private float sideBlockSlowdown = 0.5f; // 碰撞时减速系数（0~1，越小减速越多）

    // 智能的左右检测：仿NPC扇形多射线检测
    private void SideRaycastCheck()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float distance = sideRayLength;
        float radius = 0.25f;
        int resolution = lastSideRayResolution;
        float totalAngle = 180f;
        LayerMask mask = sideRayMask;

        leftBlocked = false;
        rightBlocked = false;

        lastSideRayOrigins = new Vector3[resolution];
        lastSideRayDirs = new Vector3[resolution];
        lastSideRayLengths = new float[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float angle = -totalAngle / 2 + (totalAngle / (resolution - 1)) * i;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            lastSideRayOrigins[i] = origin;
            lastSideRayDirs[i] = dir;
            float rayLen = distance;
            if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, distance, mask))
            {
                if (angle < 0) leftBlocked = true;
                if (angle > 0) rightBlocked = true;
                rayLen = hit.distance;
                // 红色可视化
                Debug.DrawRay(origin, dir * rayLen, Color.red, 0.1f);
            }
            else
            {
                Debug.DrawRay(origin, dir * rayLen, Color.yellow, 0.1f);
            }
            lastSideRayLengths[i] = rayLen;
        }
        // 只做可视化，不再在此处减速
    }

    // 在Scene视图持续画线
    private void OnDrawGizmos()
    {
        if (lastSideRayOrigins != null && lastSideRayDirs != null && lastSideRayLengths != null)
        {
            for (int i = 0; i < lastSideRayOrigins.Length; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(lastSideRayOrigins[i], lastSideRayDirs[i] * lastSideRayLengths[i]);
            }
        }
    }

    // 前方Raycast检测
    private void FrontRaycastCheck()
    {
        Debug.Log("FrontRaycastCheck called");
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = transform.forward;
        float distance = 0.5f;
        float radius = 0.3f;
        LayerMask mask = sideRayMask;

        //
        if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, distance, mask))
        {
            Debug.Log("Hit detected: " + hit.collider.gameObject.name);
            isFrontBlocked = true;
            Debug.DrawRay(origin, dir * hit.distance, Color.red, 10f);

            OnHitDivider();
        }
        // 如果没有检测到障碍物，则不触发 OnHitDivider    
        else
        {
            isFrontBlocked = false;
            Debug.DrawRay(origin, dir * distance, Color.cyan, 10f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with: " + collision.gameObject.name);
        FrontRaycastCheck();

        // 只有左右方向且Layer属于sideRayMask才触发左右减速
        int objLayer = collision.gameObject.layer;
        if (((1 << objLayer) & sideRayMask.value) != 0 && collision.contacts.Length > 0)
        {
            Vector3 contactDir = collision.contacts[0].point - transform.position;
            contactDir.y = 0f;
            contactDir.Normalize();
            float dot = Vector3.Dot(contactDir, transform.right);
            // dot > 0.5 右侧碰撞，dot < -0.5 左侧碰撞
            if (Mathf.Abs(dot) > 0.5f)
            {
                float targetThrottle = baseThrottlePower * sideBlockSlowdown;
                float minThrottle = baseThrottlePower * minSideThrottle;
                throttleLimit = Mathf.Max(targetThrottle, minThrottle);
                return;
            }
        }
        throttleLimit = -1f;
    }

    private void OnCollisionExit(Collision collision)
    {
        // 离开墙壁时恢复推进力上限，并给予短暂加速
        int objLayer = collision.gameObject.layer;
        if (((1 << objLayer) & sideRayMask.value) != 0)
        {
            throttleLimit = -1f;
            // 离开墙壁后给予短暂加速
            StartCoroutine(TemporaryBoostAfterWall());
        }
    }

    private IEnumerator TemporaryBoostAfterWall()
    {
        float boostTime = 0.8f; // 加速时长（可调）
        float prevThrottle = currentThrottle;
        float targetThrottle = Mathf.Max(maxThrottle * 1.2f, maxThrottle); // 超过最大推进力一点点
        float elapsed = 0f;
        while (elapsed < boostTime)
        {
            elapsed += Time.fixedDeltaTime;
            currentThrottle = Mathf.Lerp(prevThrottle, targetThrottle, elapsed / boostTime);
            yield return new WaitForFixedUpdate();
        }
        currentThrottle = Mathf.Min(currentThrottle, maxThrottle); // 恢复到最大推进力
    }

    protected override void HandleMovement(Vector2 input)
    {
        base.HandleMovement(input);
        // 每次玩家有移动输入时，检测左右障碍
        SideRaycastCheck();
        FrontRaycastCheck();
    }
}
