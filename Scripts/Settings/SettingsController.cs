using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    private static SettingsController instance;
    public static SettingsController Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UIInputManager.Instance.OnNavigate += OnNavigate;
        UIInputManager.Instance.OnInteract += OnConfirm;
        UIInputManager.Instance.OnCancel += OnCancel;
        StartCoroutine(NavigateDelay());
        Volume1.SetValueWithoutNotify(SettingsManager.Instance.data.MusicVol);
        Volume2.SetValueWithoutNotify(SettingsManager.Instance.data.AmbienceVol);
        Volume3.SetValueWithoutNotify(SettingsManager.Instance.data.EffectVol);
    }

    private IEnumerator NavigateDelay()
    {
        yield return null;
        OnNavigate(new Vector2(0, 1f));
    }

    private void OnDestroy()
    {
        UIInputManager.Instance.OnNavigate -= OnNavigate;
        UIInputManager.Instance.OnInteract -= OnConfirm;
        UIInputManager.Instance.OnCancel -= OnCancel;
        DOTween.Kill(MainArrow);
        DOTween.Kill(SubArrow);

        instance = null;
    }

    [SerializeField] private TextMeshProUGUI DialogFont;

    [SerializeField] private RectTransform MainArrow;
    [SerializeField] private RectTransform SubArrow;
    [SerializeField] private RectTransform ResetTransform;

    [SerializeField] private List<GameObject> SubObjectParent;
    [SerializeField] private Slider Volume1;
    [SerializeField] private Slider Volume2;
    [SerializeField] private Slider Volume3;

    [SerializeField] private List<SettingsButton> SubButtonsRes;
    [SerializeField] private List<SettingsButton> SubButtonsFps;
    [SerializeField] private List<SettingsButton> SubButtonsLang;
    [SerializeField] private List<SettingsButton> SubButtonsDialog;
    [SerializeField] private List<SettingsButton> SubButtonsMus;
    [SerializeField] private List<SettingsButton> SubButtonsAmb;
    [SerializeField] private List<SettingsButton> SubButtonsEff;

    [SerializeField] private List<SettingsButton> CurrentSubButtons;
    [SerializeField] private int HoveredSubButton;

    [SerializeField] private SettingsButton CurrentButton;
    public SettingsSection currentsection => CurrentSection;
    [SerializeField] private SettingsSection CurrentSection;

    [SerializeField] private List<RectTransform> SectionTransform;
    [SerializeField] private SettingsSection HoveredSection;
    public enum SettingsSection
    {
        Main, Resolution, FPS, Language, DialogColor, Music, Ambience, Effect, Reset
    }

    private void OnNavigate(Vector2 Input)
    {
        if (CurrentSection == SettingsSection.Main)
        {
            if (Input.y > 0.1f)
            {
                HoveredSection--;
                int current = (int)HoveredSection;
                current = Mathf.Clamp(current, 1, 8);
                HoveredSection = (SettingsSection)current;
                MainArrow.DOAnchorPos(new Vector2(SectionTransform[current - 1].anchoredPosition.x, SectionTransform[current - 1].anchoredPosition.y + 10f), 0.2f).SetDelay(0.01f);
            }
            else if (Input.y < -0.1f)
            {
                HoveredSection++;
                int current = (int)HoveredSection;
                current = Mathf.Clamp(current, 1, 8);
                HoveredSection = (SettingsSection)current;

                if (HoveredSection == SettingsSection.Reset)
                {
                    MainArrow.DOAnchorPos(ResetTransform.anchoredPosition, 0.2f).SetDelay(0.01f);
                }
                else
                {
                    MainArrow.DOAnchorPos(new Vector2(SectionTransform[current - 1].anchoredPosition.x, SectionTransform[current - 1].anchoredPosition.y + 10f), 0.2f).SetDelay(0.01f);
                }
            }

            foreach (GameObject obj in SubObjectParent)
            {
                obj.SetActive(false);
            }

            if (HoveredSection != SettingsSection.Reset)
            {
                SubObjectParent[(int)HoveredSection - 1].SetActive(true);
            }
            return;
        }
        else
        {
            if (CurrentSubButtons == null || CurrentSubButtons.Count < 1) return;

            if (Input.x > 0.1f)
            {
                HoveredSubButton++;
                HoveredSubButton = Mathf.Clamp(HoveredSubButton, 0, CurrentSubButtons.Count - 1);
                CurrentSubButtons[HoveredSubButton]._eventonhover?.Invoke();
                CurrentButton = CurrentSubButtons[HoveredSubButton];
                SubArrow.DOAnchorPos(new Vector2(CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.x, CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.y + 85f), 0.2f);
                return;
            }
            if (Input.x < -0.1f)
            {
                HoveredSubButton--;
                HoveredSubButton = Mathf.Clamp(HoveredSubButton, 0, CurrentSubButtons.Count - 1);
                CurrentSubButtons[HoveredSubButton]._eventonhover?.Invoke();
                CurrentButton = CurrentSubButtons[HoveredSubButton];
                SubArrow.DOAnchorPos(new Vector2(CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.x, CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.y + 85f), 0.2f);
                return;
            }
            if (Input.y > 0.1f && CurrentSection == SettingsSection.Resolution)
            {
                HoveredSubButton -= 3;
                HoveredSubButton = Mathf.Clamp(HoveredSubButton, 0, CurrentSubButtons.Count - 1);
                CurrentSubButtons[HoveredSubButton]._eventonhover?.Invoke();
                CurrentButton = CurrentSubButtons[HoveredSubButton];
                SubArrow.DOAnchorPos(new Vector2(CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.x, CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.y + 85f), 0.2f);
                return;
            }
            if (Input.y < -0.1f && CurrentSection == SettingsSection.Resolution)
            {
                HoveredSubButton += 3;
                HoveredSubButton = Mathf.Clamp(HoveredSubButton, 0, CurrentSubButtons.Count - 1);
                CurrentSubButtons[HoveredSubButton]._eventonhover?.Invoke();
                CurrentButton = CurrentSubButtons[HoveredSubButton];
                SubArrow.DOAnchorPos(new Vector2(CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.x, CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.y + 85f), 0.2f);
                return;
            }
        }
    }

    private void HovertoCurrentSubSettings(SettingsSection _CurrentSection)
    {
        if (CurrentSubButtons == null || CurrentSubButtons.Count < 1) return;

        HoveredSubButton = 0;

        switch (_CurrentSection)
        {
            case SettingsSection.Resolution:
                HoveredSubButton = SettingsManager.Instance.data.ScreenSetting;
                break;
            case SettingsSection.FPS:
                HoveredSubButton = SettingsManager.Instance.data.FPS;
                break;
            case SettingsSection.Language:
                switch (SettingsManager.Instance.data.Language)
                {
                    case GameLanguage.English:
                        HoveredSubButton = 1;
                        break;
                    case GameLanguage.Japanese:
                        HoveredSubButton = 2;
                        break;
                    case GameLanguage.Mandarin:
                        HoveredSubButton = 0;
                        break;
                }
                break;
            case SettingsSection.DialogColor:
                HoveredSubButton = SettingsManager.Instance.data.DialogColor;
                break;
            case SettingsSection.Music:
                HoveredSubButton = SettingsManager.Instance.data.MusicVol;
                break;
            case SettingsSection.Ambience:
                HoveredSubButton = SettingsManager.Instance.data.AmbienceVol;
                break;
            case SettingsSection.Effect:
                HoveredSubButton = SettingsManager.Instance.data.EffectVol;
                break;
        }

        CurrentButton = CurrentSubButtons[HoveredSubButton];
        SubArrow.DOAnchorPos(new Vector2(CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.x, CurrentSubButtons[HoveredSubButton].GetComponent<RectTransform>().anchoredPosition.y + 85f), 0f);
        CurrentSubButtons[HoveredSubButton]._eventonhover?.Invoke();
    }


    private void OnConfirm()
    {
        if (CurrentSection != SettingsSection.Main && CurrentButton != null)
        {
            CurrentButton._event?.Invoke();
        }
        CurrentButton = null;

        CurrentSection = CurrentSection == SettingsSection.Main ? HoveredSection : SettingsSection.Main;

        if (CurrentSection == SettingsSection.Reset)
        {
            CurrentSection = SettingsSection.Main;
            SettingsManager.Instance.ResetSettings();
            HoveredSection = 0;
            MainArrow.DOAnchorPos(new Vector2(SectionTransform[0].anchoredPosition.x, SectionTransform[0].anchoredPosition.y + 10f), 0.2f);

            SubButtonsMus[SettingsManager.Instance.data.MusicVol]._eventonhover?.Invoke();
            SubButtonsAmb[SettingsManager.Instance.data.AmbienceVol]._eventonhover?.Invoke();
            SubButtonsEff[SettingsManager.Instance.data.EffectVol]._eventonhover?.Invoke();

            StartCoroutine(NavigateDelay());
            return;
        }

        SubArrow.gameObject.SetActive(CurrentSection != SettingsSection.Main);

        ResetSection();
        HovertoCurrentSubSettings(CurrentSection);
    }

    private void OnCancel()
    {
        if (CurrentSection == SettingsSection.Main) return;
        CurrentButton = null;
        HovertoCurrentSubSettings(CurrentSection);

        CurrentSection = CurrentSection == SettingsSection.Main ? HoveredSection : SettingsSection.Main;
        SubArrow.gameObject.SetActive(CurrentSection != SettingsSection.Main);
        ResetSection();
    }

    private void ResetSection()
    {
        List<SettingsButton> CurrentList = null;

        switch (CurrentSection)
        {
            case SettingsSection.Resolution:
                CurrentList = SubButtonsRes;
                break;
            case SettingsSection.FPS:
                CurrentList = SubButtonsFps;
                break;
            case SettingsSection.Language:
                CurrentList = SubButtonsLang;
                break;
            case SettingsSection.DialogColor:
                CurrentList = SubButtonsDialog;
                break;
            case SettingsSection.Music:
                CurrentList = SubButtonsMus;
                break;
            case SettingsSection.Ambience:
                CurrentList = SubButtonsAmb;
                break;
            case SettingsSection.Effect:
                CurrentList = SubButtonsEff;
                break;
        }

        CurrentSubButtons = CurrentList;
    }
}