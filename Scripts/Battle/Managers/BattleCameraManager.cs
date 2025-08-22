using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static Unity.Cinemachine.CinemachineCore;

public class BattleCameraManager : MonoBehaviour
{
    private static BattleCameraManager instance;
    public static BattleCameraManager Instance => instance;

    [SerializeField] private List<CinemachineCamera> PreviousCams = new List<CinemachineCamera>();
    [SerializeField] private CinemachineCamera MainCam;
    [SerializeField] private CinemachineCamera P1MonsterCam;
    [SerializeField] private CinemachineCamera P1WinCam;
    [SerializeField] private CinemachineCamera P2CutsceneCam;
    [SerializeField] private CinemachinePositionComposer P2CutsceneComposer;
    [SerializeField] private CinemachineCamera P2MonsterCam;
    [SerializeField] private CinemachineCamera P2WinCam;

    [SerializeField] private CinemachinePositionComposer P1MonIntroCam;
    [SerializeField] private CinemachineRotationComposer P1MonCamPan;

    [SerializeField] private CinemachinePositionComposer P2MonIntroCam;
    [SerializeField] private CinemachineRotationComposer P2MonCamPan;

    [SerializeField] private Camera CharactersRollCam;
    [SerializeField] private Camera GloveCam;
    [SerializeField] private Camera RewardsCam;
    [SerializeField] private CinemachineCamera StartCam;
    [SerializeField] private CinemachinePanTilt MainCamTilt;
    [SerializeField] private float TiltTimer = 0.02f;
    [SerializeField] private CinemachineImpulseSource CamShake1;
    [SerializeField] private GameObject StartCutsceneGrp;
    [SerializeField] private GameObject CaptureCutsceneGrp;

    private Coroutine TiltCoroutine;
    private Coroutine cameraResetCoroutine;
    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        BattleManager.Instance.OnBattleStart += OnBattleStart;
        BattleManager.Instance.OnBattleStartCutscene += OnBattleStartCutscene;
        BattleManager.Instance.OnBattleStartCutsceneSkipped += OnBattleStartCutsceneSkipped;
        BattleManager.Instance.OnBattleStartCutsceneEnd += OnBattleStartCutsceneEnd;
        BattleManager.Instance.OnBattleEnd += OnBattleEnd;
        BattleManager.Instance.OnCapturePrompt += OnCapturePrompt;
        BattleManager.Instance.OnRewardScreen += OnRewardScreen;
        BattleManager.Instance.OnPhaseChange += OnPhaseChange;
        BattleManager.Instance.PlayerSelectAction += OnPlayerSelectAction;
        BattleManager.Instance.MonsterDamaged += OnMonsterDamaged;
    }

    private void UnsubscribeAllEvents()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStart -= OnBattleStart;
            BattleManager.Instance.OnBattleStartCutscene -= OnBattleStartCutscene;
            BattleManager.Instance.OnBattleStartCutsceneSkipped -= OnBattleStartCutsceneSkipped;
            BattleManager.Instance.OnBattleStartCutsceneEnd -= OnBattleStartCutsceneEnd;
            BattleManager.Instance.OnBattleEnd -= OnBattleEnd;
            BattleManager.Instance.OnCapturePrompt -= OnCapturePrompt;
            BattleManager.Instance.OnRewardScreen -= OnRewardScreen;
            BattleManager.Instance.OnPhaseChange -= OnPhaseChange;
            BattleManager.Instance.PlayerSelectAction -= OnPlayerSelectAction;
            BattleManager.Instance.MonsterDamaged -= OnMonsterDamaged;
        }
    }

    private void OnBattleStart()
    {
        RewardsCam.gameObject.SetActive(false);
        StartCutsceneGrp.SetActive(false);
        SwitchtoMainCam();
        StopTiltAnimation();
        OnRollEnd();

        MonsterAsset P1Asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(BattleManager.Instance.GetGlovebyIndex(1).equippedmonster.id);
        MonsterAsset P2Asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(BattleManager.Instance.GetGlovebyIndex(2).equippedmonster.id);

        P1MonIntroCam.CameraDistance = P1Asset.CameraIntroZoom;
        if (P1Asset.CameraIntroPan != 0)
        {
            P1MonIntroCam.Composition.ScreenPosition.y = P1Asset.CameraIntroPan;
        }
        else
        {
            P1MonIntroCam.Composition.ScreenPosition.y = 0.2f;
        }

        P2MonIntroCam.CameraDistance = P2Asset.CameraIntroZoom;

        if (P2Asset.CameraIntroPan != 0)
        {
            P2MonIntroCam.Composition.ScreenPosition.y = P2Asset.CameraIntroPan;
        }
        else
        {
            P2MonIntroCam.Composition.ScreenPosition.y = 0.2f;
        }

        P1MonCamPan.Composition.ScreenPosition = new Vector2(P1MonCamPan.Composition.ScreenPosition.x, P1Asset.CameraMainPan);
        P2MonCamPan.Composition.ScreenPosition = new Vector2(P2MonCamPan.Composition.ScreenPosition.x, P2Asset.CameraMainPan);
        P1MonsterCam.Lens.FieldOfView = P1Asset.CameraMainFOV;
        P2MonsterCam.Lens.FieldOfView = P2Asset.CameraMainFOV;

        cameraResetCoroutine = StartCoroutine(CameraReset());
    }

    private IEnumerator CameraReset()
    {
        MainCam.Priority = 0;
        StartCam.Priority = 0;
        MainCam.Prioritize();
        yield return new WaitForSeconds(1f);
        StartCam.Priority = 10;
    }

    private void OnBattleStartCutscene()
    {
        P2CutsceneCam.LookAt = BattleManager.Instance.battleType == battleType.Wild ? BattleObjectsManager.Instance.p2monstersolo.transform : BattleObjectsManager.Instance.p2human.transform;
        P2CutsceneComposer.TargetOffset.y = BattleManager.Instance.battleType == battleType.Wild ? 0.45f: 1.38f;
        StartCutsceneGrp.SetActive(true);    
    }

    private void OnBattleStartCutsceneSkipped()
    {
        if(cameraResetCoroutine != null)
        {
            StopCoroutine(cameraResetCoroutine);
        }
        StartCutsceneGrp.SetActive(false);
        StartCam.Priority = 0;
        SwitchtoMainCam();
    }

    private void OnBattleStartCutsceneEnd()
    {
        StartCutsceneGrp.SetActive(false);
        StartCam.Priority = 0;
        SwitchtoMainCam();
    }

    private void OnBattleEnd()
    {
        StartCutsceneGrp.SetActive(false);
        StartCam.Priority = 0;
        StopTiltAnimation();
        OnRollEnd();
        SwitchtoWinCam(BattleManager.Instance.playerwon);
    }

    private void OnCapturePrompt()
    {
        P1WinCam.gameObject.SetActive(false);
        P2WinCam.gameObject.SetActive(false);
        CaptureCutsceneGrp.gameObject.SetActive(true);
    }

    private void OnRewardScreen(int[] ExpRewards)
    {
        CaptureCutsceneGrp.gameObject.SetActive(false);
        P1WinCam.gameObject.SetActive(false);
        P2WinCam.gameObject.SetActive(false);
        RewardsCam.gameObject.SetActive(true);
    }

    private void OnPhaseChange(battleState s)
    {
        switch (s)
        {
            case battleState.Start:
                break;
            case battleState.Select:
                SwitchtoMainCam();
                OnRollEnd();
                StartTiltAnimation();
                break;
            case battleState.Roll:
                OnRollPhase();
                StopTiltAnimation();
                break;
            case battleState.Execution:
                StopTiltAnimation();
                break;
            case battleState.End:
                break;
            case battleState.Dialogue:
                break;
            case battleState.None:
                break;
        }
    }

    private void OnPlayerSelectAction(int player, selectAction action)
    {
        SwitchtoMonsterCam(player);
    }

    private void OnMonsterDamaged(int mon, int dmg)
    {
        SwitchtoMonsterCam(mon);
        if (dmg > 0)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                CamShake1.GenerateImpulse();
            });
        }
    }

    public void SwitchtoMainCam()
    {
        ClearPreviousCams();
        MainCam.Priority = 5;
        PreviousCams.Add(MainCam);
    }

    public void SwitchtoMonsterCam(int player)
    {
        ClearPreviousCams();
        CinemachineCamera camera = player == 1 ? P1MonsterCam : P2MonsterCam;
        camera.Priority = 5;
        camera.Prioritize();
        PreviousCams.Add(camera);
    }

    public void SwitchtoWinCam(int player)
    {
        ClearPreviousCams();
        CinemachineCamera camera = player == 1 ? P1WinCam : P2WinCam;
        camera.gameObject.SetActive(true);
        camera.Priority = 5;
        camera.Prioritize();
        PreviousCams.Add(camera);
    }

    private void ClearPreviousCams()
    {
        if (PreviousCams.Count > 0)
        {
            for (int i = 0; i < PreviousCams.Count; i++)
            {
                if (PreviousCams[i] != null)
                {
                    PreviousCams[i].Priority = 0;
                }
            }
        }
        PreviousCams.Clear();
    }

    private void OnRollPhase()
    {
        CharactersRollCam.gameObject.SetActive(true);
        GloveCam.gameObject.SetActive(true);
    }

    private void OnRollEnd()
    {
        CharactersRollCam.gameObject.SetActive(false);
        GloveCam.gameObject.SetActive(false);
    }

    private void StartTiltAnimation()
    {
        if(TiltCoroutine != null)
        {
            StopCoroutine(TiltCoroutine);
        }
        TiltCoroutine = StartCoroutine(TiltAnimation());
    }

    private void StopTiltAnimation()
    {
        if (TiltCoroutine != null)
        {
            StopCoroutine(TiltCoroutine);
            TiltCoroutine = null;
        }
        MainCamTilt.PanAxis.Value = 0;
    }

    public IEnumerator TiltAnimation()
    {
        MainCamTilt.PanAxis.Value = 0;
        float targetValue = 360f;
        float duration = TiltTimer * targetValue;
        float elapsedTime = 0f;

        while (true)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            MainCamTilt.PanAxis.Value = Mathf.Lerp(0, targetValue, t);

            if (elapsedTime >= duration)
            {
                elapsedTime = 0f;
                MainCamTilt.PanAxis.Value = 0f;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeAllEvents();
        if (instance == this)
        {
            instance = null;
        }
    }
}