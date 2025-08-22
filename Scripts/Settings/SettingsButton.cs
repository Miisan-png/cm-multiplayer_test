
using UnityEngine.Events;
using UnityEngine;

[System.Serializable]
public class SettingsButton : MonoBehaviour
{
    [SerializeField] private UnityEvent Event;
    public UnityEvent _event => Event;
    [SerializeField] private UnityEvent EventOnHover;
    public UnityEvent _eventonhover => EventOnHover;

    public void SetLanguage(int newlanguage)
    {
        SettingsManager.Instance.SetLanguage(newlanguage);
    }

    public void SetResolution(int screen)
    {
        SettingsManager.Instance.SetResolution(screen);
    }

    public void SetFPS(int fps)
    {
        SettingsManager.Instance.SetFPS(fps);
    }

    public void SetDialogColor(int color)
    {
        SettingsManager.Instance.SetDialogColor(color);
    }

    public void SetMusicVol(int vol)
    {
        SettingsManager.Instance.SetMusicVol(vol);
    }

    public void SetAmbVol(int vol)
    {
        SettingsManager.Instance.SetAmbVol(vol);
    }

    public void SetEffVol(int vol)
    {
        SettingsManager.Instance.SetEffVol(vol);
    }

    public void SaveSettings()
    {
        SettingsManager.Instance.SaveSettingsData();
    }
}