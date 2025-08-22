using TMPro;
using UnityEngine;

public class CalendarManager : MonoBehaviour
{
    private static CalendarManager instance;
    public static CalendarManager Instance => instance;

    private void Awake()
    {
        instance = this;  
    }

    private void Start()
    {
        GameManager.Instance.onDayStart += SkipNextDay;
    }


    [SerializeField] private CalendarDay _Day = CalendarDay.Sunday;
    public CalendarDay day => _Day;

    [SerializeField] private int dayCount = 0;
    public int daycount => dayCount;

    public enum DayState
    {
        Weekday, Weekend
    }
    [SerializeField] private DayState dayState = DayState.Weekend;
    public DayState daystate => dayState;

    public void SkipNextDay()
    {
        dayCount++;

        switch(_Day)
        {
            case CalendarDay.Monday:
                _Day = CalendarDay.Tuesday;
                SetState(DayState.Weekday);
                break;
            case CalendarDay.Tuesday:
                _Day = CalendarDay.Wednesday;
                SetState(DayState.Weekday);
                break;
            case CalendarDay.Wednesday:
                _Day = CalendarDay.Thursday;
                SetState(DayState.Weekday);
                break;
            case CalendarDay.Thursday:
                _Day = CalendarDay.Friday;
                SetState(DayState.Weekday);
                break;
            case CalendarDay.Friday:
                _Day = CalendarDay.Saturday;
                SetState(DayState.Weekend);
                break;
            case CalendarDay.Saturday:
                _Day = CalendarDay.Sunday;
                SetState(DayState.Weekend);
                break;
            case CalendarDay.Sunday:
                _Day = CalendarDay.Monday;
                SetState(DayState.Weekday);
                break;
        }
    }

    public void SetState(DayState state)
    {
        dayState = state;
    }

    public string GetCurrentDayString()
    {
        switch (SettingsManager.Instance.data.Language)
        {
            case GameLanguage.English:
                return day.ToString();
            case GameLanguage.Japanese:
                switch (_Day)
                {
                    case CalendarDay.Monday:
                        return "月曜日";
                    case CalendarDay.Tuesday:
                        return "火曜日";
                    case CalendarDay.Wednesday:
                        return "水曜日";
                    case CalendarDay.Thursday:
                        return "木曜日";
                    case CalendarDay.Friday:
                        return "金曜日";
                    case CalendarDay.Saturday:
                        return "土曜日";
                    case CalendarDay.Sunday:
                        return "日曜日";
                }
                break;
            case GameLanguage.Mandarin:
                switch (_Day)
                {
                    case CalendarDay.Monday:
                        return "星期一";
                    case CalendarDay.Tuesday:
                        return "星期二";
                    case CalendarDay.Wednesday:
                        return "星期三";
                    case CalendarDay.Thursday:
                        return "星期四";
                    case CalendarDay.Friday:
                        return "星期五";
                    case CalendarDay.Saturday:
                        return "星期六";
                    case CalendarDay.Sunday:
                        return "星期日";
                }
                break;
        }

        return "";
    }
}
public enum CalendarDay
{
    Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
}
