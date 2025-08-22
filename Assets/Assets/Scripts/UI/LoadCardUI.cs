using TMPro;
using UnityEngine;

public class LoadCardUI : MonoBehaviour
{
    [SerializeField] private int SaveSlot;
    public int saveslot => SaveSlot;
    [SerializeField] private GameObject InfoParent;
    [SerializeField] private TextMeshProUGUI Playtime;
    public TextMeshProUGUI playtime => Playtime;
    [SerializeField] private TextMeshProUGUI Gametime;
    public TextMeshProUGUI gametime => Gametime;

    [SerializeField] private TextMeshProUGUI PlayerName;

    [SerializeField] private SaveSlotInfo SlotData;
    public SaveSlotInfo slotdata => SlotData;

    public void InitializeUI()
    {
        SaveSlotInfo info = SaveLoadManager.Instance.LoadSlotInfo(SaveSlot);

        if (info != null)
        {
            SlotData = info;
            float totalSeconds = info.playTime;
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);

            string hoursstring = "h";
            string minutestring = "m";
            string daystring = "Days";
            Playtime.font = SettingsManager.Instance.GetLocalizedFont();
            gametime.font = SettingsManager.Instance.GetLocalizedFont();

            switch (SettingsManager.Instance.data.Language)
            {
                case GameLanguage.Japanese:
                    hoursstring = "時";
                    minutestring = "分";
                    daystring = "日";  
                    break;
                case GameLanguage.Mandarin:
                    hoursstring = "时";
                    minutestring = "分";
                    daystring = "天";
                    break;
            }

            Playtime.text = $"{hours}{hoursstring} {minutes}{minutestring}";
            Gametime.text = $"{info.dayCount} {daystring}";
            PlayerName.text = $"{SlotData.playerName}";
            InfoParent.SetActive(true);
        }
        else
        {
            InfoParent.SetActive(false);
        }
    }

}
