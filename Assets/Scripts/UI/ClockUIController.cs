using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClockUIController : MonoBehaviour
{
    public static ClockUIController instance;
    public static ClockUIController Instance => instance;

    private void Awake()
    {
        instance = this;
    }


    [SerializeField] private List<GameObject> InitializedObjects;
    [SerializeField] private TextMeshProUGUI ClockText;
    [SerializeField] private TextMeshProUGUI DayText;
    [SerializeField] private int fastforwardhours;

    private bool Initialized;
    public bool initialized => Initialized;

    private void Start()
    {
        PauseUIController.instance.OnPauseStart += UpdateDayText;
        SettingsManager.Instance.OnLanguageUpdated += UpdateDayText;
    }

    private void UpdateDayText()
    {
        DayText.text = CalendarManager.Instance.GetCurrentDayString();

        if(SettingsManager.Instance.data.Language != GameLanguage.English)
        {
            ClockText.font = SettingsManager.Instance.GetLocalizedFont();
            DayText.font = SettingsManager.Instance.GetLocalizedFont();
        }
        else
        {
            ClockText.font = SettingsManager.Instance.enfontline;
            DayText.font = SettingsManager.Instance.enfontline;
        }

        if (TimeManager.Instance.state != DayState.Night)
        {
            ClockText.text = TimeManager.Instance.GetCurrentTime(TimeManager.Instance.currenttimer);
        }
        else
        {
            switch (SettingsManager.Instance.data.Language)
            {
                case GameLanguage.English:
                    ClockText.text = "NIGHT";
                    break;
                case GameLanguage.Japanese:
                    ClockText.text = "夜";
                    break;
                case GameLanguage.Mandarin:
                    ClockText.text = "夜晚";
                    break;
            }
        }
    }

    public void InitializeUI()
    {
        fastforwardhours = 0;

        SubscribetoInputs();

        foreach (GameObject obj in InitializedObjects)
        {
            obj.SetActive(true);
        }
        Initialized = true;
    }

    private void SubscribetoInputs()
    {
        UIInputManager.Instance.OnNavigate += OnNavigate;
        UIInputManager.Instance.OnInteract += OnConfirm;
    }

    private void UnsubscribetoInputs()
    {
        UIInputManager.Instance.OnNavigate -= OnNavigate;
        UIInputManager.Instance.OnInteract -= OnConfirm;
    }

    private void OnNavigate(Vector2 input)
    {
        // Get current in-game hour (10 AM to 8 PM)
        float currentHour = 10f + ((1f - (TimeManager.Instance.currenttimer / TimeManager.Instance.daydurationinseconds)) * 10f);

        // Calculate remaining hours until 7 PM (max fast-forward)
        float remainingHoursTo7PM = Mathf.Max(0f, 20f - currentHour);

        if (input.x > 0.1f) // Right stick → Increase time (fast-forward)
        {
            fastforwardhours = Mathf.Min(fastforwardhours + 1, Mathf.FloorToInt(remainingHoursTo7PM));
        }
        else if (input.x < -0.1f) // Left stick → Decrease time (rewind)
        {
            fastforwardhours = Mathf.Max(0, fastforwardhours - 1);
        }
        else
        {
            return;
        }

        // Calculate the preview time (current time + fast-forward offset)
        float previewTimeInSeconds = TimeManager.Instance.currenttimer - (fastforwardhours * TimeManager.Instance.SecondsPerInGameHour);

        // Update UI
        ClockText.text = TimeManager.Instance.GetCurrentTime(previewTimeInSeconds);
    }

    private void OnConfirm()
    {
        TimeManager.Instance.FastForwardHours(fastforwardhours);
        ClockText.text = TimeManager.Instance.GetCurrentTime(TimeManager.Instance.currenttimer);

        PauseUIController.instance.ExitSubMenu();
    }

    public void UninitializeUI()
    {
        UnsubscribetoInputs();
        ClockText.text = TimeManager.Instance.GetCurrentTime(TimeManager.Instance.currenttimer);
        foreach (GameObject obj in InitializedObjects)
        {
            obj.SetActive(false);
        }
        Initialized = false;
    }


}
