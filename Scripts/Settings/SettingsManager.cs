using System;
using TMPro;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;
    public static SettingsManager Instance => instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

        TryLoadData();
    }

    private void Start()
    {
        SetResolution(SettingsData.ScreenSetting);
        SetFPS(SettingsData.FPS);
        SetDialogColor(SettingsData.DialogColor);
        SetMusicVol(SettingsData.MusicVol);
        SetAmbVol(SettingsData.AmbienceVol);
        SetEffVol(SettingsData.EffectVol);

        cacheddialogcolor = SettingsData.DialogColor;
        cachedvol1 = SettingsData.MusicVol;
        cachedvol2 = SettingsData.AmbienceVol;
        cachedvol3 = SettingsData.EffectVol;

        SaveLoadManager.Instance.OnAutoSave += SaveSettingsData;
    }

    [SerializeField] private TMP_FontAsset EnFontOutline;
    public TMP_FontAsset enfontline => EnFontOutline;
    [SerializeField] private TMP_FontAsset EnFontDialogue;
    public TMP_FontAsset enfontdialogue => EnFontDialogue;

    [SerializeField] private TMP_FontAsset EnFont;
    public TMP_FontAsset enfont => EnFont;
    [SerializeField] private TMP_FontAsset JpFontOutline;
    public TMP_FontAsset jpfontoutline => JpFontOutline;
    [SerializeField] private TMP_FontAsset JpFont;
    public TMP_FontAsset jpfont => JpFont;
    [SerializeField] private TMP_FontAsset JpFontDialogue;
    public TMP_FontAsset jpfontdialogue => JpFontDialogue;

    [SerializeField] private TMP_FontAsset CnFontOutline;
    public TMP_FontAsset cnfontoutline => CnFontOutline;
    [SerializeField] private TMP_FontAsset CnFont;
    public TMP_FontAsset cnfont => CnFont;
    [SerializeField] private TMP_FontAsset CnFontDialogue;
    public TMP_FontAsset cnfontdialogue => CnFontDialogue;

    public TMP_FontAsset GetLocalizedFont()
    {
        switch (data.Language)
        {
            case GameLanguage.English:
                return enfont;
            case GameLanguage.Japanese:
                return jpfont;
            case GameLanguage.Mandarin:
                return cnfont;
        }
        return enfont;
    }
    public TMP_FontAsset GetLocalizedDialogFont()
    {
        switch (data.Language)
        {
            case GameLanguage.English:
                return enfontdialogue;
            case GameLanguage.Japanese:
                return jpfontdialogue;
            case GameLanguage.Mandarin:
                return cnfontdialogue;
        }
        return enfontdialogue;
    }

    public TMP_FontAsset GetLocalizedOutlineFont()
    {
        switch (data.Language)
        {
            case GameLanguage.English:
                return enfontline;
            case GameLanguage.Japanese:
                return jpfontoutline;
            case GameLanguage.Mandarin:
                return cnfontoutline;
        }
        return enfontline;
    }
    public Action OnLanguageUpdated;

    [SerializeField] private SettingsWrapper SettingsData;
    public SettingsWrapper data => SettingsData;

    private int cacheddialogcolor;
    private int cachedvol1;
    private int cachedvol2;
    private int cachedvol3;

    [SerializeField] private List<Color> AllDialogColors;
    public List<Color> alldialogcolors => AllDialogColors;
    [SerializeField] private List<Vector2> AllScreenSize;
    [SerializeField] private List<int> AllFps;

    public void SetLanguage(int newlanguage)
    {
        newlanguage = Mathf.Clamp(newlanguage, 0, Enum.GetNames(typeof(GameLanguage)).Length);
        if (SettingsData.Language == (GameLanguage)newlanguage) return;
        SettingsData.Language = (GameLanguage)newlanguage;
        OnLanguageUpdated?.Invoke();
        SettingsData.Language = (GameLanguage)newlanguage;
    }

    public void SetResolution(int screen)
    {
        screen = Mathf.Clamp(screen, 0, 5);
        SettingsData.ScreenSetting = screen;
        Screen.SetResolution((int)AllScreenSize[SettingsData.ScreenSetting].x, (int)AllScreenSize[SettingsData.ScreenSetting].y,FullScreenMode.FullScreenWindow);
    }

    public void SetFPS(int fps)
    {
        fps = Mathf.Clamp(fps, 0, 1);
        Application.targetFrameRate = AllFps[fps];
        SettingsData.FPS = fps;
    }

    public void SetDialogColor(int color)
    {
        color = Mathf.Clamp(color, 0, 11);
        if (AllDialogColors[color] == null || SettingsController.Instance == null) return;
        Color newcolor = AllDialogColors[color];

        EnFontDialogue.material.SetColor(ShaderUtilities.ID_FaceColor, newcolor);
        JpFontDialogue.material.SetColor(ShaderUtilities.ID_FaceColor, newcolor);
        CnFontDialogue.material.SetColor(ShaderUtilities.ID_FaceColor, newcolor);

        cacheddialogcolor = color;
    }

    public void SetMusicVol(int vol)
    {
        vol = Mathf.Clamp(vol,0, 10);
        cachedvol1 = vol;
    }

    public void SetAmbVol(int vol)
    {
        vol = Mathf.Clamp(vol, 0, 10);
        cachedvol2 = vol;
    }

    public void SetEffVol(int vol)
    {
        vol = Mathf.Clamp(vol, 0, 10);
        cachedvol3 = vol;
    }

    public void SaveSettingsData()
    {
        SettingsData.CurrrentSaveSlot = SaveLoadManager.Instance.CurrentSaveSlot;
        SettingsData.DialogColor = cacheddialogcolor;
        SettingsData.MusicVol = cachedvol1;
        SettingsData.AmbienceVol = cachedvol2;
        SettingsData.EffectVol = cachedvol3;

        string path = Path.Combine(Application.persistentDataPath, "settings.json"); // Added filename
        string json = JsonUtility.ToJson(SettingsData, true);
        File.WriteAllText(path, json);
    }

    private void TryLoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, "settings.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SettingsData = JsonUtility.FromJson<SettingsWrapper>(json);
        }
        else
        {
            SettingsData = new SettingsWrapper();
        }
    }

    public void ResetSettings()
    {
        GameLanguage currentlanguage = SettingsData.Language;
        SettingsData = new SettingsWrapper();
        SettingsData.Language = currentlanguage;

        SetResolution(SettingsData.ScreenSetting);
        SetFPS(SettingsData.FPS);
        SetDialogColor(SettingsData.DialogColor);
        SetMusicVol(SettingsData.MusicVol);
        SetAmbVol(SettingsData.AmbienceVol);
        SetEffVol(SettingsData.EffectVol);

        SaveSettingsData();
    }
}

[System.Serializable]
public class SettingsWrapper
{
    public int CurrrentSaveSlot = 1;
    public int ScreenSetting = 1;
    public int FPS = 0;
    public int DialogColor = 3;
    public GameLanguage Language = GameLanguage.English;
    public int MusicVol = 5;
    public int AmbienceVol = 5;
    public int EffectVol = 5;
}


[System.Serializable]
public class LanguageTextWrapper
{
    [SerializeField] private string Text;
    public string text => Text;
    [SerializeField] private GameLanguage Language;
    public GameLanguage language => Language;
    [SerializeField] private TMP_FontAsset Font;
    public TMP_FontAsset font => Font;
}


public enum GameLanguage
{
     English, Japanese ,Mandarin
}