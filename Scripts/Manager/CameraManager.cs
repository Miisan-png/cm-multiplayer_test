
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public static CameraManager Instance => instance;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (LastDetector != null)
        {
            LastDetector.activateCam(true);
        }

        TimeManager.Instance.OnDayStateChanged += OnDayStateChanged;

        PauseUIController.Instance.OnPauseStart += () => { PauseCamera.gameObject.SetActive(true); };
        PauseUIController.Instance.OnPauseEnd += () => { PauseCamera.gameObject.SetActive(false); };
        MiniGameManager.Instance.arttest.onMiniGameStart += () => { ArtTestCamera.gameObject.SetActive(true); };
        MiniGameManager.Instance.arttest.onMiniGameEnd += () => { ArtTestCamera.gameObject.SetActive(false); };
        MiniGameManager.Instance.sciencetest.onMiniGameStart += () => { ScienceTestCamera.gameObject.SetActive(true); };
        MiniGameManager.Instance.sciencetest.onMiniGameEnd += () => { ScienceTestCamera.gameObject.SetActive(false); };
    }

    [SerializeField] private Camera PauseCamera;
    public Camera pausecamera => PauseCamera;
    [SerializeField] private Camera ArtTestCamera;
    public Camera arttestcamera => ArtTestCamera;
    [SerializeField] private Camera ScienceTestCamera;
    public Camera sciencetestcamera => ScienceTestCamera;

    [SerializeField] private List<CinemachineCamera> AllCameras;
    [SerializeField] private CinemachineCamera CurrentCam;
    [SerializeField] private CinemachineBrain MainCameraBrain;
    public CinemachineCamera currentcam => CurrentCam;
    [SerializeField] private CameraDetector CurrentDetector;
    public CameraDetector currentdetector => CurrentDetector;

    [SerializeField] private CameraDetector LastDetector;
    [SerializeField] private List<GameObject> ObjectsCached;
    [SerializeField] private GameObject ControlForward;
    public GameObject controlforward => ControlForward;

    [SerializeField] private SpriteRenderer BGRenderer;

    public delegate void cameraChange();
    public event cameraChange onCameraChange;

    public void SwitchtoCam(CinemachineCamera _camera, Sprite BGtoopen, CameraDetector Detector)
    {
        if (ObjectsCached != null)
        {
            for (int i = 0; i < ObjectsCached.Count; i++)
            {
                ObjectsCached[i].SetActive(false);
            }
            ObjectsCached = null;
        }

        for (int i = 0; i < AllCameras.Count; i++)  //reset all camera to low priority
        {  
                AllCameras[i].Priority = 0;

        }

        BGRenderer.sprite = BGtoopen;
        _camera.Priority = 3; //Set Curent Cam to HigherPriority
        CurrentCam = _camera;
        CurrentDetector = Detector;
        ControlForward = null;
        onCameraChange?.Invoke();
    }

    public void SwitchtoCam(CinemachineCamera _camera, Sprite BGtoopen, List<GameObject> Objects,CameraDetector Detector)
    {
        if (ObjectsCached != null)
        {
            for(int i = 0;i< ObjectsCached.Count;i++)
            {
                ObjectsCached[i].SetActive(false);
            }
            ObjectsCached = null;
        }

        for (int i = 0; i < AllCameras.Count; i++)  //reset all camera to low priority
        {
            AllCameras[i].Priority = 0;

        }

        if(Objects != null)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].SetActive(true);
            }

            ObjectsCached = Objects;
        }


        _camera.Priority = 5; //Set Curent Cam to HigherPriority
        BGRenderer.sprite = BGtoopen;
        CurrentCam = _camera;
        CurrentDetector = Detector;
        ControlForward = null;
        onCameraChange?.Invoke();
    }

    public void SwitchtoCam(CinemachineCamera _camera, Sprite BGtoopen, List<GameObject> Objects, CameraDetector Detector,GameObject controlforward)
    {
        ControlForward = null;

        for (int i = 0; i < AllCameras.Count; i++)  //reset all camera to low priority
        {
            AllCameras[i].Priority = 0;

        }

        _camera.Priority = 5; //Set Curent Cam to HigherPriority
        CurrentDetector = Detector;
        ControlForward = controlforward;
        CurrentCam = _camera;

        StartCoroutine(Delay(() => {
            if (ObjectsCached != null)
            {
                for (int i = 0; i < ObjectsCached.Count; i++)
                {
                    ObjectsCached[i].SetActive(false);
                }
                ObjectsCached = null;
            }

            if (Objects != null)
            {
                for (int i = 0; i < Objects.Count; i++)
                {
                    Objects[i].SetActive(true);
                }

                ObjectsCached = Objects;
            }

            BGRenderer.sprite = BGtoopen;
        }));


        onCameraChange?.Invoke();
    }

    private void OnDayStateChanged(DayState State)
    {
        Sprite sprite = BGRenderer.sprite;

        if (CurrentDetector == null) return;

        switch (State)
        {
            case DayState.Morning:
                sprite = currentdetector.bgspritemorning;
                break;
            case DayState.Afternoon:
                sprite = currentdetector.bgspritenoon;
                break;
            case DayState.Night:
                sprite = currentdetector.bgspritenight;
                break;
        }

        if (sprite == null) return;

        BGRenderer.sprite = sprite;
    }

    private IEnumerator Delay(Action A)
    {
        yield return null;
        A();
    }

}
