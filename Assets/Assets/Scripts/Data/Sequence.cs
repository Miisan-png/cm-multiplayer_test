using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Sequence_", menuName = "Quest/Sequence")]
public class Sequence : ScriptableObject
{
    [SerializeField] private int SequenceID;
    public int sequenceID => SequenceID;

    [SerializeField] private string SequenceName;
    [SerializeField] private string Description;
    [SerializeField] private SequenceStatus Status = SequenceStatus.NotStarted;
    public SequenceStatus status => Status;
    [SerializeField] private SequenceType QuestType;

    [SerializeField] private List<Task> Tasks;

    [SerializeField] private int TaskIndex = 0;
    public int taskindex => TaskIndex;

    [SerializeField] bool isLinear;

    public SequenceStatus GetStatus()
    {
        SequenceStatus S = Status;
        return S;
    }
    public SequenceType GetQuestType()
    {
        SequenceType T = QuestType;
        return T;
    }
    public List<Task> GetOngoingTasks()
    {
        List<Task> T = null;

        for(int i=0;i<Tasks.Count;i++)
        {
            if (Tasks[i].GetStatus() == QuestStatus.InProgress)
            {
                T.Add(Tasks[i]);
            }
        }
        return T;
    }


    public void InitializeSequence(SequenceStatus _Status,int _index)
    {
        Status = _Status;
        TaskIndex = _index;
    }

    public virtual void StartSequence() //Can be called in subclasses then add on extra actions if neccessary
    {
        Status = SequenceStatus.InProgress;

        switch(isLinear)
        {
            case true:
                Tasks[TaskIndex].StartTask();
                break;
            case false:
                for(int i=0;i<Tasks.Count;i++)
                {
                    Tasks[i].StartTask();
                }
                break;
        }
    }

    public void SkipNextTask()
    {
        if(isLinear && TaskIndex < Tasks.Count-1)
        {
            TaskIndex++;
            Tasks[TaskIndex].StartTask();
        }
    }

    public bool ChecktoComplete()
    {
        Debug.Log("checking sequence for completion..");
        SkipNextTask();
        for (int i = 0; i < Tasks.Count; i++)
        {
            if (Tasks[i].GetStatus() != QuestStatus.Completed)
            {
                return false;
            }
        }

        Debug.Log("sequence complete");
        QuestManager.Instance.CompleteSequence(this);
        return true;

    }

    public void CompleteSequence() //Can be called in subclasses then add on extra actions if neccessary
    {
        Status = SequenceStatus.Completed;
    }

}
public enum SequenceStatus { NotStarted, InProgress, Completed }

public enum SequenceType { MainQuest,SideQuest }