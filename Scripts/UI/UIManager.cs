using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private AudioListener cameraAudioListener;
    [SerializeField] private List<GameObject> OverworldSceneObjs;
    [SerializeField] private GameObject DayUI;
    [SerializeField] private GameObject DialogueUI;
    [SerializeField] private GameObject ScreenTransitionUI;
    [SerializeField] private GameObject PauseUI;

    public delegate void dialogueOn();
    public event dialogueOn onDialogueOn;

    public delegate void dialogueOff();
    public event dialogueOff onDialogueOff;

    public delegate void ScreenTransitionOn();
    public event ScreenTransitionOn onScreenTransitionOn;

    public delegate void ScreenTransition();
    public event ScreenTransition duringScreenTransition;

    [SerializeField] private Animator ScreenTransitionAnimator;
    public delegate void ScreenTransitionOff();
    public event ScreenTransitionOff onScreenTransitionOff;

    public delegate void dayOn();
    public event dayOn onDayOn;

    public delegate void dayOff();
    public event dayOff onDayOff;

    public delegate void pauseOn();
    public event pauseOn onPauseOn;

    public delegate void pauseOff();
    public event pauseOff onPauseOff;

    private Action duringscreentransition;
    private Action screentransitionend;

    private void Start()
    {
        GameManager.Instance.onDayStart += HandleDayStart;
        GameManager.Instance.OnGameStateChange += HandleGameStateChange;
        TimeManager.Instance.on8PM += Handle8PM;
        PauseUIController.Instance.OnPauseStart += HandlePauseStart;
        PauseUIController.Instance.OnPauseEnd += HandlePauseEnd;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onDayStart -= HandleDayStart;
        GameManager.Instance.OnGameStateChange -= HandleGameStateChange;
        TimeManager.Instance.on8PM -= Handle8PM;
        PauseUIController.Instance.OnPauseStart -= HandlePauseStart;
        PauseUIController.Instance.OnPauseEnd -= HandlePauseEnd;
    }

    private void HandleDayStart()
    {
        OpenMenubyName("DayUI");
    }

    private void HandleGameStateChange(gameState State)
    {
        switch (State)
        {
            case gameState.Overworld:
                for (int i = 0; i < OverworldSceneObjs.Count; i++)
                {
                    OverworldSceneObjs[i].SetActive(true);
                }
                cameraAudioListener.enabled = true;
                break;
            case gameState.Battle:
                for (int i = 0; i < OverworldSceneObjs.Count; i++)
                {
                    OverworldSceneObjs[i].SetActive(false);
                }
                cameraAudioListener.enabled = false;
                break;
            case gameState.Minigame:
                for (int i = 0; i < OverworldSceneObjs.Count; i++)
                {
                    OverworldSceneObjs[i].SetActive(false);
                }
                cameraAudioListener.enabled = false;
                break;
            case gameState.MainMenu:
                for(int i=0;i<OverworldSceneObjs.Count;i++)
                {
                    OverworldSceneObjs[i].SetActive(false);
                }
                cameraAudioListener.enabled = false;
                break;
        }
    }

    private void Handle8PM()
    {
        CloseMenubyName("DayUI");
    }

    private void HandlePauseStart()
    {
        OpenMenubyName("PauseUI");
    }

    private void HandlePauseEnd()
    {
        CloseMenubyName("PauseUI");
    }

    public void OpenMenubyName(string _name)
    {
        switch (_name)
        {
            case "DialogueUI":
                onDialogueOn?.Invoke();
                DialogueUI.SetActive(true);
                break;

            case "DayUI":
                onDayOn?.Invoke();
                DayUI.SetActive(true);
                break;

            case "ScreenTransitionUI":
                onScreenTransitionOn?.Invoke();
                ScreenTransitionUI.SetActive(true);
                break;

            case "PauseUI":
                onPauseOn?.Invoke();
                PauseUI.SetActive(true);
                break;
        }
    }

    public void CloseMenubyName(string _name)
    {
        switch (_name)
        {
            case "DialogueUI":
                onDialogueOff?.Invoke();
                DialogueUI.SetActive(false);
                break;

            case "DayUI":
                onDayOff?.Invoke();
                DayUI.SetActive(false);
                break;

            case "ScreenTransitionUI":
                onScreenTransitionOn -= () => { duringscreentransition(); };
                onScreenTransitionOff?.Invoke();
                ScreenTransitionUI.SetActive(false);
                break;
            case "PauseUI":
                onPauseOff?.Invoke();
                PauseUI.SetActive(false);
                break;
        }
    }

    public void sendSignal()
    {
        duringScreenTransition?.Invoke();
        duringscreentransition = null;
    }

    public void onScreenTransitionEnded()
    {
        screentransitionend?.Invoke();
        screentransitionend = null;
    }

    public void startFade(float delay, Action F)
    {
        StartCoroutine(this.delay(delay, () => {
            OpenMenubyName("ScreenTransitionUI");
            duringscreentransition = F;
            duringScreenTransition += () => { duringscreentransition(); };
        }));
    }

    public void endFade()
    {
        ScreenTransitionAnimator.SetTrigger("EndFade");
    }

    public void endFade(Action A)
    {
        ScreenTransitionAnimator.SetTrigger("EndFade");
        screentransitionend = A;
    }

    public IEnumerator delay(float y, Action F)
    {
        yield return new WaitForSeconds(y);
        F();
    }
}
