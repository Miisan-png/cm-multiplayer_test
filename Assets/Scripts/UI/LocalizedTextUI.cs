using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextBox;
    [SerializeField] private List<LanguageTextWrapper> AllTexts;
    [SerializeField] private GameLanguage CurrentLanguage = GameLanguage.English;

    private void Start()
    {
        SettingsManager.Instance.OnLanguageUpdated += UpdateText;
    }

    private void OnDestroy()
    {
        SettingsManager.Instance.OnLanguageUpdated -= UpdateText;
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (!gameObject.activeSelf || !gameObject.activeInHierarchy || AllTexts == null || AllTexts.Count < 1 || SettingsManager.Instance == null || CurrentLanguage == SettingsManager.Instance.data.Language) return;

        for (int i = 0; i < AllTexts.Count; i++)
        {
            if (AllTexts[i].language == SettingsManager.Instance.data.Language)
            {
                if (AllTexts[i].font != null)
                {
                    TextBox.font = AllTexts[i].font;
                }
                else
                {
                    TextBox.font = SettingsManager.Instance.GetLocalizedFont();
                }    
                TextBox.text = AllTexts[i].text.Trim();
                CurrentLanguage = SettingsManager.Instance.data.Language;
                break;
            }
        }
    }
}
