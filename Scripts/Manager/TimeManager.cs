using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager instance;
    public static TimeManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float dayDurationInSeconds = 280f; // Total duration for a full day (10AM-8PM)
    public float daydurationinseconds => dayDurationInSeconds;
    [SerializeField] private float currentTimer;
    public float currenttimer => currentTimer;
    [SerializeField] private DayState State;
    public DayState state => State;
    private Coroutine DaytimerCoroutine;

    // Time settings
    private const float realWorldStartHour = 10f; // 10 AM
    private const float realWorldEndHour = 20f;   // 8 PM
    private const float realWorldHoursInDay = realWorldEndHour - realWorldStartHour; // 10 hours
    private const float secondsPerInGameHour = 28f; // 280s day / 10 hours = 28s/hour
    public float SecondsPerInGameHour => secondsPerInGameHour;

    private void SetState(DayState S)
    {
        if (State == S) return;
        State = S;
        OnDayStateChanged?.Invoke(State);
    }
    public Action<DayState> OnDayStateChanged;

    public delegate void sixPM();
    public event sixPM on6PM;
    private bool after6;

    public delegate void eightPM();
    public event eightPM on8PM;
    private bool After8;

    [SerializeField] public bool isPaused;

    private void Start()
    {
        GameManager.Instance.onDayStart += startCountDown;
        GameManager.Instance.onDayEnd += StopAllCoroutines;

        UIManager.Instance.onDialogueOn += pauseTimer;
        UIManager.Instance.onDialogueOff += resumeTimer;

        UIManager.Instance.onScreenTransitionOn += pauseTimer;
        UIManager.Instance.onScreenTransitionOff += resumeTimer;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onDayStart -= startCountDown;
        GameManager.Instance.onDayEnd -= StopAllCoroutines;

        UIManager.Instance.onDialogueOn -= pauseTimer;
        UIManager.Instance.onDialogueOff -= resumeTimer;

        UIManager.Instance.onScreenTransitionOn -= pauseTimer;
        UIManager.Instance.onScreenTransitionOff -= resumeTimer;
    }

    public void startCountDown()
    {
        after6 = false;
        After8 = false;
        currentTimer = dayDurationInSeconds;
        if (DaytimerCoroutine != null)
        {
            StopCoroutine(DaytimerCoroutine);
        }
        SetState(DayState.Morning);
        DaytimerCoroutine = StartCoroutine(startTimer());
    }

    private IEnumerator startTimer()
    {
        while (currentTimer > 0)
        {
            if (!isPaused)
            {
                currentTimer -= Time.deltaTime;
                // 6PM check (which is at 8 hours into the 10-hour day, or 80% progress)
                if (currentTimer <= dayDurationInSeconds * 0.2f && !after6) // 20% remaining = 8 hours passed
                {
                    after6 = true;
                    SetState(DayState.Afternoon);
                    on6PM?.Invoke();
                }
            }

            yield return null;
        }

        currentTimer = 0;

        if (!After8)
        {
            After8 = true;

            UIManager.Instance.startFade(0f, () => {
                SetState(DayState.Night);
                on8PM?.Invoke();
                UIManager.Instance.endFade();
            });
            yield break;
        }
    }
    public void FastForwardHours(int hoursToFastForward)
    {
        if (hoursToFastForward <= 0 || After8) return;

        float currentHour = realWorldStartHour + ((1f - (currentTimer / dayDurationInSeconds)) * realWorldHoursInDay);
        float remainingHoursTo7PM = 20f - currentHour;
        float hoursToAdvance = Mathf.Min(hoursToFastForward, remainingHoursTo7PM);

        if (hoursToAdvance <= 0) return;

        float timeToSubtract = hoursToAdvance * secondsPerInGameHour;
        currentTimer = Mathf.Max(currentTimer - timeToSubtract, 0);

        if (currentTimer <= dayDurationInSeconds * 0.2f && !after6)
        {
            after6 = true;
            SetState(DayState.Afternoon);
            on6PM?.Invoke();
        }

        if (currentTimer <= 0 && !After8)
        {
            After8 = true;
            SetState(DayState.Night);
            on8PM?.Invoke();

            if(DaytimerCoroutine != null)
            {
                StopCoroutine(DaytimerCoroutine);
            }
        }
    }

    public string GetCurrentTime(float currenttimer)
    {
        if (After8)
        {
            return "";
        }

        // Calculate progress through the day (0 to 1)
        float dayProgress = 1f - (currenttimer / dayDurationInSeconds);

        // Convert to in-game time (10AM to 8PM)
        float currentHour = realWorldStartHour + (dayProgress * realWorldHoursInDay);

        // Format as 24-hour time (no AM/PM)
        return $"{Mathf.FloorToInt(currentHour):00}:00";
    }
    public void pauseTimer()
    {
        if (!isPaused)
        {
            isPaused = true;
        }
    }

    public void resumeTimer()
    {
        if (!DialogueManager.Instance.dialogueOn && (GameManager.Instance.gamestate != gameState.Battle) && isPaused)
        {
            isPaused = false;
        }
    }
}

public enum DayState
{
    Morning, Afternoon, Night
}