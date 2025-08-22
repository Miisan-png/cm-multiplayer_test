using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RacingUIManager : MonoBehaviour
{
    private static RacingUIManager instance;
    public static RacingUIManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        playerboat.ondriftUsage_UI += handleDriftSlider;    
        playerboat.onSpeedChange += (float percentage) => { handleSpeed(percentage);}; 
        //playerboat.BoostTimerUI += handleBoostSlider; //当 playerboat 的 Boost 进度变化时，调用 handleBoostSlider() 函数来更新UI。
               

        Minigame_BoatRace.Instance.onMiniGameStart += () => { StartCoroutine(StartCountdown()); };
        Minigame_BoatRace.Instance.OnLapsUpdated += handleLaps;
        Minigame_BoatRace.Instance.OnPlayerFinish += (int player) => { handleGameOver(player); }; 
    }

    [SerializeField] private Slider DriftSlider; 
    [SerializeField] private BoatController_Player playerboat;

    //[SerializeField] private Slider BoostSlider;

    [SerializeField] private GameObject SpeedMeterObj;
    [SerializeField] private float minAngle;  // 0% speed angle
    [SerializeField] private float maxAngle;   // 100% speed angle
    [SerializeField] private float trueMaxAngle; // Actual maximum when at 100% speed
    private float currentNeedleAngle;

    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private GameObject popupParent;

    [SerializeField] private TextMeshProUGUI LapsCounter;

    [SerializeField] private TextMeshProUGUI DistanceCounter;

    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private Image GameOverImage;
    [SerializeField] private Sprite WinSprite;
    [SerializeField] private Sprite LoseSprite;

    private IEnumerator StartCountdown()
    {
        GameObject obj = popupPrefab;
        UI_PopUp popup = obj.GetComponent<UI_PopUp>();
        yield return new WaitForSecondsRealtime(0.5f);
        popup.setText("3");
        Instantiate(popup,popupParent.transform);
        yield return new WaitForSecondsRealtime(1f);
        popup.setText("2");
        Instantiate(popup, popupParent.transform);
        yield return new WaitForSecondsRealtime(1f);
        popup.setText("1");
        Instantiate(popup, popupParent.transform);
        yield return new WaitForSecondsRealtime(1f);
        popup.setText("GO!");
        Instantiate(popup, popupParent.transform);
    }

    private void handleLaps()
    {
        LapsCounter.text = $"Laps: {Minigame_BoatRace.Instance.GetPlayerLaps(1)}/{Minigame_BoatRace.Instance.lapstowin}";
    }

    private void handleGameOver(int playerwon)
    {
        Sprite spritetouse = null;
        if(playerwon ==1)
        {
            spritetouse = WinSprite;
        }
        else
        {
            spritetouse = LoseSprite;
        }

        GameOverImage.sprite = spritetouse;
        GameOverUI.SetActive(true);
    }

    private void handleSpeed(float percentage)
    {
        percentage = Mathf.Clamp01(percentage);

        // Calculate target angle
        float targetAngle;

        if (playerboat.inspeedboost)
        {
            // During slingshot - use full range
            targetAngle = Mathf.Lerp(minAngle, maxAngle, percentage);
        }
        else
        {
            // Normal operation - use limited range
            targetAngle = Mathf.Lerp(minAngle, minAngle + trueMaxAngle, percentage);
        }

        // Smoothly lerp to target angle
        currentNeedleAngle = Mathf.Lerp(currentNeedleAngle, targetAngle, 10f * Time.deltaTime);

        // Apply rotation
        SpeedMeterObj.transform.localEulerAngles = new Vector3(0, 0, currentNeedleAngle);
    }
    private void handleDriftSlider(float value)
    {
        DriftSlider.value = value;
    }

    // private void handleBoostSlider(float value) //当接收到 BoostTimer 的事件时，会将 UI 的滑条更新为对应的进度（value 是 0~1）
    // {
    //     //BoostSlider.value = value; 
    // }


}
