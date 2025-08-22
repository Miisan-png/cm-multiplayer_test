using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractableDetector : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private List<Interactable> Interactables;
    [SerializeField] private Interactable hoverInteractable;
    public Interactable HoverInteractable => hoverInteractable;
    [SerializeField] private List<Interactable> BlockedInteractables = new List<Interactable>();

    [SerializeField] private LayerMask visibleLayers;
    private RaycastHit lastHitInfo;
    private Vector3 lastDirection;
    private float lastDistance;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            Interactable _interactable = other.gameObject.GetComponent<Interactable>();

            HandleOnTouchedInteractable(_interactable);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            Interactable _interactable = other.gameObject.GetComponent<Interactable>();

            HandleOnLeaveInteractable(_interactable);
        }
    }

    public void HandleOnTouchedInteractable(Interactable _interactable)
    {
        if (_interactable == null || !_interactable._Interactable) return;

        if(Interactables.Contains(_interactable) && BlockedInteractables.Contains(_interactable)) return;

        if (isInteractableVisible(_interactable))
        {
            Interactables.Add(_interactable);
            if (hoverInteractable == null)
            {
                SetHoverInteractable(_interactable);
            }
            checkInteractables();
        }
        else
        {
            BlockedInteractables.Add(_interactable);
        }
    }
    public void HandleOnLeaveInteractable(Interactable _interactable)
    {
        if (_interactable == null) return;
        Interactables.Remove(_interactable);
        BlockedInteractables.Remove(_interactable);

        if (hoverInteractable == _interactable)
        {
            if (Interactables.Count > 0)
            {
                SetHoverInteractable(Interactables[Interactables.Count - 1]);
            }
            else
            {
                hoverInteractable?.setState(Interactable.Interaction.notInteracting);
                hoverInteractable = null;
            }
        }
        checkInteractables();
    }
    public void StartHoverInteraction()
    {
        hoverInteractable.startInteraction();
    }

    private void UpdateVisibility()
    {
        // Check if visible interactables became blocked
        for (int i = Interactables.Count - 1; i >= 0; i--)
        {
            Interactable interactable = Interactables[i];

            if (!isInteractableVisible(interactable))
            {
                Interactables.RemoveAt(i);
                BlockedInteractables.Add(interactable);

                if (hoverInteractable == interactable)
                {
                    hoverInteractable.setState(Interactable.Interaction.notInteracting);
                    hoverInteractable = null;
                }

                checkInteractables();
            }
        }

        // Check if blocked interactables became visible
        for (int i = BlockedInteractables.Count - 1; i >= 0; i--)
        {
            Interactable interactable = BlockedInteractables[i];

            if (isInteractableVisible(interactable))
            {
                BlockedInteractables.RemoveAt(i);
                Interactables.Add(interactable);

                if (hoverInteractable == null)
                {
                    SetHoverInteractable(interactable);
                }

                checkInteractables();
            }
        }
    }


    public bool isInteractableVisible(Interactable _interactable)
    {
        lastDirection = (_interactable.transform.position - transform.position).normalized;
        lastDistance = Vector3.Distance(transform.position, _interactable.transform.position);

        if (Physics.SphereCast(transform.position, 0.5f, lastDirection, out lastHitInfo, lastDistance, visibleLayers))
        {
            return lastHitInfo.collider.gameObject == _interactable.gameObject;
        }
        return false;
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw the sphere cast visualization
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, lastDirection * lastDistance);

        // Draw hit point if there was a hit
        if (lastHitInfo.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastHitInfo.point, 0.5f);
            Gizmos.DrawSphere(lastHitInfo.point, 0.1f);
        }
    }

    public void changeInteractables()
    {
        if (Interactables.Count < 1 || player.state == PlayerState.None) return;

         int currentIndex = Interactables.IndexOf(hoverInteractable);
        if(currentIndex >= Interactables.Count-1)
        {
            SetHoverInteractable(Interactables[0]);
        }
        else
        {
            SetHoverInteractable(Interactables[currentIndex+1]);
        }

        checkInteractables();
    }

    private void SetHoverInteractable(Interactable newInteractable)
    {
        if (hoverInteractable != null)
        {
            hoverInteractable.setState(Interactable.Interaction.notInteracting);
        }

        hoverInteractable = newInteractable;
        hoverInteractable.setState(Interactable.Interaction.Interacting);
    }

    public void checkInteractables()
    {
        if (Interactables.Count < 1 || player.state == PlayerState.None) return;

        if (Interactables.Count > 1)
        {
            if (hoverInteractable != null)
            {
                hoverInteractable.switchonMultiPopup();
            }
        }

        else if (Interactables.Count < 2)
        {
            if (hoverInteractable != null)
            {
                hoverInteractable.switchoffMultiPopup();
            }
        }
    }
    private void RemoveInteractable(Interactable interactable)
    {
        if (Interactables.Contains(interactable))
        {
            Interactables.Remove(interactable);

            if (hoverInteractable == interactable)
            {
                if (Interactables.Count > 0)
                {
                    SetHoverInteractable(Interactables[Interactables.Count - 1]);
                }
                else
                {
                    hoverInteractable.setState(Interactable.Interaction.notInteracting);
                    hoverInteractable = null;
                }
            }

            checkInteractables();
        }
    }

    private void Start()
    {
        InputManager.Instance.OnNextInteract += changeInteractables;
        Interactable.OnInteractableDisabled += RemoveInteractable;
        if(player == null)
        {
            player = PlayerManager.Instance.player;
        }
        player.WhileMoving += UpdateVisibility;
    }

}
