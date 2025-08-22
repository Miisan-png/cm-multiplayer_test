using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    private static MainMenuUIController instance;
    public static MainMenuUIController Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void InitializeUI()
    {
        CurrentSection = MainMenuSection.Title;
        UIInputManager.Instance.OnNavigate += OnNavigate;
        UIInputManager.Instance.OnCancel += OnCancel;
        UIInputManager.Instance.OnInteract += OnConfirm;

        OnEnterSection();

        TitleScreenText.gameObject.SetActive(true);
        TitleScreenTweener = TitleScreenText.DOColor(new Color(0,0,0,0.7f),1f).SetEase(Ease.InOutSine).SetLoops(-1,LoopType.Yoyo);

        Initialized = true;
    }

    private void OnDestroy()
    {
        UIInputManager.Instance.OnNavigate -= OnNavigate;
        UIInputManager.Instance.OnCancel -= OnCancel;
        UIInputManager.Instance.OnInteract -= OnConfirm;
        instance = null;

        if(TitleScreenTweener != null)
        {
            TitleScreenTweener.Complete();
            TitleScreenTweener.Kill();
            TitleScreenTweener = null;
        }
    }
    private bool Initialized;

    [SerializeField] private GameObject EventSystem;
    [SerializeField] private AudioListener CameraAudioSource;

    [SerializeField] private GameObject MainUIParent;
    [SerializeField] private List<RectTransform> MainMenuRect;
    [SerializeField] private List<RectTransform> LoadMenuRect;
    [SerializeField] private List<RectTransform> ConfirmationMenuRect;

    [SerializeField] private List<LoadCardUI> LoadCardsUI;

    [SerializeField] private TextMeshProUGUI TitleScreenText;
    [SerializeField] private GameObject TitleScreenParent;
    [SerializeField] private GameObject MainMenuParent;
    [SerializeField] private GameObject LoadParent;
    [SerializeField] private GameObject ConfirmationParent;
    [SerializeField] private GameObject NewGameProfileParent;
    [SerializeField] private TextMeshProUGUI ConfirmationText;
    [SerializeField] private TextMeshProUGUI ContinueText;

    [SerializeField] private TMP_InputField NameInputField;
    [SerializeField] private playerGender CurrentGender;
    [SerializeField] private List<RectTransform> GenderRects;
    [SerializeField] private List<GameObject> GenderSelected;
    [SerializeField] private GameObject MalePanel;
    [SerializeField] private GameObject FemalePanel;
    [SerializeField] private RectTransform NameInputRect;
    [SerializeField] private RectTransform CharacterConfirmRect;
    [SerializeField] private GameObject NameErrorObject;

    [SerializeField] private Image MaleCreationImage;
    [SerializeField] private Image FemaleCreationImage;
    [SerializeField] private List<Sprite> MaleCreationSprites;
    [SerializeField] private List<Sprite> FemaleCreationSprites;

    private Action ConfirmationAction;

    [SerializeField] private int MainMenuIndex;
    [SerializeField] private int LoadMenuIndex;
    [SerializeField] private int ConfirmationMenuIndex;
    [SerializeField] private int CharacterCreationIndex;

    [SerializeField] private RectTransform MainMenuArrow;
    [SerializeField] private RectTransform LoadMenuArrow;
    [SerializeField] private RectTransform ConfirmationMenuArrow;
    [SerializeField] private RectTransform CreationArrow;

    [SerializeField] private MainMenuSection LastSection;
    [SerializeField] private MainMenuSection CurrentSection;

    private Coroutine MainMenuStartCoroutine;
    private Coroutine MainMenuStopCoroutine;

    private Tweener TitleScreenTweener;

    public enum MainMenuSection
    {
        Title, MainMenu, LoadScreen, CharacterCreation ,Options,Confirmation,None
    }

    private void OnNavigate(Vector2 Input)
    {
        switch (CurrentSection)
        {
            case MainMenuSection.Title:
                break;
            case MainMenuSection.MainMenu:
                if (Input.x > 0.1f)
                {
                    MainMenuIndex++;            
                }
                else if (Input.x < -0.1f)
                {
                    MainMenuIndex--;

                }
                else if (Input.y < -0.1f)
                {
                    MainMenuIndex += 2;
                }
                else if (Input.y > 0.1f)
                {
                    MainMenuIndex-= 2;
                }
                MainMenuIndex = Mathf.Clamp(MainMenuIndex, 0, 4);
                MainMenuArrow.DOAnchorPos(new Vector2(MainMenuRect[MainMenuIndex].anchoredPosition.x, MainMenuRect[MainMenuIndex].anchoredPosition.y), 0.2f);
                break;
            case MainMenuSection.LoadScreen:
                if (Input.x > 0.1f)
                {
                    LoadMenuIndex++;
                }
                else if (Input.x < -0.1f)
                {
                    LoadMenuIndex--;
                }
                LoadMenuIndex = Mathf.Clamp(LoadMenuIndex, 0, 2);
                LoadMenuArrow.DOAnchorPos(new Vector2(LoadMenuRect[LoadMenuIndex].anchoredPosition.x, LoadMenuRect[LoadMenuIndex].anchoredPosition.y), 0.2f);
                break;
            case MainMenuSection.CharacterCreation:
                switch(CharacterCreationIndex)
                {
                    case 0:
                        if (Input.x > 0.1f)
                        {
                            CurrentGender = playerGender.Female;
                            CreationArrow.DOAnchorPos(new Vector2(GenderRects[1].anchoredPosition.x, GenderRects[1].anchoredPosition.y), 0.2f);
                        }
                        else if (Input.x < -0.1f)
                        {
                            CurrentGender = playerGender.Male;  
                            CreationArrow.DOAnchorPos(new Vector2(GenderRects[0].anchoredPosition.x, GenderRects[0].anchoredPosition.y), 0.2f);
                        }

                        GenderSelected[1].SetActive(CurrentGender == playerGender.Female);
                        GenderSelected[0].SetActive(CurrentGender == playerGender.Male);
                        break;
                }
                break;
            case MainMenuSection.Options:
                break;
            case MainMenuSection.Confirmation:
                if (Input.x > 0.1f)
                {
                    ConfirmationMenuIndex++;
                }
                else if (Input.x < -0.1f)
                {
                    ConfirmationMenuIndex--;
                }
                ConfirmationMenuIndex = Mathf.Clamp(ConfirmationMenuIndex, 0, 1);
                ConfirmationMenuArrow.DOAnchorPos(new Vector2(ConfirmationMenuRect[ConfirmationMenuIndex].anchoredPosition.x, ConfirmationMenuRect[ConfirmationMenuIndex].anchoredPosition.y), 0.2f);
                break;
        }
    }

    private void OnCancel()
    {
        switch (CurrentSection)
        {
            case MainMenuSection.Title:

                break;
            case MainMenuSection.MainMenu:
                if (MainMenuStopCoroutine != null) return;
                MainMenuStopCoroutine = StartCoroutine(MainMenuEnd());
                return;
            case MainMenuSection.LoadScreen:
                CurrentSection = MainMenuSection.MainMenu;
                break;
            case MainMenuSection.CharacterCreation:
                switch (CharacterCreationIndex)
                {
                    case 0:
                        CurrentSection = MainMenuSection.LoadScreen;                     
                        break;
                    case 1:
                        if (NameInputField.isFocused) return;
                        NameInputField.DeactivateInputField();
                        NameInputField.interactable = false;
                        break;
                }
                break;
            case MainMenuSection.Options:
                if (SettingsController.Instance.currentsection != SettingsController.SettingsSection.Main) return;
                GameManager.Instance.UnloadSettingScene(() => {
                    CurrentSection = MainMenuSection.MainMenu;
                    MainUIParent.SetActive(true);
                });
                break;
            case MainMenuSection.Confirmation:
                CurrentSection = LastSection;
                break;
        }

        OnEnterSection();
    }
    public void OnConfirm()
    {
        switch (CurrentSection)
        {
            case MainMenuSection.MainMenu:
                LastSection = CurrentSection;

                switch (MainMenuIndex)
                {
                    case 0:
                        if(SaveLoadManager.Instance.LoadSlotInfo(SettingsManager.Instance.data.CurrrentSaveSlot) != null)
                        {
                            CurrentSection = MainMenuSection.Confirmation;
                            ConfirmationMenuIndex = 0;
                            ConfirmationText.font = SettingsManager.Instance.GetLocalizedFont();

                            string confirmation = "Continue from previous save?";

                            switch (SettingsManager.Instance.data.Language)
                            {
                                case GameLanguage.Japanese:
                                    confirmation = "前回のセーブから続きますか?";
                                    break;
                                case GameLanguage.Mandarin:
                                    confirmation = "从上次存档继续?";
                                    break;
                            }

                            ConfirmationText.text = confirmation;
                            ConfirmationAction = () => {
                                CurrentSection = MainMenuSection.None;
                                ConfirmationParent.SetActive(false);
                                GameManager.Instance.LoadOverworldScene(() => {
                                    UIManager.Instance.startFade(0f, () => {
                                        GameManager.Instance.UnloadMainMenuScene(() => {
                                            GameManager.Instance.StartGame();
                                            GameManager.Instance.startDay();
                                            UIManager.Instance.endFade();
                                        });
                                    });
                                });
                            };
                        }
                        break;
                    case 1:
                        CurrentSection = MainMenuSection.LoadScreen;
                        break;
                    case 2:
                        CurrentSection = MainMenuSection.LoadScreen;
                        break;
                    case 3:  
                        GameManager.Instance.LoadSettingScene(() => {
                            CurrentSection = MainMenuSection.Options;
                            MainUIParent.SetActive(false);
                        });
                        break;
                    case 4:
                        CurrentSection = MainMenuSection.Confirmation;
                        ConfirmationMenuIndex = 0;
                        ConfirmationText.font = SettingsManager.Instance.GetLocalizedFont();

                        string confirmationstring = "Quit Game?";

                        switch (SettingsManager.Instance.data.Language)
                        {
                            case GameLanguage.Japanese:
                                confirmationstring = "ゲームを終了?";
                                break;
                            case GameLanguage.Mandarin:
                                confirmationstring = "退出游戏?";
                                break;
                        }

                        ConfirmationText.text = confirmationstring;

                        ConfirmationAction = Application.Quit;
                        break;
                }
                break;
            case MainMenuSection.LoadScreen:
                LastSection = CurrentSection;

                if (MainMenuIndex == 1)
                {
                    CurrentSection = MainMenuSection.CharacterCreation;
                }
                else if (SaveLoadManager.Instance.LoadSlotInfo(LoadMenuIndex + 1) != null)
                {
                    CurrentSection = MainMenuSection.Confirmation;
                    ConfirmationMenuIndex = 0;
                    ConfirmationText.font = SettingsManager.Instance.GetLocalizedFont();

                    string confirmationstring = $"Continue with Slot {LoadMenuIndex + 1}?";

                    switch (SettingsManager.Instance.data.Language)
                    {
                        case GameLanguage.Japanese:
                            confirmationstring = $"スロット{LoadMenuIndex + 1}から続きますか?";
                            break;
                        case GameLanguage.Mandarin:
                            confirmationstring = $"从存档位{LoadMenuIndex + 1}继续吗?";
                            break;
                    }

                    ConfirmationText.text = confirmationstring;

                    ConfirmationAction = () => {
                        CurrentSection = MainMenuSection.None;
                        SaveLoadManager.Instance.CurrentSaveSlot = LoadMenuIndex + 1;
                        ConfirmationParent.SetActive(false);
                        GameManager.Instance.LoadOverworldScene(() => {
                            UIManager.Instance.startFade(0f, () => {
                                GameManager.Instance.UnloadMainMenuScene(() => {
                                    SaveLoadManager.Instance.AutoSave();
                                    GameManager.Instance.StartGame();
                                    GameManager.Instance.startDay();
                                    UIManager.Instance.endFade();
                                });
                            });
                        });
                    };
                }

                 break;
            case MainMenuSection.CharacterCreation:
                switch(CharacterCreationIndex)
                {
                    case 0:
                        CharacterCreationIndex++;
                        CharacterCreationIndex = Mathf.Clamp(CharacterCreationIndex, 0, 2);
                        NameInputField.ActivateInputField();
                        NameInputField.interactable = true;
                        CreationArrow.DOAnchorPos(new Vector2(NameInputRect.anchoredPosition.x, NameInputRect.anchoredPosition.y), 0.2f);

                        MalePanel.SetActive(CurrentGender == playerGender.Male);
                        FemalePanel.SetActive(CurrentGender == playerGender.Female);

                        MaleCreationImage.sprite = MaleCreationSprites[1];
                        FemaleCreationImage.sprite = FemaleCreationSprites[1];
                        return;
                    case 1:
                        if (NameInputField.isFocused) return;
                        CharacterCreationIndex++;
                        CharacterCreationIndex = Mathf.Clamp(CharacterCreationIndex, 0, 2);
                        NameInputField.DeactivateInputField();
                        NameInputField.interactable = false;
                        CreationArrow.DOAnchorPos(new Vector2(CharacterConfirmRect.anchoredPosition.x, CharacterConfirmRect.anchoredPosition.y), 0.2f);
                        return;
                    case 2:
                        if (NameInputField.text != null && NameInputField.text.Length > 0)
                        {
                            LastSection = CurrentSection;
                            ConfirmationMenuIndex = SaveLoadManager.Instance.LoadSlotInfo(LoadMenuIndex + 1) != null ? 1 : 0;
                            CurrentSection = MainMenuSection.Confirmation;

                            ConfirmationText.font = SettingsManager.Instance.GetLocalizedFont();

                            string extratext = SaveLoadManager.Instance.LoadSlotInfo(LoadMenuIndex + 1) != null ? "\n(Existing progress will be lost.)" : "";
                            string confirm = $"Start new game in Slot {LoadMenuIndex + 1}?";

                            switch (SettingsManager.Instance.data.Language)
                            {
                                case GameLanguage.Japanese:
                                    confirm = $"スロット{LoadMenuIndex + 1}で新しいゲームを開始しますか?";
                                    extratext = SaveLoadManager.Instance.LoadSlotInfo(LoadMenuIndex + 1) != null ? "\n(既存のセーブデータは消去されます)" : "";
                                    break;
                                case GameLanguage.Mandarin:
                                    confirm = $"在存档位{LoadMenuIndex + 1}开始新游戏?";
                                    extratext = SaveLoadManager.Instance.LoadSlotInfo(LoadMenuIndex + 1) != null ? "\n(现有进度将会丢失)" : "";
                                    break;
                            }

                            ConfirmationText.text = confirm + extratext;

                            ConfirmationAction = () => {
                                CurrentSection = MainMenuSection.None;
                                SaveLoadManager.Instance.DeleteSaveSlot(LoadMenuIndex + 1);
                                SaveLoadManager.Instance.CurrentSaveSlot = LoadMenuIndex + 1;
                                ConfirmationParent.SetActive(false);
                                GameManager.Instance.LoadOverworldScene(() => {
                                    UIManager.Instance.startFade(0f, () => {
                                        GameManager.Instance.UnloadMainMenuScene(() => {
                                            SaveSlotInfo slotinfo = new SaveSlotInfo(NameInputField.text, 0, 0, CurrentGender, CalendarDay.Monday);
                                            SaveLoadManager.Instance.SaveSlotInfo(slotinfo, LoadMenuIndex + 1);
                                            SaveLoadManager.Instance.AutoSave();
                                            GameManager.Instance.StartGame();
                                            GameManager.Instance.startDay();
                                            UIManager.Instance.endFade();
                                        });
                                    });
                                });
                            };
                        }
                        else
                        {
                            NameErrorObject.SetActive(true);
                            DOVirtual.DelayedCall(2f, () => NameErrorObject.SetActive(false));
                            CharacterCreationIndex = 1;
                            NameInputField.ActivateInputField();
                            NameInputField.interactable = true;
                            CreationArrow.DOAnchorPos(new Vector2(NameInputRect.anchoredPosition.x, NameInputRect.anchoredPosition.y), 0.2f);
                            return;
                        }
                         break;
                }
                break;
            case MainMenuSection.Options:
                LastSection = CurrentSection;

                break;
            case MainMenuSection.Confirmation:
                switch(ConfirmationMenuIndex)
                {
                    case 0:
                        ConfirmationAction?.Invoke();
                        break;
                    case 1:
                        CurrentSection = LastSection;
                        break;
                }
                break;
        }

        OnEnterSection();
    }

    private void OnEnterSection()
    {
        switch (CurrentSection)
        {
            case MainMenuSection.Title:
                ConfirmationAction = null;
                TitleScreenParent.SetActive(true);
                MainMenuParent.SetActive(false);
                ConfirmationParent.SetActive(false);
                break;
            case MainMenuSection.MainMenu:
                ConfirmationAction = null;
                MainMenuArrow.DOAnchorPos(new Vector2(MainMenuRect[MainMenuIndex].anchoredPosition.x, MainMenuRect[MainMenuIndex].anchoredPosition.y), 0f);
                TitleScreenParent.SetActive(false);
                LoadParent.SetActive(false);
                ConfirmationParent.SetActive(false);
                MainMenuParent.SetActive(true);

                if (SaveLoadManager.Instance.LoadSlotInfo(SettingsManager.Instance.data.CurrrentSaveSlot) != null)
                {
                    ContinueText.color = Color.black;
                }
                else
                {
                    ContinueText.color = Color.gray;
                }
                NameInputField.onValueChanged.RemoveAllListeners();

                break;
            case MainMenuSection.LoadScreen:
                foreach (LoadCardUI UI in LoadCardsUI)
                {
                    UI.InitializeUI();
                }
                ConfirmationAction = null;
                ConfirmationParent.SetActive(false);
                MainMenuParent.SetActive(false);
                LoadParent.SetActive(true);
                NewGameProfileParent.SetActive(false);
                LoadMenuArrow.DOAnchorPos(new Vector2(LoadMenuRect[LoadMenuIndex].anchoredPosition.x, LoadMenuRect[LoadMenuIndex].anchoredPosition.y), 0f);
                NameInputField.onValueChanged.RemoveListener(UpdateInputFieldCharacters);
                NameInputField.text = "Bendi";

                break;
            case MainMenuSection.CharacterCreation:
                MaleCreationImage.sprite = MaleCreationSprites[0];
                FemaleCreationImage.sprite = FemaleCreationSprites[0];

                ConfirmationAction = null;
                ConfirmationParent.SetActive(false);
                MainMenuParent.SetActive(false);
                LoadParent.SetActive(false);
                NewGameProfileParent.SetActive(true);
                CurrentGender = playerGender.Male;
                GenderSelected[1].SetActive(false);
                GenderSelected[0].SetActive(true);
                CharacterCreationIndex = 0;
                CreationArrow.DOAnchorPos(new Vector2(GenderRects[0].anchoredPosition.x, GenderRects[0].anchoredPosition.y), 0f);
                NameInputField.fontAsset = SettingsManager.Instance.GetLocalizedFont();
                NameInputField.onValueChanged.AddListener(UpdateInputFieldCharacters);
                MalePanel.SetActive(true);
                FemalePanel.SetActive(true);
                break;
            case MainMenuSection.Options:
                ConfirmationAction = null;

                break;
            case MainMenuSection.Confirmation:
                ConfirmationMenuArrow.DOAnchorPos(new Vector2(ConfirmationMenuRect[ConfirmationMenuIndex].anchoredPosition.x, ConfirmationMenuRect[ConfirmationMenuIndex].anchoredPosition.y), 0f);
                ConfirmationParent.SetActive(true);
                break;
        }

        EventSystem.SetActive(CurrentSection != MainMenuSection.None);
        CameraAudioSource.enabled = CurrentSection != MainMenuSection.None;

        UIInputManager.Instance.ClearInputs();
    }

    private void UpdateInputFieldCharacters(string newText)
    {
        if (NameInputField.fontAsset != null && !string.IsNullOrEmpty(newText))
        {
            // Try to add missing characters to the font asset
            bool addedChars = NameInputField.fontAsset.TryAddCharacters(newText);

            // Optional: Log if characters were added
            if (addedChars)
            {
                Debug.Log("Added new characters to font asset");
            }
            else
            {
                Debug.Log("Did not add new characters to font aaset");
            }
        }
    }

    private void Update()
    {
        if (Initialized && Input.anyKeyDown && CurrentSection == MainMenuSection.Title)
        {
            if (MainMenuStartCoroutine != null) return;

            MainMenuStartCoroutine = StartCoroutine(MainMenuStart());
        }
    }

    private IEnumerator MainMenuStart()
    {
        yield return new WaitForSeconds(0.5f);
 
        LastSection = CurrentSection;
        CurrentSection = MainMenuSection.MainMenu;
        OnEnterSection();
        MainMenuStartCoroutine = null;
    }

    private IEnumerator MainMenuEnd()
    {
        yield return new WaitForSeconds(0.5f);
        CurrentSection = MainMenuSection.Title;
        OnEnterSection();
        MainMenuStopCoroutine = null;
    }
}
