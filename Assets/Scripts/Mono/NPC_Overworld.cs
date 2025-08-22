using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Action = System.Action;

public enum state
{
    Interacting, Idle, Walking
}

public enum QuestState
{
    None, OnQuest
}

public enum NPCBehaviour
{
    Idle,
    WalkingLoop,
    Walking
}
public class NPC_Overworld : MonoBehaviour
{
    [SerializeField] private string npcname;

    [SerializeField] private NPC NPCData;
    public NPC npcData => NPCData;

    [SerializeField] private NPCBehaviour behaviour;
    private Dictionary<NPCBehaviour, Action> NPCBehaviour = new Dictionary<NPCBehaviour, Action>();

    [SerializeField] private Interactable interactable;
    [SerializeField] private int dialogueIndex;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private List<GameObject> path;
    [SerializeField] private int pathIndex;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float fallSpeed;
    [SerializeField] private float delay;
    [SerializeField] private float gravity;

    [SerializeField] private Quaternion initialRotation;
    [SerializeField] private Vector3 initialPos;
    private Tween rotateTween;
    private Tween walkTween;
    [SerializeField] private Quaternion cachedQuaternion;
    private bool rotationSet;
    private Vector3[] pathPoints;

    [SerializeField] private bool resetPosOnDayEnd = true;

    [SerializeField] private state NPCstate;
    [SerializeField] private bool Interactable;

    [SerializeField] private QuestState NPCQstate = QuestState.None;

    private Action QuestAction;

    public void setState(state _state)
    {
        NPCstate = _state;
        switch (NPCstate)
        {
            case state.Interacting:
                break;
            case state.Idle:
                break;
            case state.Walking:
                break;
        }
    }

    public void startWalkCycle(bool looping)
    {
        if (path == null || path.Count < 1) return;

        setState(state.Walking);

        Vector3[] newpath = new Vector3[path.Count];

        for(int i=0;i<path.Count;i++)
        {
            newpath[i] = path[i].transform.position;
        }

        walkTween = rb.DOLocalPath(newpath, walkSpeed, PathType.Linear, PathMode.Full3D)
        .SetEase(Ease.Linear)
        .SetSpeedBased()
        .SetLoops(looping ? -1 : 0)
        .OnUpdate(() => UpdateLookAtTarget())
        .SetLookAt(0.01f);
    }
    private void UpdateLookAtTarget()
    {
        if (walkTween == null || pathPoints == null || pathPoints.Length == 0)
            return;

        // Get the current path progress (0 to 1)
        float progress = walkTween.ElapsedPercentage();

        // Estimate the next target index
        int nextIndex = Mathf.FloorToInt(progress * (pathPoints.Length - 1)) + 1;
        nextIndex %= pathPoints.Length; // Loop around if needed

        // Make the object look at the next point
        Vector3 nextTarget = pathPoints[nextIndex];
        rb.transform.rotation = Quaternion.Lerp(rb.transform.rotation, Quaternion.Euler(nextTarget),turnSpeed * Time.deltaTime);
    }
    
    public void pauseWalkCycle()
    {
        setState(state.Idle);

        if(walkTween != null)
        {
            walkTween.Pause();
        }
    }

    public void resumeWalkCycle()
    {
        if (ResumeWalkCondition())
        {
            walkTween.Play();
        }
    }

    public void resetNPC()
    {
        if(resetPosOnDayEnd)
        {
            transform.position = initialPos;
            transform.rotation = initialRotation;
        }
        
    }

    private void Start()
    {
        if(rb != null)
        {
            rb.isKinematic = true;
        }

        //Defining dictionary for NPC behaviours
        NPCBehaviour[global::NPCBehaviour.Idle] = () => {
            //NPC that idle in one place
            if (interactable != null)  //For NPC that wont walk but can still be talked to
            {
                interactable.inTrigger += () =>
                {
                    AxisConstraint constraint = AxisConstraint.X & AxisConstraint.Z;
                    rotateTween = transform.DOLookAt(PlayerManager.Instance.player.transform.position, 1f, constraint);
                };
                interactable.outTrigger += () => {
                    rotateTween = transform.DORotateQuaternion(initialRotation, 1f);
                };
            }
        };
        NPCBehaviour[global::NPCBehaviour.Walking] = () => {
            //NPC that Walk to a spot on start and stays there.
            if (path.Count > 0) // For NPC that will walk and may be able to talk to
            {
                GameManager.Instance.onDayStart += () => { startWalkCycle(false); }; 
                if (interactable != null)
                {
                    interactable.inTrigger += () => {
                        if (rotateTween != null)
                        {
                            rotateTween.Kill();
                        }
                        if(!rotationSet)
                        {
                            cachedQuaternion = transform.rotation;
                            rotationSet = true;
                        }
                        AxisConstraint constraint = AxisConstraint.X & AxisConstraint.Z;
                        rotateTween = transform.DOLookAt(PlayerManager.Instance.player.transform.position, 1f, constraint);
                        pauseWalkCycle();
                    };
                    interactable.outTrigger += () => {
                        rotateTween = transform.DORotateQuaternion(cachedQuaternion, 1f).OnComplete(() => { resumeWalkCycle(); rotationSet = false; });
                    };
                }
            }
        };
        NPCBehaviour[global::NPCBehaviour.WalkingLoop] = () => {
            //NPC that walk in a loop with given path
            if (path.Count > 0) // For NPC that will walk and may be able to talk to
            {
                GameManager.Instance.onDayStart += () => { startWalkCycle(true); };
                if (interactable != null)
                {
                    interactable.inTrigger += () => {
                        if(rotateTween != null)
                        {
                            rotateTween.Kill();
                        }
                        if(!rotationSet)
                        {
                            cachedQuaternion = transform.rotation;
                            rotationSet = true;
                        }
                        AxisConstraint constraint = AxisConstraint.X & AxisConstraint.Z;
                        rotateTween = transform.DOLookAt(PlayerManager.Instance.player.transform.position, 1f, constraint);
                        pauseWalkCycle();
                    };
                    interactable.outTrigger += () => {
                        rotateTween = transform.DORotateQuaternion(cachedQuaternion, 1f).OnComplete(() => { resumeWalkCycle(); rotationSet = false; });
                    };
                }
            }
        };

        if(Interactable)
        {
            if (interactable != null)
            {
                interactable.gameObject.SetActive(true);
                interactable.onInteraction += SubscribetoInteractable;
            }
        }
        else
        {
            if (interactable != null)
            {
                interactable.gameObject.SetActive(false);
                interactable.onInteraction -= SubscribetoInteractable;
            }
        }

        initialRotation = transform.rotation;
        initialPos = transform.position;
        setState(state.Idle);

        if (npcname != null)
        {
            NPC data = NPCManager.Instance.GetNPCbyName(npcname);
            if(data !=null)
            {
                NPCData = data;
            }
        }

        if (NPCBehaviour.TryGetValue(behaviour, out Action B))
        {
            B();
        }

        GameManager.Instance.onDayEnd += resetNPC;

    }

    private void SubscribetoInteractable()
    {
        if (NPCQstate != QuestState.OnQuest)
        {
            if (!feedDialogue($"{NPCData.Name}_{dialogueIndex}"))
            {
                dialogueIndex = 0;
                feedDialogue($"{NPCData.Name}_{dialogueIndex}");
            }
        }
        else
        {
            QuestAction(); //This will keep feed dialogue for quest overwrites
        }
    }

    private bool ResumeWalkCondition()
    {
        return (behaviour == global::NPCBehaviour.WalkingLoop ||
             behaviour == global::NPCBehaviour.Walking) &&
             GameManager.Instance.gamestate == gameState.Overworld &&
             walkTween != null;
    }

    private void OnEnable()
    {
        if (ResumeWalkCondition())
        {
            resumeWalkCycle();
        }
    }

    private void OnDisable()
    {
        pauseWalkCycle();
    }

    public bool feedDialogue(string dialogueID)
    {
        List<string> _dialogue = DialogueManager.Instance.getDialogue($"{dialogueID}");

        if (_dialogue == null)
        {
            return false;
        }
       
        if (_dialogue != null && _dialogue.Count > 0) // Check for both null and empty list
        {
            DialogueManager.Instance.StartDialogue(_dialogue, this);
            dialogueIndex++;
        }
        else
        {
            switch (NPCQstate)
            {
                case QuestState.None:
                    dialogueIndex = 0;
                    List<string> _dialogue2 = DialogueManager.Instance.getDialogue($"{NPCData.Name}_0");
                    if (_dialogue2 == null) return false;

                    DialogueManager.Instance.StartDialogue(_dialogue2, this);
                    dialogueIndex++;
                    break;
                case QuestState.OnQuest:
                    dialogueIndex = 0;
                    List<string> _dialogue3 = DialogueManager.Instance.getDialogue($"{dialogueID}");
                    if (_dialogue3 == null) return false;

                    DialogueManager.Instance.StartDialogue(_dialogue3, this);
                    dialogueIndex++;
                    break;
            }

        }

        setState(state.Interacting);
        if (interactable != null)
        {
            interactable.switchoffPopup();
        }
        return true;
    }

    public void completeInteraction()
    {
        if (interactable != null)
        {
            interactable.switchonPopup();
            interactable.interacted = false;
        }

        switch(behaviour)
        {
            default:
                setState(state.Idle);
                break;
            case global::NPCBehaviour.Walking:
                setState(state.Walking);
                break;
            case global::NPCBehaviour.WalkingLoop:
                setState(state.Walking);
                break;
        }

    }

    public void HandleQuestOverride(string dialogueID)
    {
        NPCQstate = QuestState.OnQuest;
        dialogueIndex = 0;
        QuestAction = () => { feedDialogue($"{NPCData.Name}_{dialogueID}_{dialogueIndex}"); };
        Debug.Log("Subscribed to Interactable");
    }


    public void HandleQuestComplete()
    {
        NPCQstate = QuestState.None;
        dialogueIndex = 0;
        QuestAction = null;
    }


}
