using UnityEngine;

public class PlayerBed : MonoBehaviour
{
    [SerializeField] private GameObject TopBunkerCharacter;
    [SerializeField] private GameObject BtmBunkerCharacter;
    [SerializeField] private Interactable _Interactable;

    [SerializeField] private GameObject SleepEndDetector;

    private void Start()
    {
        _Interactable._Interactable = TimeManager.Instance.state == DayState.Night;
        _Interactable.onInteraction += GotoSleep;

        TimeManager.Instance.on8PM += ActivateBed;
    }

    private void ActivateBed()
    {
        _Interactable._Interactable = true;
    }

    private void GotoSleep()
    {
        InputManager.Instance.ActivateInputs(false);
        GameManager.Instance.endDay();

        PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
        _Interactable._Interactable = false;
        _Interactable.resetInteraction();
        PlayerManager.Instance.player.gameObject.SetActive(false);

        GameObject charobj = PlayerInventory.Instance.playergender == playerGender.Female ? TopBunkerCharacter : BtmBunkerCharacter;
        charobj.SetActive(true);

        UIManager.Instance.startFade(1f, () => {
            PlayerManager.Instance.player.gameObject.SetActive(true);
            TopBunkerCharacter.SetActive(false);
            BtmBunkerCharacter.SetActive(false);
            GameManager.Instance.startDay();
            UIManager.Instance.endFade();

            PlayerManager.Instance.player.Teleport(SleepEndDetector.transform);
        });
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnTouchedInteractable(_Interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
        }
    }
}
