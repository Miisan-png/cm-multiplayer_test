using UnityEngine;

public enum QuestStatus { NotStarted, InProgress, Completed }

[System.Serializable]
public class Task : ScriptableObject    
{
    [SerializeField] private string TaskName;
    public string taskname => TaskName;
    [SerializeField] private string Description;
    [SerializeField] protected QuestStatus Status; 
    [SerializeField] private Sequence ParentSequence;

    public QuestStatus GetStatus()
    {
        return Status;
    }    

    public virtual void StartTask() //Can be called in subclasses then add on extra actions if neccessary
    {
        if (Status == QuestStatus.Completed) return;
        Status = QuestStatus.InProgress;
    }

    public virtual void CompleteTask() //Can be called in subclasses then add on extra actions if neccessary
    {
        if (Status == QuestStatus.InProgress)
        {
            Status = QuestStatus.Completed;
            ParentSequence.ChecktoComplete();
        }
    }


}
