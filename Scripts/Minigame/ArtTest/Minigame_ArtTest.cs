using UnityEngine;
using System.Collections.Generic;
using System;

public class Minigame_ArtTest : Minigame
{
    private static Minigame_ArtTest instance;
    public static Minigame_ArtTest Instance => instance;

    public void Awake()
    {
        instance = this;

    }
    private void Start()
    {
        _Interactable.onInteraction += () => { MiniGameManager.Instance.StartMiniGame(MiniGameManager.Instance.arttest); };
    }

    private void SubsribetoInputs(bool subscribe)
    {
        if (subscribe)
        {
            UIInputManager.Instance.OnNavigate += HandleInput;
            UIInputManager.Instance.OnInteract += HandleSubmit;
        }
        else
        {
            UIInputManager.Instance.OnNavigate -= HandleInput;
            UIInputManager.Instance.OnInteract -= HandleSubmit;
        }
    }

    [SerializeField] private LanguageLocalizedSpriteRenderer TestPaperSR;
    [SerializeField] private Interactable _Interactable;

    [SerializeField] private List<ArttestColorObjects> Test1Objects;
    [SerializeField] private int HorizontalObjectIndex;
    [SerializeField] private ArtTestUI UIHandler;

    [SerializeField] private List<ColorCombination> AllColorCombinations;
    public List<ColorCombination> allcolorcombinations => AllColorCombinations;
    [SerializeField] private List<Color> SelectedColor;
    [SerializeField] private int VerticalObjectIndex;

    [SerializeField] private GameObject SpawnPoint;

    [SerializeField] private bool TestResult;
    public bool testresult => TestResult;

    public override void StartMinigame()
    {
        TestResult = false;
        PlayerManager.Instance.player.setstate(PlayerState.None);
        TestPaperSR.UpdateSprite();
        TimeManager.Instance.pauseTimer();
        HorizontalObjectIndex = 0;
        VerticalObjectIndex = 0;
        ResetAllObjects();
        UIHandler.gameObject.SetActive(true);
        UIHandler.ResetUI();
        SubsribetoInputs(true);

        base.StartMinigame();
    }

    private void ResetAllObjects()
    {
        for (int i = 0; i < Test1Objects.Count; i++)
        {
            Test1Objects[i].ResetObject();
        }
    }

    private void HandleInput(Vector2 Input)
    {
        if(Input.x > 0.1f)
        {
            HorizontalObjectIndex++;
            HorizontalObjectIndex = Mathf.Clamp(HorizontalObjectIndex,0, Test1Objects.Count - 1);
        }
        else if (Input.x < -0.1f)
        {
            HorizontalObjectIndex--;
            HorizontalObjectIndex = Mathf.Clamp(HorizontalObjectIndex, 0, Test1Objects.Count - 1);
        }
        if (Input.y > 0.1f)
        {
            VerticalObjectIndex--;
            VerticalObjectIndex = Mathf.Clamp(VerticalObjectIndex, 0, SelectedColor.Count);
        }
        else if (Input.y < -0.1f)
        {
            VerticalObjectIndex++;
            VerticalObjectIndex = Mathf.Clamp(VerticalObjectIndex, 0, SelectedColor.Count);
        }

        UIHandler.HandleUI(Input);
    }
    private void HandleSubmit()
    {
        if(VerticalObjectIndex == 3)
        {
            SubmitTest();
            return;
        }
        else
        {
            ColorObject(HorizontalObjectIndex, SelectedColor[VerticalObjectIndex]);
        }
    }

    private void ColorObject(int index,Color color)
    {
        if (Test1Objects[index] != null)
        {
            Test1Objects[index].ColorObject(color);
        }
    }

    private void SubmitTest()
    {
        bool Success = true;
        for(int i=0;i<Test1Objects.Count;i++)
        {
            if (!Test1Objects[i].correct)
            {
                Success = false;
                break;
            }
        }

        TestResult = Success;

        StopMinigame();
    }

    public override void StopMinigame()
    {
        PlayerManager.Instance.player.setstate(PlayerState.Idle);
        _Interactable.resetInteraction(); 

        if(TestResult)
        {
            PlayerManager.Instance.player.Teleport(SpawnPoint.transform);
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
        }

        TimeManager.Instance.resumeTimer();
        UIHandler.gameObject.SetActive(false);
        SubsribetoInputs(false);

        base.StopMinigame();
    }
}

