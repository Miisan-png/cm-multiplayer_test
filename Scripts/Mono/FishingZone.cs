using System.Collections;
using UnityEngine;

public class FishingZone : MonoBehaviour
{
    [SerializeField] Interactable interactable;

    private void Start()
    {
        interactable.inTrigger += () => { };
        interactable.outTrigger += () => { };
        interactable.onInteraction += () => { interactable.switchoffPopup(); interactable.switchoffMultiPopup(); MiniGameManager.Instance.StartMiniGame(MiniGameManager.Instance.fishingminigame); PlayerRotatetoCenter(); };
        MiniGameManager.Instance.onMiniGameEnd += () => { interactable.resetInteraction(); StopAllCoroutines(); interactable.switchonPopup(); PlayerManager.Instance.player.idetector.checkInteractables(); };
    }

    public void PlayerRotatetoCenter()
    {
        StartCoroutine(LooktoWater(3f));
    }

    public IEnumerator LooktoWater(float timer)
    {
        Player P = PlayerManager.Instance.player;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            P.HandleRotationTowards(this.gameObject.transform.position);
            yield return new WaitForFixedUpdate();
        }
    }
}
