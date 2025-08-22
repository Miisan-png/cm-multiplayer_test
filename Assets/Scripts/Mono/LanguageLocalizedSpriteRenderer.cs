using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageLocalizedSpriteRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private List<LocalizedSprite> Sprites;
    [SerializeField] private GameLanguage CurrentLanguage = GameLanguage.English;

    private void Start()
    {
        SettingsManager.Instance.OnLanguageUpdated += UpdateSprite;
    }

    private void OnEnable()
    {
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (SR == null || Sprites == null || Sprites.Count < 1 || SettingsManager.Instance == null || CurrentLanguage == SettingsManager.Instance.data.Language) return;

        for (int i = 0; i < Sprites.Count; i++)
        {
            if (Sprites[i].Language == SettingsManager.Instance.data.Language)
            {
                SR.sprite = Sprites[i].Sprite;
                CurrentLanguage = SettingsManager.Instance.data.Language;
                break;
            }
        }
    }
}

[System.Serializable]
public class LocalizedSprite
{
    public Sprite Sprite;
    public GameLanguage Language;
}
