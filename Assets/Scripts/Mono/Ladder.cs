using UnityEngine;
using static Player;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject startTransform;
    [SerializeField] private Interactable _Interactable;

    private void Start()
    {
        _Interactable.onInteraction += OnClimbLadder;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnTouchedInteractable(_Interactable);
            OnOffLadder();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
        }
    }

    private void OnClimbLadder()
    {
        Player player = PlayerManager.Instance.player;

        if (player != null && (player.state == PlayerState.Walk || player.state == PlayerState.Idle || player.state == PlayerState.Swimming))
        {
            player.startLerp(startTransform.transform, PlayerState.Climbing, 0.2f, () => { player.transform.rotation = startTransform.transform.rotation; player.OnClimbLadder?.Invoke(); });
        }
    }

    private void OnOffLadder()
    {
        Player player = PlayerManager.Instance.player;
        if (player != null && player.state == PlayerState.Climbing)
        {
            player.setstate(PlayerState.Airborne);
            player.OnOffLadder?.Invoke();
            _Interactable.resetInteraction();
        }
    }
}


