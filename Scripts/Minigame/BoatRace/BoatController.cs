using System;
using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Action = System.Action;

public class BoatController : MonoBehaviour
{
    // 本编程文件 为 父类 
    [SerializeField] private int PlayerNumber;
    public int playernumber => PlayerNumber;

    [Header("Throttle Settings")] //油门
    [SerializeField] protected float throttlePower;
    [SerializeField] protected float throttleSlow;
    [SerializeField] protected float maxThrottle;
    protected float throttleLimit = -1f; // -1表示不限制最大推进力
    public Action onThrottleStart;
    public Action onThrottleEnd;
    [SerializeField] private float fallSpeed;

    [Header("Steering Settings")] //转向
    [SerializeField] private float lateralPower = 1.2f; // 原值可能较大，调低
    [SerializeField] private float lateralSlow = 4.0f;  // 可适当调高，增加回正阻力
    [SerializeField] private float maxLateralSpeed = 1.2f; // 原值可能较大，调低
    [SerializeField] float bumpPower = 10f; // tweak as needed
    private Coroutine bumpcoroutine;
    public Action onBump;

    [Header("Brake Settings")] //刹车
    [SerializeField] private float brakePower;
    [Header("Drift Settings")]
    [SerializeField] private float currentDriftStamina; // 当前漂移体力
    [SerializeField] private float maxDriftStamina ; // 最大漂移体力
    [SerializeField] private float CurrentDriftDuration;
    [SerializeField] private float MaxDriftDuration;
    [SerializeField] private float driftIncrease;
    [SerializeField] private float boosttimer;
    [SerializeField] private float boostMultiplier; //the multiplier
    [SerializeField] private AnimationCurve TurnCurve;
    [SerializeField] private float rotationDuration = 0.5f;
    public Action<float> ondriftUsage_UI; // 通知 UI 用的事件（0~1 的值）
    public Action<TurnDirection> onDrift;
    public Action<TurnDirection> onDirectionChanged;
    public Action onExitCorner;
    public Action onDriftEnd;
    [SerializeField] private float staminaBoostMultiplier = 2.0f; // 加速倍率
    //[SerializeField] private float staminaBoostConsumeRate = 1.0f; // 每秒消耗stamina
    private Coroutine staminaBoostCoroutine;
    
    

    [Header("Stat Values")] //状态
    [SerializeField] protected float currentThrottle;
    [SerializeField] private int lateralMovement;
    [SerializeField] private float currentLateralSpeed;
    [SerializeField] private float currentBrake;
    public Action<float> onSpeedChange;
    private Coroutine speedboostcoroutine;
    private Coroutine teleportCoroutine;

    public int lateralmovement => lateralMovement;

    [Header("Outside Force Settings")] //负效果
    [SerializeField] private float StunDuration;
    public Action onStunStart;
    public Action onStunEnd;

    private Vector3 speedboostCurrentDirection;
    private Vector3 speedboostTarget;
    [SerializeField] private Vector3 bumpForce = Vector3.zero; //撞开方向
    [SerializeField] private Vector3 zoneBoost = Vector3.zero; //加速速度
    private Vector2 lastinput;
    //public Action <float> BoostTimerUI; // 定义了一个“广播器”，
    // 名字叫 BoostTimer，它可以传一个数字（float），
    // 告诉监听的人：当前 Boost 进度是多少（比如 1 = 满，0 = 空）。

    private Coroutine zoneBoostCoroutine; //可延迟或逐帧控制

    [Header("Serialized in Scene")]
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected BoatTrackDetector trackDetector;
    [SerializeField] protected BoatCheckpointDetector checkpointDetector;
    public BoatCheckpointDetector checkpointdetector => checkpointDetector;
    public BoatTrackDetector trackdetector => trackDetector;

    [Header("State & Bools")] //转角状态
    [SerializeField] protected CornerTile CurrentCorner;
    [SerializeField] protected CornerTile NextCorner;
    [SerializeField] protected TurnDirection NextTurnDirection;
    [SerializeField] private BoatState State;
    public Action<GameObject> OnEnterAlternateCorner;
    public BoatState state => State;

    // 油门
    [SerializeField] private bool inThrottle;
    public bool inthrottle => inThrottle;

    // 漂移
    [SerializeField] private bool inDrift;
    public bool indrift => inDrift;

    // 赛道转角
    [SerializeField] private bool inCorner;
    public bool incorner => inCorner;

    // 加速
    [SerializeField] protected bool inSpeedBoost;
    public bool inspeedboost => inSpeedBoost;

    // 地面范围
    public bool InGroundingZone;

    private Action functionState; //它里面可以装一段不带参数的函数，然后我可以在需要的时候执行它 -> Setboat
                                  // 事件广播器

    [SerializeField] public Vector3 lastContactPoint;
    [SerializeField] public Vector3 cachedContactPoint;

    // 替代赛道路线
    [SerializeField] protected bool AllowAlternatePath = true;
    public bool allowalternatepath => AllowAlternatePath;

    // 赛道 道具
    public bool isPF_Collecting_disappear;
    



    // 编程开始 -----------------------------------------------------------------------------------------------------------------

    // 船的状态
    protected virtual void Start()
    {
        setState(BoatState.none); // 船初始状态

    }

    void Update()
    {
        
    }

    private bool DisableStateChange(BoatState s)
    {
        return State == s; //如果你传进来的状态 s 跟现在的 State 一样，就返回 true，表示“没必要换状态”
    }

    public void setState(BoatState s) //设置 船的状态
    {
        if (DisableStateChange(s)) return; //如果一样，就提前退出，不再往下执行状态切换逻辑

        State = s;

        switch (State) //根据 State 的不同值，执行对应的一段代码。
        {
            case BoatState.none:
                functionState = null;
                bumpForce = Vector3.zero;
                currentDriftStamina = 0; // 初始化漂移体力为0
                if (zoneBoostCoroutine != null) //正在进行的区域加速动作
                {
                    StopCoroutine(zoneBoostCoroutine);
                    zoneBoost = Vector3.zero;
                }
                bumpcoroutine = null;
                if (speedboostcoroutine != null) //正在进行的加速
                {
                    StopCoroutine(speedboostcoroutine);
                    speedboostCurrentDirection = Vector3.zero;
                    inSpeedBoost = false;
                    speedboostcoroutine = null;
                    //BoostTimer?.Invoke(0f); // Ensure slider returns to 0 
                    // 通知监听者，Boost 归零
                    // ?.Invoke() -	安全地执行通知，防止空引用错误
                }
                rb.linearVelocity = Vector3.zero;
                currentThrottle = 0;
                currentLateralSpeed = 0.8f;
                currentBrake = 0;
                inDrift = false;
                lateralMovement = 0;
                CurrentDriftDuration = 0;
                onDriftEnd?.Invoke(); //告知漂移已经结束
                break;

            case BoatState.idle:
                cachedContactPoint = lastContactPoint; //把 lastContactPoint 记下来，
                                                       // 存在 cachedContactPoint，以备后用。
                functionState = Movement;
                break;

            case BoatState.stunned:
                functionState = null;
                bumpForce = Vector3.zero; //无撞击
                if (zoneBoostCoroutine != null)
                {
                    StopCoroutine(zoneBoostCoroutine);
                    zoneBoost = Vector3.zero; // 加无加速
                }
                bumpcoroutine = null; //无
                if (speedboostcoroutine != null)
                {
                    StopCoroutine(speedboostcoroutine);
                    speedboostCurrentDirection = Vector3.zero;
                    inSpeedBoost = false;
                    speedboostcoroutine = null;
                    //BoostTimer?.Invoke(0f); // Ensure slider returns to 0
                }
                rb.linearVelocity = Vector3.zero;
                currentThrottle = 0;
                currentLateralSpeed = 0;
                currentBrake = 0;
                inDrift = false;
                lateralMovement = 0;
                CurrentDriftDuration = 0;
                onDriftEnd?.Invoke();
                break;

        }
    }

    ///船体移动
    protected virtual void HandleMovement(Vector2 input) //一个二维向量，玩家的控制输入
    {
        lastinput = input; // 保存当前的输入值（X 为水平，Y 为垂直）
        steering(input.x); // 根据水平输入处理转向
        Brake(input.y);    // 根据垂直输入处理刹车
    }

    protected void throttle(bool Activate) //子类可继承的加速决定
    {
        if (Activate)
        {
            inThrottle = true; // 开启油门状态
            onThrottleStart?.Invoke(); // 通知：油门已开始（如果有监听者）
        }
        else
        {
            inThrottle = false; // 关闭油门状态
            onThrottleEnd?.Invoke();
        }
    }

    // 加速行动
    public void handleCurrentSpeed()
    {
        if (inDrift) return; //Prevent acceleration when in drift

        if (inThrottle)
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, maxThrottle, throttlePower * Time.fixedDeltaTime);
        }
        else
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, -2f, (throttleSlow + currentBrake) * Time.fixedDeltaTime);
        }

        // 只有非inSpeedBoost时才限制最大推进力
        if (!inSpeedBoost) {
            if (throttleLimit > 0)
                currentThrottle = Mathf.Min(currentThrottle, throttleLimit);
            else
                currentThrottle = Mathf.Min(currentThrottle, maxThrottle);
        }
    }

    private bool bumpCondition()
    {
        return State == BoatState.idle && bumpcoroutine == null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BoatController enemy = collision.gameObject.GetComponent<BoatController>();
            StartCoroutine(HandleBoatBump(collision, enemy.bumpPower));
            onBump?.Invoke(); // 播放音效、震动、UI特效等
            Debug.Log("Collided with player");
        }
    }
    public IEnumerator HandleBoatBump(Collision collision, float power)//当船碰到其他玩家时，
                                                                       // 朝着相反方向推开一点点，制造“撞击感”，然后0.15秒后停止这个推力。
    {
        if (!bumpCondition()) //检查当前是否允许产生碰撞反应（例如不能在 stunned 状态下撞
        {
            yield break; // 不满足撞击条件就直接退出协程
        }

        // Find the contact point  获取第一个接触点
        ContactPoint contact = collision.contacts[0];

        // Direction from the collision point to your boat's center
        // 从接触点朝自己的方向算一个单位向量
        // 代表“我被撞了，要往反方向推开”
        Vector3 bumpDirection = (transform.position - contact.point).normalized;

        // Flatten Y axis
        bumpDirection.y = 0f; // 保证水平移动（不往上飞）

        bumpDirection = bumpDirection.normalized * power; //应用推力大小，并赋值给bumpforce

        bumpForce = bumpDirection;  // 把推力存下来

        float timer = 0.15f;

        while (timer > 0) //表示“推动持续一小段时间”
        {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        bumpForce = Vector3.zero; //推力清空，不再继续推
        bumpcoroutine = null; //被设为空，表示没有撞击动作正在进行
    }

    private void TryAddBumpForce()
    {
        if (bumpForce != Vector3.zero)
        {
            rb.AddForce(bumpForce);
        }
    }

    public void HandleSpeedZone(float BoostAmount)
    {
        if (zoneBoostCoroutine != null)
        {
            StopCoroutine(zoneBoostCoroutine);
        }
        zoneBoostCoroutine = StartCoroutine(LerpZoneSpeed(BoostAmount));
    }

    private IEnumerator LerpZoneSpeed(float BoostAmount)
    {
        Vector3 Boost = transform.forward * BoostAmount;

        while (Vector3.Distance(zoneBoost, Boost) > 0.1f)
        {
            zoneBoost = Vector3.MoveTowards(zoneBoost, Boost, boosttimer * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        zoneBoost = transform.forward * BoostAmount;
    }

    private void TryAddZoneBoost()
    {
        if (zoneBoost != Vector3.zero)
        {
            rb.AddForce(zoneBoost, ForceMode.VelocityChange);
        }
    }

    private void Brake(float input)
    {
        if (input <= -0.1f)
        {
            currentBrake = brakePower;
        }
        else
        {
            currentBrake = 0;
        }
    }

    protected void steering(float input)
    {
        if (input >= 0.5f)
        {
            lateralMovement = 1;
        }
        else if (input <= -0.5f)
        {
            lateralMovement = -1;
        }
        else
        {
            lateralMovement = 0;
        }
    }

    private bool DriftCondition()
    {
        return currentThrottle >= maxThrottle / 4 &&
               State != BoatState.stunned &&
               State != BoatState.none &&
               !inDrift;
    }

    protected void StartDrift()
    {
        if (!DriftCondition())
        {
            ReleaseDrift();
            return;
        }
        inDrift = true;

        onDrift?.Invoke(NextTurnDirection);
    }

    //漂移体力控制
    private void HandleDriftStamina() //漂移体力
    {

        // if (inDrift) //如果正在漂移，消耗体力，每帧减少 0.1。
        // {

        //     // currentDriftStamina -= 0.1f;


        //     // if (currentDriftStamina <= 0) //如小于0，则等于0
        //     // {
        //     //     currentDriftStamina = 0;
        //     //     //ReleaseDrift();
        //     // }
        // }
        // else if (!inDrift) //反之，当前漂移体力等于最大体力
        // {

        //     // if (currentDriftStamina < maxDriftStamina)
        //     // {
        //     //     currentDriftStamina += 0.05f; // 恢复速度可调
        //     //     if (currentDriftStamina >   0.05f)
        //     //     {
        //     //         currentDriftStamina = maxDriftStamina;
        //     //     }

        //     // }
        // }

        //计算当前漂移体力的使用比例，并通过事件通知 UI 去更新显示
        float driftusage = currentDriftStamina / maxDriftStamina;
        ondriftUsage_UI?.Invoke(driftusage);
    }

    //// public void ResetStamina()
    //// {
    ////     currentDriftStamina = maxDriftStamina;
    // //    float driftusage = currentDriftStamina / maxDriftStamina;
    ////     ondriftUsage_UI?.Invoke(driftusage);
    //// }

    private void IncreaseDriftAccumulated()
    {
        if (inDrift)
        {
            if (CurrentDriftDuration < MaxDriftDuration)
            {
                CurrentDriftDuration += driftIncrease * Time.fixedDeltaTime;
            }
        }
    }

    private bool TurnCondition()
    {
        return inCorner && State != BoatState.stunned && State != BoatState.none;
    }

    protected virtual void ReleaseDrift()
    {
        if (!inDrift) return;

        if (TurnCondition())
        {
            TurnBoat(NextTurnDirection);
            Debug.Log("Done Turning");
        }
        else
        {
            CurrentDriftDuration = 0;
            inDrift = false;
        }

        onDriftEnd?.Invoke();
        //// 启动stamina加速协程
        //// if (staminaBoostCoroutine != null)
        //// {

        //// StopCoroutine(staminaBoostCoroutine);

        //// }
        //// Debug.Log("stamina boost");
        //// staminaBoostCoroutine = StartCoroutine(StaminaBoostCoroutine());
        
       
    }

    private IEnumerator StaminaBoostCoroutine()
    {
        inSpeedBoost = true;
        // 根据当前体力比例决定加速时长
        float duration = (currentDriftStamina / maxDriftStamina) * 0.6f; // 满体力0.6秒，60%体力0.36秒
        float startStamina = currentDriftStamina;
        float elapsed = 0f;

        while (elapsed < duration && currentDriftStamina > 0)
        {
            elapsed += Time.deltaTime;
            // 线性消耗体力
            currentDriftStamina = Mathf.Lerp(startStamina, 0.5f, elapsed / duration);

            // 通知UI
            float driftusage = currentDriftStamina / maxDriftStamina;
            ondriftUsage_UI?.Invoke(driftusage);

            // 持续加速
            currentThrottle = Mathf.Min(maxThrottle * staminaBoostMultiplier, maxThrottle * 3f);

            yield return null;
        }

        // 确保体力为0
        currentDriftStamina = 0f;
        ondriftUsage_UI?.Invoke(0f);
        // stamina耗光后，恢复正常
        currentThrottle = Mathf.Min(currentThrottle, maxThrottle);
        inSpeedBoost = false;
        staminaBoostCoroutine = null;

        // 确保UI归零
        ondriftUsage_UI?.Invoke(0f);
    }

    protected void StopStaminaBoost()
    {
        
        if (inSpeedBoost == true && staminaBoostCoroutine != null)
        {
            // 停止当前的Stamina Boost Coroutine
            Debug.Log("Stopping Stamina Boost Coroutine");
            StopCoroutine(staminaBoostCoroutine);
            staminaBoostCoroutine = null;
        }

        inSpeedBoost = false;
        currentThrottle = Mathf.Min(currentThrottle, maxThrottle);
        currentDriftStamina = 0f;
        ondriftUsage_UI?.Invoke(0f);
        
        
    }

    private bool SteerCondition()
    {
        // 只要不是漂移/撞击/禁用状态就允许横向控制（低速也能横移）
        return !inDrift && bumpForce == Vector3.zero && State != BoatState.stunned && State != BoatState.none;
    }

    private void HandleCurrentLateralSpeed()
    {
        if (SteerCondition())
        {
            // Apply scaled lateral power
            float effectiveLateralPower = lateralPower * (1 + (currentThrottle / maxThrottle) * 1.5f);

            // 增加横向阻力，速度越大阻力越大
            float resistance = Mathf.Abs(currentLateralSpeed) * 4f; // 阻力系数可调
            float targetLateral = (maxLateralSpeed + currentThrottle) * lateralMovement;
            targetLateral -= resistance * Mathf.Sign(currentLateralSpeed); // 施加阻力

            if (currentLateralSpeed < maxLateralSpeed || currentLateralSpeed > -maxLateralSpeed)
            {
                currentLateralSpeed = Mathf.MoveTowards(
                    currentLateralSpeed,
                    targetLateral,
                    effectiveLateralPower * Time.fixedDeltaTime
                );
            }
        }
        else
        {
            currentLateralSpeed = Mathf.MoveTowards(currentLateralSpeed, 0, lateralSlow * Time.fixedDeltaTime);
        }
    }

    private bool PreventMovementCondition()
    {
        return State == BoatState.none ||
               Minigame_BoatRace.Instance.state != MinigameState.Active;
    }

    private void Movement()
    {
        if (PreventMovementCondition()) return;

        handleCurrentSpeed();
        HandleCurrentLateralSpeed();
        IncreaseDriftAccumulated();
        HandleDriftStamina();

        // Forward movement
        Vector3 moveDirectionForward = transform.forward * currentThrottle * Time.fixedDeltaTime;

        // Lateral movement: boost时横向速度减半
        float lateralSlowFactor = inSpeedBoost ? 0.5f : 1f; // 你可以调整0.5为更小或更大
        Vector3 moveDirectionLateral = inSpeedBoost
            ? (transform.right * currentLateralSpeed * lateralSlowFactor * Time.fixedDeltaTime)
            : (transform.right * currentLateralSpeed * currentThrottle / maxThrottle * Time.fixedDeltaTime);

        Vector3 moveTotal = new Vector3(
            moveDirectionForward.x + moveDirectionLateral.x,
            0,
            moveDirectionForward.z + moveDirectionLateral.z
        );

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 newVelocity = moveTotal + speedboostCurrentDirection + bumpForce;


        rb.linearVelocity = new Vector3(newVelocity.x, currentVelocity.y, newVelocity.z);

        HandleFall();

        TryAddZoneBoost();
        TryAddBumpForce();

        float uiSpeed = currentThrottle / maxThrottle;
        onSpeedChange?.Invoke(uiSpeed);
    }

    // 从水流中掉落
    // 处理掉落逻辑
    private void HandleFall()
    {
        Vector3 fallforce = Vector3.down * fallSpeed;

        fallforce = inSpeedBoost && InGroundingZone ? fallforce * 5f : fallforce;

        fallforce = fallforce * currentThrottle / maxThrottle;

        rb.AddForce(fallforce);
    }

    public void AssignNextCorner(CornerTile currentcorner, CornerTile nextcorner, bool allowTurn)
    {
        if (State == BoatState.stunned) return;

        if (currentcorner.alternatepath && !AllowAlternatePath) return;

        inCorner = allowTurn;
        CurrentCorner = currentcorner;
        NextCorner = nextcorner;
    }

    //公共虚方法：可以在子类中被重写（override）。表示“取消当前角落”的逻辑处理。
    public virtual void UnAssignCurrentCorner()
    {
        //如果当前的角落（CurrentCorner）不是备用路径（alternatepath == false）
        if (!CurrentCorner.alternatepath)
        {
           
            //就从下一个角落（NextCorner）获取转向方向，并设置为 NextTurnDirection。
            NextTurnDirection = NextCorner.directiontoturn; 
        }

        // 触发事件 onDirectionChanged，传入 NextTurnDirection
        // 可能是通知其他系统，例如 UI 或 AI，方向已更新。
        onDirectionChanged?.Invoke(NextTurnDirection);

        onExitCorner?.Invoke();//触发事件 onExitCorner
    }

    private IEnumerator TurnAnimation(float targetYRotation)
    {
        float startYRotation = transform.eulerAngles.y;
        float timeElapsed = 0f;

        // Calculate shortest rotation
        // 从当前 Y 角度到目标 Y 角度的最短旋转
        float shortestRotation = Mathf.DeltaAngle(startYRotation, targetYRotation);
        targetYRotation = startYRotation + shortestRotation;

        float durationtouse = (inSpeedBoost || inDrift) ? 0.07f : rotationDuration;

        while (timeElapsed < durationtouse)
        {
            timeElapsed += Time.deltaTime;
            float curveValue = TurnCurve.Evaluate(timeElapsed / durationtouse);
            float newYRotation = Mathf.Lerp(startYRotation, targetYRotation, curveValue);

            // Use physics rotation instead of direct transform manipulation
            rb.MoveRotation(Quaternion.Euler(0f, newYRotation, 0f));

            yield return new WaitForFixedUpdate();
        }

        // Final rotation with physics
        rb.MoveRotation(Quaternion.Euler(0f, targetYRotation, 0f));
        updateBoostTarget();
    }

    protected void TurnBoat(TurnDirection direction)
    {
        if (!TurnCondition()) return;

        float currentY = transform.eulerAngles.y;
        float targetY = Mathf.Round(currentY / 90f) * 90f + (direction == TurnDirection.Right ? 90f : -90f);

        zoneBoost = Vector3.zero;
        bumpForce = Vector3.zero;

        rb.angularVelocity = Vector3.zero;
        StartCoroutine(TurnAnimation(targetY));
        // 不再直接清零rb.linearVelocity，保留加速分量
        // rb.linearVelocity = Vector3.zero;

        // if we have accumulated atleast half of max drift duration, start a new boost
        if (CurrentDriftDuration > MaxDriftDuration / 1)
        {
            // Stop any existing boost before starting new one
            if (speedboostcoroutine != null)
            {
                StopCoroutine(speedboostcoroutine);
            }

            speedboostcoroutine = StartCoroutine(speedboost(0.1f + CurrentDriftDuration));
        }

        currentLateralSpeed = 0;

        inCorner = false;
        inDrift = false;
        CurrentDriftDuration = 0;
        NextTurnDirection = NextCorner.directiontoturn;
        onDirectionChanged?.Invoke(NextTurnDirection);

        //=== 转角完成后触发stamina boost ===
        if (staminaBoostCoroutine != null)
        {
            StopCoroutine(staminaBoostCoroutine);
        }
        staminaBoostCoroutine = StartCoroutine(StaminaBoostCoroutine());
    }


    private IEnumerator speedboost(float timer)
    {
        inSpeedBoost = true;
        updateBoostTarget();
        zoneBoost = Vector3.zero;
        bumpForce = Vector3.zero;

        // Initial boost buildup (slider goes up)
        float buildupTime = timer * 0.3f; // 30% of time for buildup
        float elapsed = 0f;

        while (elapsed < buildupTime)
        {
            updateBoostTarget();
            speedboostCurrentDirection = Vector3.Lerp(
                speedboostCurrentDirection,
                speedboostTarget,
                boosttimer * Time.fixedDeltaTime
            );

            // Progress goes from 0 to 1 during buildup
            float progress = elapsed / buildupTime;
            //BoostTimer?.Invoke(progress);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Full boost maintained (slider stays at max briefly)
        float fullBoostTime = timer * 0.4f; // 40% of time at full boost
        elapsed = 0f;

        while (elapsed < fullBoostTime)
        {
            updateBoostTarget();
            speedboostCurrentDirection = speedboostTarget;

            // Progress stays at 1 during full boost
            //BoostTimerUI?.Invoke(1f);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Boost cooldown (slider goes down)
        float cooldownTime = timer * 0.5f; // 30% of time for cooldown
        elapsed = 0f;

        while (elapsed < cooldownTime)
        {
            speedboostCurrentDirection = Vector3.Lerp(
                speedboostCurrentDirection,
                Vector3.zero,
                boosttimer * Time.fixedDeltaTime
            );

            // Progress goes from 1 to 0 during cooldown
            float progress = 1 - (elapsed / cooldownTime);
            //BoostTimerUI?.Invoke(progress);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Clean up
        speedboostCurrentDirection = Vector3.zero;
        inSpeedBoost = false;
        speedboostcoroutine = null;
        //BoostTimerUI?.Invoke(0f); // Ensure slider returns to 0
    }
    private void updateBoostTarget()
    {
        // Use current forward direction
        speedboostTarget = transform.forward * currentThrottle * boostMultiplier * Time.fixedDeltaTime;

        // Flatten Y component
        speedboostTarget.y = 0f;
    }


    public void OnHitDivider()
    {
        if (inCorner)
        {
            inCorner = false;
            UnAssignCurrentCorner();
        }
        StartCoroutine(StunSequence());
    }

    private IEnumerator GradualSpeedRecovery(float targetThrottle, float duration)
    {

        float startThrottle = 0f;
        float elapsed = 0f;
        // 先将当前油门归零
        currentThrottle = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            currentThrottle = Mathf.Lerp(startThrottle, targetThrottle, elapsed / duration);
            yield return new WaitForFixedUpdate();
        }
        currentThrottle = targetThrottle;
    }

    private IEnumerator StunSequence()
    {
        setState(BoatState.stunned);
        onStunStart?.Invoke();

        float teleporttimer = StunDuration / 1f;

        while (teleporttimer > 0)
        {
            teleporttimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Vector3 teleportPos = trackDetector.CheckGroundOnce(checkpointDetector.latestcheckpoint.transform.position);

        if (State != BoatState.none)
        {
            teleport(teleportPos, checkpointDetector.latestcheckpoint.transform.rotation, BoatState.stunned);
        }

        float stuntimer = StunDuration / 1f ;

        while (stuntimer > 0)
        {
            stuntimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (State != BoatState.none)
        {
            setState(BoatState.idle);
        }

        // 回传后不自动恢复速度，保持currentThrottle为0，等待玩家操作
        // float recoveryDuration = 0.5f;
        // float targetThrottle = maxThrottle * 0.5f;
        // yield return StartCoroutine(GradualSpeedRecovery(targetThrottle, recoveryDuration));
        // if (inThrottle)
        // {
        //     onThrottleStart?.Invoke();
        // }

        onStunEnd?.Invoke();
    }

    public void teleport(Vector3 Position, Quaternion rotation, BoatState stateafterteleport)
    {
        setState(BoatState.none);

        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
        }

        teleportCoroutine = StartCoroutine(teleportdelay(Position, rotation, stateafterteleport));
    }

    private IEnumerator teleportdelay(Vector3 Position, Quaternion rotation, BoatState stateafterteleport)
    {
        float timer = 0.2f;
        while (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        transform.position = Position;
        transform.rotation = rotation;
        setState(stateafterteleport);
    }

    private void FixedUpdate()
    {
        functionState?.Invoke();
    }


    private void OnTriggerEnter(Collider other) //这里拾取收集品，则加一
    {

        if (other.transform.tag == "Racing_Collection")
        {
            DriftStamina_Collecting();
            var respawn = other.GetComponent<Collection>();
            if (respawn != null)
                respawn.Collect();
            else
            other.gameObject.SetActive(false); // 兼容未加脚本的情况
            
        }

        Debug.Log($"Collided with {other.gameObject.name} with tag {other.gameObject.tag}");
    }

    public void DriftStamina_Collecting()
    {
        currentDriftStamina++;

        if (currentDriftStamina >= 5)
        {
            Debug.Log("Collected all!");
        }

    }

}



public enum BoatState
{
    none, idle, stunned
}