using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task_TalktoNPC_", menuName = "Quest/Task_TalktoNPC")]
public class Task_TalktoNPC : Task
{
    [SerializeField] string npcName;
    public string npcname => npcName;
    [SerializeField] string dialogueID;
    

    public override void StartTask()
    {
        base.StartTask();
        OverrideNPCDialogue();
    }

    public void OverrideNPCDialogue()
    {
        List<NPC_Overworld> NPCs = NPCManager.Instance.SearchForNPC();
        NPC_Overworld NPCtoOverride = null;

        for (int i = 0; i < NPCs.Count; i++)
        {
            if (NPCs[i].npcData.Name == npcName)
            {
                NPCtoOverride = NPCs[i];
            }
        }
        for (int i = 0; i < NPCs.Count; i++)
        {
            QuestManager.Instance.AddQuestNPCs(NPCs[i]);
        }
        if (NPCtoOverride == null) return;
        Debug.Log($"{NPCtoOverride.npcData.Name}");
        NPCtoOverride.HandleQuestOverride(dialogueID);
        DialogueManager.Instance.AddDialogueTasks(this);
    }

}
