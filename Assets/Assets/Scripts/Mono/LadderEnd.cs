using UnityEngine;
using static Player;

public class LadderEnd :MonoBehaviour
{
    [SerializeField] private GameObject endTransform;
    [SerializeField] private Interactable _Interactable;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player player = other.gameObject.GetComponent<Player>();

            if (player.state == PlayerState.Climbing)
            {
                player.startLerp(endTransform.transform,PlayerState.Idle, 0.2f, () => { player.OnOffLadder?.Invoke(); if (InputManager.Instance.moveaction.IsPressed()) { player.setstate(PlayerState.Walk); }; });
                _Interactable.resetInteraction();
            }
        }
    }
}