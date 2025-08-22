using System;
using UnityEngine;

public class AreaTransitioner_PlayerHouse : AreaTransitioner
{
    private new void Start()
    {
        base._Interactable.onInteraction += () => { this.teleport(PlayerManager.Instance.player); };
    }

    public void GoHome(Player p,Action A)
    {
        base.teleport(p,A);
    }

    protected override void teleport(Player p)
    {
        if(TimeManager.Instance.state == DayState.Night)
        {
            Debug.Log("Nighttime,cannot go out");
            DialogueManager.Instance.StartDialogue(DialogueManager.Instance.getDialogue("Mom_1"),null);
            base._Interactable.resetInteraction();
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(base._Interactable);
            return;
        }
        base.teleport(p);
    }
}
