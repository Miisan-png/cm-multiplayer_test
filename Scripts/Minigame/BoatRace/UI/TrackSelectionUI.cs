using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TrackSelectionUI : UIBase
{
    [SerializeField] private GameObject BackBtn;
    private void Start()
    {
        PauseUIController.Instance.OnPauseStart += () => {
            if(Minigame_BoatRace.Instance.racestarted)
            {
                gameObject.SetActive(true);
            }
        };
        PauseUIController.Instance.OnPauseEnd += () => {
            if (Minigame_BoatRace.Instance.racestarted)
            {
                gameObject.SetActive(false);
            }
        };
        Minigame_BoatRace.Instance.onMiniGameEnd += () => { gameObject.SetActive(false); };
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Minigame_BoatRace.Instance.racestarted)
        {
            BackBtn.SetActive(true);
        }
        else
        {
            BackBtn.SetActive(false);
        }
    }
}