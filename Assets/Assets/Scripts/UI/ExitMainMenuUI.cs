using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ExitMainMenuUI : MonoBehaviour
{
    public static ExitMainMenuUI instance;
    public static ExitMainMenuUI Instance => instance;


    private void Awake()
    {
        instance = this;
    }

    private bool Initialized;
    public bool initialized => Initialized;

    [SerializeField] private GameObject MainUIParent;

    [SerializeField] private List<RectTransform> NavigationRects;
    [SerializeField] private RectTransform SelectionArrow;
    private int NavigationIndex;

    public void InitializeUI()
    {
        Initialized = true;
        SubscribetoInputs();
        MainUIParent.SetActive(true);

        NavigationIndex = 1;
        SelectionArrow.DOAnchorPos(new Vector2(NavigationRects[NavigationIndex].anchoredPosition.x, NavigationRects[NavigationIndex].anchoredPosition.y), 0f);
    }

    private void SubscribetoInputs()
    {
        UIInputManager.Instance.OnNavigate += OnNavigate;
        UIInputManager.Instance.OnInteract += OnConfirm;
    }

    private void OnDestroy()
    {
        UnSubscribetoInputs();
    }

    private void UnSubscribetoInputs()
    {
        UIInputManager.Instance.OnNavigate -= OnNavigate;
        UIInputManager.Instance.OnInteract -= OnConfirm;
    }

    private void OnNavigate(Vector2 input)
    {
        if(input.x > 0.1f)
        {
            NavigationIndex++;
        }
        else if(input.x < -0.1f)
        {
            NavigationIndex--;
        }
        NavigationIndex = Mathf.Clamp(NavigationIndex, 0, 1);

        SelectionArrow.DOAnchorPos(new Vector2(NavigationRects[NavigationIndex].anchoredPosition.x, NavigationRects[NavigationIndex].anchoredPosition.y), 0.2f);
    }

    private void OnConfirm()
    {
        switch(NavigationIndex)
        {
            case 0:
                UIManager.Instance.startFade(0f, () =>
                {
                    GameManager.Instance.LoadMainMenuScene(() =>
                    {
                        UIManager.Instance.endFade(() =>
                        {
                            GameManager.Instance.UnloadOverworldScene(() =>
                            {
                                MainMenuUIController.Instance.InitializeUI();
                            });
                        });
                    });
                });
                UnSubscribetoInputs();
                break;
            case 1:
                PauseUIController.instance.ExitSubMenu();
                break;
        }
    }


    public void UninitializeUI()
    {
        Initialized = false;
        UnSubscribetoInputs();

        MainUIParent.SetActive(false);
    }
}
