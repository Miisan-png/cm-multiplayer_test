using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DialogueManager;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeAllDialogue();

        SettingsManager.Instance.OnLanguageUpdated += InitializeAllDialogue;
        UIInputManager.Instance.OnSubmit += handleOnSubmit;

        dialogueEvents["F:Go Home"] = () => {
            finishDialogue();
            PlayerManager.Instance.goHome();
        };
        dialogueEvents["F:Complete NPC Talk"] = () => {
            finishDialogue();
            CompleteDialogueTask(GetDialogueTask(NPCInteracting.npcData.Name));
        };
        dialogueEvents["F:Prompt Capture"] = () => {
            onCaptureMonster?.Invoke();
        };
        dialogueEvents["F:ShakeText"] = () => {
            ShakeTextBox();
            skipNextLine();
        };
    }

    public Action onCaptureMonster;

    [SerializeField] private List<Dialogue> allDialogue;
    [SerializeField] private List<string> currentDialogue;
    [SerializeField] private TextMeshProUGUI textBox;
    public bool dialogueOn;

    [SerializeField] private NPC_Overworld NPCInteracting;
    public NPC_Overworld npcinteracting => NPCInteracting;

    [SerializeField] private List<Task_TalktoNPC> TalktoNPCTasks = new List<Task_TalktoNPC>();

    public void AddDialogueTasks(Task_TalktoNPC T)
    {
        TalktoNPCTasks.Add(T);
    }

    public Task_TalktoNPC GetDialogueTask(string npcname)
    {
        if (TalktoNPCTasks == null || TalktoNPCTasks.Count < 1) return null;
        for(int i=0;i<TalktoNPCTasks.Count;i++)
        {
            if(npcname == TalktoNPCTasks[i].npcname)
            {
                return TalktoNPCTasks[i];
            }
        }
        return null;
    }

    public void CompleteDialogueTask(Task_TalktoNPC T)
    {
        T.CompleteTask();
        TalktoNPCTasks.Remove(T);
    }

    private void ShakeTextBox()
    {
        textBox.rectTransform.DOShakeAnchorPos(0.5f,new Vector3(2f,10f,0),100,90,false,false);
    }

    public Action onDialogueStart;
    public Action DialogueFunction;
    public Action functiontoUse;

    private Dictionary<string, Action> dialogueEvents = new Dictionary<string, Action>();

    public void InitializeAllDialogue()
    {
        DialogueList dialogueData = SaveLoadManager.Instance.LoadData<DialogueList>("AllNPCDialogue_EN.JSON");

        switch (SettingsManager.Instance.data.Language)
        {
            case GameLanguage.English:
                dialogueData = SaveLoadManager.Instance.LoadData<DialogueList>("AllNPCDialogue_EN.JSON");
                break;
            case GameLanguage.Mandarin:
                dialogueData = SaveLoadManager.Instance.LoadData<DialogueList>("AllNPCDialogue_CH.JSON");
                break;
        }

        if (dialogueData != null && dialogueData.dialogues != null)
        {
            allDialogue.Clear(); // Ensure the list is empty before adding
            allDialogue = dialogueData.dialogues;
            Debug.Log($"Successfully loaded {allDialogue.Count} dialogues!");
        }
        else
        {
            Debug.LogError("Dialogue data is null or empty! Check JSON file.");
        }
    }

    public List<string> getDialogue(string name)
    {
        List<string> T = new List<string>();

        for (int i = 0; i < allDialogue.Count; i++)
        {
            if (allDialogue[i].dialoguename == name)
            {
                T = allDialogue[i].dialogue;
                return T;
            }
        }

        Debug.Log("No Dialogue Found");
        Debug.Log($"{name}");
        return null;
    }

    public void StartDialogue(List<string> texts,NPC_Overworld npc)
    {
        if (texts == null) return;

        if(npc != null)
        {
            NPCInteracting = npc;
        }

        currentDialogue = new List<string>(texts);
        textBox.text = currentDialogue[0];
        textBox.gameObject.SetActive(true);
        CheckCurrentLine(currentDialogue[0]);
        dialogueOn = true;
        onDialogueStart?.Invoke();
        UIManager.Instance.OpenMenubyName("DialogueUI");
    }

    public void skipNextLine()
    {
        if(currentDialogue.Count > 1)
        {
            currentDialogue.RemoveAt(0);
            textBox.text = currentDialogue[0];
            CheckCurrentLine(currentDialogue[0]);
        }

        else
        {
            finishDialogue();
        }
    }

    public void finishDialogue()
    {
        currentDialogue.Clear();
        textBox.text = "";
        textBox.gameObject.SetActive(false);
        UIManager.Instance.CloseMenubyName("DialogueUI");
        if(NPCInteracting !=null)
        {
            NPCInteracting.completeInteraction();
            NPCInteracting = null;
        }
        dialogueOn = false;
        TimeManager.Instance.resumeTimer();
    }

    public void CheckCurrentLine(string s)
    {
        if (dialogueEvents.TryGetValue(s, out Action command))
        {
            DialogueFunction = command;
            DialogueFunction.Invoke();
            Debug.Log($"Action Found for {s}");
        }
    }

    public void handleOnSubmit()
    {
        if (dialogueOn)
        {
            skipNextLine();
        }
    }
}

[System.Serializable]
public class DialogueList
{
    public List<Dialogue> dialogues;
}
