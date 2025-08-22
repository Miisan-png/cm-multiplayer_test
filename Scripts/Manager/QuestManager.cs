using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private static QuestManager instance;
    public static QuestManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadSavedQuests();
        SaveLoadManager.Instance.OnAutoSave += () => {
            SaveQuestsData();
        };
    }

    [SerializeField] private List<Sequence> AllQuest; //serialize in scene from SOs

    [SerializeField] private QuestList PlayerQuestData;

    [SerializeField] private List<Sequence> OngoingQuest;
    [SerializeField] private List<Sequence> CompletedQuest;

    [SerializeField] private GameObject ExplorationParent;
    [SerializeField] private List<NPC_Overworld> CurrentQuestNPCs = new List<NPC_Overworld>();

    private void LoadSavedQuests()
    {
        QuestList data = SaveLoadManager.Instance.LoadData<QuestList>("PlayerSavedQuests.json");
        if(data == null)
        {
            SaveLoadManager.Instance.SaveData<QuestList>(PlayerQuestData, "PlayerSavedQuests.json");
            return;
        }
        PlayerQuestData = data;

        if (PlayerQuestData.SavedQuestData == null || PlayerQuestData.SavedQuestData.Count == 0) return;

        OngoingQuest = new List<Sequence>();
        CompletedQuest = new List<Sequence>();

        for(int i =0;i<PlayerQuestData.SavedQuestData.Count;i++)
        {
            switch(PlayerQuestData.SavedQuestData[i]._QuestsProgress)
            {
                case SequenceStatus.NotStarted:
                    break;
                case SequenceStatus.InProgress:
                    OngoingQuest.Add(GetQuestbyID(PlayerQuestData.SavedQuestData[i]._QuestsID, PlayerQuestData.SavedQuestData[i]._QuestsIndex, PlayerQuestData.SavedQuestData[i]._QuestsProgress));
                    break;
                case SequenceStatus.Completed:
                    CompletedQuest.Add(GetQuestbyID(PlayerQuestData.SavedQuestData[i]._QuestsID, PlayerQuestData.SavedQuestData[i]._QuestsIndex, PlayerQuestData.SavedQuestData[i]._QuestsProgress));
                    break;
            }
        }
    }

    public void SaveQuestsData()
    {
        QuestList data = new QuestList(new List<QuestData>());
        QuestData d = new QuestData(0, 0, 0);

        if (OngoingQuest != null && OngoingQuest.Count > 0)
        {
            for (int i = 0; i < OngoingQuest.Count; i++)
            {
                d._QuestsID = OngoingQuest[i].sequenceID;
                d._QuestsProgress = OngoingQuest[i].status;
                d._QuestsIndex = OngoingQuest[i].taskindex;

                data.SavedQuestData.Add(d);
            }
        }

        if(CompletedQuest!=null && CompletedQuest.Count>0)
        {
            for (int i = 0; i < CompletedQuest.Count; i++)
            {
                d._QuestsID = CompletedQuest[i].sequenceID;
                d._QuestsProgress = CompletedQuest[i].status;
                d._QuestsIndex = CompletedQuest[i].taskindex;

                data.SavedQuestData.Add(d);
            }
        }

        PlayerQuestData = data;
        SaveLoadManager.Instance.SaveData<QuestList>(PlayerQuestData, "PlayerSavedQuests.json");
    }

    public Sequence GetQuestbyID(int id,int trackindex,SequenceStatus status)
    {
        Sequence SQ = null;
        for(int i =0;i<AllQuest.Count;i++)
        {
            if (AllQuest[i].sequenceID==id)
            {
                SQ = AllQuest[i];
                SQ.InitializeSequence(status,trackindex);
            }
        }
        return SQ;
    }

    public void StartQuest(int QuestID)
    {
        Sequence SQ = GetQuestbyID(QuestID,0,SequenceStatus.InProgress);

        if(OngoingQuest.Contains(SQ)||CompletedQuest.Contains(SQ))
        {
            return;
        }
        else
        {
            SQ.StartSequence();
            OngoingQuest.Add(SQ);
        }
    }

    public void CompleteSequence(Sequence sequencetoComplete)
    {
        sequencetoComplete.CompleteSequence();
        OngoingQuest.Remove(sequencetoComplete);
        CompletedQuest.Add(sequencetoComplete);

        if (CurrentQuestNPCs!=null && CurrentQuestNPCs.Count>0)
        {
            for (int i = 0; i < CurrentQuestNPCs.Count; i++)
            {
                CurrentQuestNPCs[i].HandleQuestComplete();
            }
        }
        CurrentQuestNPCs.Clear();
    }

    public void AddQuestNPCs(NPC_Overworld npctoAdd) //To reset NPCs after quest completion
    {
        if (npctoAdd == null || CurrentQuestNPCs.Contains(npctoAdd)) return;
        CurrentQuestNPCs.Add(npctoAdd);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            StartQuest(1);
        }
    }
}

[System.Serializable]
public class QuestList
{
    public List<QuestData> SavedQuestData;

    public QuestList(List<QuestData> _savedata)
    {
        SavedQuestData = _savedata;
    }
}

[System.Serializable]
public class QuestData
{
    public int _QuestsID;
    public int _QuestsIndex;
    public SequenceStatus _QuestsProgress;

    public QuestData(int questsID, int questsIndex, SequenceStatus questsProgress)
    {
        _QuestsID = questsID;
        _QuestsIndex = questsIndex;
        _QuestsProgress = questsProgress;
    }
}
