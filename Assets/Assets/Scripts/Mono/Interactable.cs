using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [SerializeField] GameObject UIPopUp;
    public Interaction interactionState;
    [SerializeField] string closeAnimName;
    [SerializeField] GameObject QPopUp;

    public delegate void Interacted();
    public event Interacted onInteraction;

    public delegate void InTrigger();
    public event InTrigger inTrigger;

    public delegate void OutTrigger();
    public event OutTrigger outTrigger;

    [SerializeField] public bool _Interactable = true;
    [SerializeField] public bool interacted;
    public enum Interaction
    {
        Interacting, notInteracting
    }

    [SerializeField] private BillBoardMode billBoardMode = BillBoardMode.Self;
    [SerializeField] private string BubbletoActivate;
    private enum BillBoardMode
    {
        Self, Player
    }

    public void setState(Interaction _state)
    {
        interactionState = _state;

        switch (interactionState)
        {
            case Interaction.Interacting:
                switchonPopup();
                inTrigger?.Invoke();
                break;

            case Interaction.notInteracting:
                switchoffPopup();
                switchoffMultiPopup();
                outTrigger?.Invoke();
                break;
        }
    }

    public void startInteraction()
    {
        if (interactionState == Interaction.Interacting && !interacted && _Interactable)
        {
            interacted = true;
            onInteraction?.Invoke();
        }
    }

    public void resetInteraction()
    {
        interacted = false;
    }

    public void switchonPopup()
    {
        switch(billBoardMode)
        {
            case BillBoardMode.Self:
                if (UIPopUp != null)
                {
                    UIPopUp.SetActive(true);
                }
                break;
            case BillBoardMode.Player:
                PlayerManager.Instance.player.bubble.HandleBubble(BubbletoActivate, true);
                break;
        }

    }
    public void switchoffPopup()
    {
        switch (billBoardMode)
        {
            case BillBoardMode.Self:
                if (UIPopUp != null)
                {
                    UIPopUp.SetActive(false);
                }
                break;
            case BillBoardMode.Player:
                PlayerManager.Instance.player.bubble.HandleBubble(BubbletoActivate, false);
                break;
        }
    }
    public void switchonMultiPopup()
    {
        switch (billBoardMode)
        {
            case BillBoardMode.Self:
                if (QPopUp != null)
                {
                    QPopUp.SetActive(true);
                }
                break;
            case BillBoardMode.Player:
                PlayerManager.Instance.player.bubble.HandleBubble("Multi", true);
                break;
        }
    }

    public void switchoffMultiPopup()
    {
        switch (billBoardMode)
        {
            case BillBoardMode.Self:
                if (QPopUp != null)
                {
                    QPopUp.SetActive(false);
                }
                break;
            case BillBoardMode.Player:
                PlayerManager.Instance.player.bubble.HandleBubble("Multi", false);
                break;
        }
    }


    public delegate void InteractableDisabled(Interactable interactable);
    public static event InteractableDisabled OnInteractableDisabled;

    private void OnDisable()
    {
        // Notify the detector that this interactable is disabled
         OnInteractableDisabled?.Invoke(this);
    }


}