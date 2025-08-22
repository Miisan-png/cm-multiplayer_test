using UnityEngine;
using System.IO;
using System;

public class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager instance;
    public static SaveLoadManager Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if(SettingsManager.Instance != null)
        {
            currentSaveSlot = SettingsManager.Instance.data.CurrrentSaveSlot;
        }

        GameManager.Instance.onDayEnd += AutoSave;
    }

    [SerializeField] private int currentSaveSlot = 1; // Default to slot 1
    public int CurrentSaveSlot
    {
        get => currentSaveSlot;
        set => currentSaveSlot = Mathf.Clamp(value, 1, 3); // Ensure slot is between 1-3
    }

    public delegate void onautosave();
    public event onautosave OnAutoSave;

    private const string SAVE_SLOT_INFO_FILE = "SlotInfo.json";

    public void SaveSlotInfo(SaveSlotInfo info, int slot)
    {
        SaveData(info, SAVE_SLOT_INFO_FILE, slot);
    }

    public SaveSlotInfo LoadSlotInfo(int slot)
    {
        return LoadData<SaveSlotInfo>(SAVE_SLOT_INFO_FILE, slot);
    }

    public SaveSlotInfo[] GetAllSaveSlotsInfo()
    {
        SaveSlotInfo[] slots = new SaveSlotInfo[3];
        for (int i = 0; i < 3; i++)
        {
            slots[i] = LoadSlotInfo(i + 1) ?? null;
        }
        return slots;
    }

    private string GetFilePath(string fileName, int? slot = null)
    {
        int saveSlot = slot ?? CurrentSaveSlot;
        string slotFolder = $"SaveSlot{saveSlot}";

        // Create the slot directory if it doesn't exist
        string slotPath = Path.Combine(Application.persistentDataPath, slotFolder);
        if (!Directory.Exists(slotPath))
        {
            Directory.CreateDirectory(slotPath);
        }

        string path = Path.Combine(slotPath, fileName);

        // Fallback to streaming assets if file doesn't exist in persistent data
        if (!File.Exists(path))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            if (File.Exists(streamingPath))
            {
                return streamingPath;
            }
        }

        return path;
    }

    public void AutoSave()
    {
        OnAutoSave?.Invoke();

        SaveSlotInfo slotdata = LoadSlotInfo(currentSaveSlot);

        if(slotdata == null)
        {
            slotdata = new SaveSlotInfo("Player", CalendarManager.Instance.daycount, GameManager.Instance.totalPlayTime, PlayerInventory.Instance.playergender,CalendarManager.Instance.day);
        }
        else
        {
            slotdata = new SaveSlotInfo(slotdata.playerName, CalendarManager.Instance.daycount, GameManager.Instance.totalPlayTime + slotdata.playTime, PlayerInventory.Instance.playergender, CalendarManager.Instance.day);
        }

        SaveSlotInfo(slotdata, currentSaveSlot);

        Debug.Log("Auto Save Triggered");
    }

    public void SaveData<T>(T data, string fileName, int? slot = null)
    {
        string path = GetFilePath(fileName, slot);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Data saved to {path}");
    }

    public T LoadData<T>(string fileName, int? slot = null)
    {
        string path = GetFilePath(fileName, slot);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Debug.Log($"Loaded JSON from {path}: {json}");
            return JsonUtility.FromJson<T>(json);
        }
        else
        {
            Debug.LogWarning($"File not found at {path}, returning default.");
            return default;
        }
    }

    public void DeleteData(string fileName, int? slot = null)
    {
        string path = GetFilePath(fileName, slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save file: {path}");
        }
        else
        {
            Debug.LogWarning("No save file to delete.");
        }
    }

    // Optional: Method to delete an entire save slot
    public void DeleteSaveSlot(int slot)
    {
        string slotPath = Path.Combine(Application.persistentDataPath, $"SaveSlot{slot}");
        if (Directory.Exists(slotPath))
        {
            Directory.Delete(slotPath, true);
            Debug.Log($"Deleted all data in Save Slot {slot}");
        }
        else
        {
            Debug.LogWarning($"No data found for Save Slot {slot}");
        }
    }
}

[System.Serializable]
public class SaveSlotInfo
{
    public string playerName;
    public int dayCount;
    public float playTime;
    public playerGender Gender;
    public CalendarDay calendarday;

    public SaveSlotInfo(string name,int daycount, float playtime, playerGender gender, CalendarDay _calendarday)
    {
        playerName = name;
        dayCount = daycount;
        playTime = playtime;
        Gender = gender;
        calendarday = _calendarday;
    }
}