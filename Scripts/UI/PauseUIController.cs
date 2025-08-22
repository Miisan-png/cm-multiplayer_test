using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Collections;

public class PauseUIController : MonoBehaviour
{
    public static PauseUIController instance;
    public static PauseUIController Instance => instance;


    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        UIInputManager.Instance.OnPause += HandlePause;
        GameManager.Instance.onDayEnd += () => { CanPause = false; };
        UIManager.Instance.onDialogueOn += () => { CanPause = false; };
        UIManager.Instance.onDialogueOff += () => { CanPause = true; };
        UIManager.Instance.onScreenTransitionOn += () => { CanPause = false; };
        UIManager.Instance.onScreenTransitionOff += () => { CanPause = true; };
    }

    private void OnDestroy()
    {
        UnSubsribeEvents();
        UIInputManager.Instance.OnPause -= HandlePause;
        GameManager.Instance.onDayEnd -= () => { CanPause = false; };
        UIManager.Instance.onDialogueOn -= () => { CanPause = false; };
        UIManager.Instance.onDialogueOff -= () => { CanPause = true; };
        UIManager.Instance.onScreenTransitionOn -= () => { CanPause = false; };
        UIManager.Instance.onScreenTransitionOff -= () => { CanPause = true; };
    }

    [SerializeField] private List<RectTransform> SelectionRects;
    [SerializeField] private RectTransform MainArrow;
    [SerializeField] private PauseSelection CurrentSelection;
    [SerializeField] private GameObject PauseMenuParent;
    [SerializeField] private GameObject IDCardCam;
    [SerializeField] private GameObject GloveCam;
    [SerializeField] private GameObject ClockUI;

    private bool CanPause = true;
    private bool Paused;
    private bool InSubMenu;

    public Action OnPauseStart;
    public Action OnPauseEnd;

    private bool ActivateUICondition()
    {
        return GameManager.Instance.gamestate == gameState.Overworld && !Paused && CanPause;
    }

    public void HandlePause()
    {
        if (ActivateUICondition())
        {
            SubsribeEvents();
            PauseMenuParent.SetActive(true);
            ResetController();
            Paused = true;
            TimeManager.Instance.pauseTimer();
            InputManager.Instance.ActivateInputs(false);
            OnPauseStart?.Invoke();
        }
        else if(Paused)
        {
            if(UnPauseCondition())
            {
                StartCoroutine(ExitDelay());
            }
        }
    }
    private bool UnPauseCondition()
    {
        bool condition = true;
        switch (CurrentSelection)
        {
            case PauseSelection.Food:
                break;
            case PauseSelection.Map:
                break;
            case PauseSelection.Inventory:
                break;
            case PauseSelection.Clock:
                break;
            case PauseSelection.Glove:
                condition = MonsterInventoryUIController.Instance != null && !MonsterInventoryUIController.Instance.initialized;
                break;
            case PauseSelection.Dex:
                condition = PokeDexUIController.Instance != null && !PokeDexUIController.Instance.initialized;
                break;
            case PauseSelection.MainMenu:
                condition = ClockUIController.instance != null && !ClockUIController.instance.initialized;
                break;
            case PauseSelection.ID:
                break;
            case PauseSelection.Setting:
                condition = !GameManager.Instance.settingsloaded && SettingsController.Instance == null;
                break;
        }

        return condition;
    }
    private IEnumerator ExitDelay()
    {
        yield return new WaitUntil(() => !InSubMenu);
        UnSubsribeEvents();
        PauseMenuParent.SetActive(false);
        Paused = false;
        TimeManager.Instance.resumeTimer();
        InputManager.Instance.ActivateInputs(true);
        OnPauseEnd?.Invoke();
    }


    private enum PauseSelection
    {
        Food,Map,Inventory,Clock,Glove,Dex,MainMenu,ID,Setting
    }

    private void SubsribeEvents()
    {
        UIInputManager.Instance.OnNavigate += OnNavigate;
        UIInputManager.Instance.OnInteract += OnConfirm;
        UIInputManager.Instance.OnCancel += OnCancel;
    }
    private void UnSubsribeEvents()
    {
        UIInputManager.Instance.OnNavigate -= OnNavigate;
        UIInputManager.Instance.OnInteract -= OnConfirm;
        UIInputManager.Instance.OnCancel -= OnCancel;
    }

    private void ResetController()
    {
        MainArrow.DOAnchorPos(
        new Vector2(
        SelectionRects[(int)CurrentSelection].anchoredPosition.x,
        SelectionRects[(int)CurrentSelection].anchoredPosition.y
        ),
        0f
        );
    }

    private void OnNavigate(Vector2 input)
    {
        if (InSubMenu) return;

        int currentIndex = (int)CurrentSelection;

        if (input.x > 0.1f && currentIndex != 2 && currentIndex != 5)
        {
            currentIndex++;
        }
        else if (input.x < -0.1f && currentIndex != 3 && currentIndex != 6)
        {
            currentIndex--;
        }
        else if (input.y > 0.1f)
        {
            currentIndex -= 3;
        }
        else if (input.y < -0.1f)
        {
            currentIndex += 3;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, SelectionRects.Count - 1);

        CurrentSelection = (PauseSelection)currentIndex;

        MainArrow.DOAnchorPos(
            new Vector2(
                SelectionRects[currentIndex].anchoredPosition.x,
                SelectionRects[currentIndex].anchoredPosition.y
            ),
            0.2f
        );
    }

 

    private void OnConfirm()
    {
        switch (CurrentSelection)
        {
            case PauseSelection.Food:
                break;
            case PauseSelection.Map:
                break;
            case PauseSelection.Inventory:
                break;
            case PauseSelection.Clock:
                if (ClockUIController.Instance != null && !ClockUIController.Instance.initialized && TimeManager.Instance.state != DayState.Night)
                {
                    ClockUIController.Instance.InitializeUI();
                    MainArrow.gameObject.SetActive(false);
                    InSubMenu = true;
                    ClockUI.SetActive(true);
                }
                break;
            case PauseSelection.Glove:
                if(MonsterInventoryUIController.Instance != null && !MonsterInventoryUIController.Instance.initialized)
                {
                    GloveCam.SetActive(true);
                    MonsterInventoryUIController.Instance.InitializeInventoryUI();
                    MainArrow.gameObject.SetActive(false);
                    InSubMenu = true;
                    ClockUI.SetActive(false);
                }
                break;
            case PauseSelection.Dex:
                if (PokeDexUIController.Instance != null && !PokeDexUIController.Instance.initialized && PokeDexUIController.Instance.InitializeDexCondition())
                {
                    PokeDexUIController.Instance.InitializeDex();
                    MainArrow.gameObject.SetActive(false);
                    InSubMenu = true;
                    ClockUI.SetActive(false);
                }
                break;
            case PauseSelection.MainMenu:
                if(ExitMainMenuUI.Instance != null && !ExitMainMenuUI.Instance.initialized)
                {
                    ExitMainMenuUI.Instance.InitializeUI();
                    MainArrow.gameObject.SetActive(false);
                    InSubMenu = true;
                    ClockUI.SetActive(false);
                }
                break;
            case PauseSelection.ID:
                IDCardCam.SetActive(true);
                MainArrow.gameObject.SetActive(false);
                InSubMenu = true;
                ClockUI.SetActive(false);
                break;
            case PauseSelection.Setting:
                if (!GameManager.Instance.settingsloaded)
                {
                    InSubMenu = true;
                    GameManager.Instance.LoadSettingScene(() => { MainArrow.gameObject.SetActive(false); ClockUI.SetActive(false); });               
                }
                break;
        }
    }

    private void OnCancel()
    {
        if(!InSubMenu)
        {
            HandlePause();
            return;
        }

        ExitSubMenu();
    }

    private bool ExitSubMenuCondition()
    {
        bool condition = true;
        switch (CurrentSelection)
        {
            case PauseSelection.Food:
                break;
            case PauseSelection.Map:
                break;
            case PauseSelection.Inventory:
                break;
            case PauseSelection.Clock:
                condition = ClockUIController.instance != null && ClockUIController.instance.initialized;
                break;
            case PauseSelection.Glove:
                condition = MonsterInventoryUIController.Instance != null && MonsterInventoryUIController.Instance.initialized && MonsterInventoryUIController.Instance.currentsection == MonsterInventoryUIController.InventorySection.Glove && MonsterInventoryUIController.Instance.selectedmonster == null;
                break;
            case PauseSelection.Dex:
                condition = PokeDexUIController.Instance != null && PokeDexUIController.Instance.initialized;
                break;
            case PauseSelection.MainMenu:
                condition = ExitMainMenuUI.Instance != null && ExitMainMenuUI.instance.initialized;
                break;
            case PauseSelection.ID:
                break;
            case PauseSelection.Setting:
                condition = GameManager.Instance.settingsloaded && SettingsController.Instance != null && SettingsController.Instance.currentsection == SettingsController.SettingsSection.Main;
                break;
        }

        return condition;
    }
    public void ExitSubMenu()
    {
        if (!ExitSubMenuCondition()) return;

        switch (CurrentSelection)
        {
            case PauseSelection.Food:
                break;
            case PauseSelection.Map:
                break;
            case PauseSelection.Inventory:
                break;
            case PauseSelection.Clock:
                ClockUIController.Instance.UninitializeUI();
                MainArrow.gameObject.SetActive(true);
                InSubMenu = false;
                ClockUI.SetActive(true);
                break;
            case PauseSelection.Glove:
                MonsterInventoryUIController.Instance.UnInitializeInventoryUI(() => {
                    MainArrow.gameObject.SetActive(true);
                    GloveCam.SetActive(false);
                    InSubMenu = false;
                    ClockUI.SetActive(true);
                });
                break;
            case PauseSelection.Dex:
                PokeDexUIController.Instance.UnintializeDex(() => {
                    GloveCam.SetActive(false);
                    MainArrow.gameObject.SetActive(true);
                    InSubMenu = false;
                    ClockUI.SetActive(true);
                });
                break;
            case PauseSelection.MainMenu:
                ExitMainMenuUI.instance.UninitializeUI();
                MainArrow.gameObject.SetActive(true);
                InSubMenu = false;
                ClockUI.SetActive(true);
                break;
            case PauseSelection.ID:
                IDCardCam.SetActive(false);
                MainArrow.gameObject.SetActive(true);
                InSubMenu = false;
                ClockUI.SetActive(true);
                break;
            case PauseSelection.Setting:
                GameManager.Instance.UnloadSettingScene(() => {
                    MainArrow.gameObject.SetActive(true);
                    InSubMenu = false;
                    ClockUI.SetActive(true);
                });
                break;
        }
    }
}
