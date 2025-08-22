using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minigame_Fishing : Minigame
{

    [SerializeField] private FishingState fishingState = FishingState.None;

    public void setState(FishingState S)
    {
        fishingState = S;
        switch(fishingState)
        {
            case FishingState.None:

                break;
            case FishingState.Waiting:

                break;
            case FishingState.MiniGame:

                break;
        }
    }

    [SerializeField] private GameObject FishingUI;
    [SerializeField] private ParticleSystem HookedIndicator;
    // phase 1
    [SerializeField] private GameObject PlayerFishingRod;
    [SerializeField] private Coroutine WaitingCoroutine;
    [SerializeField] private bool FishHooked;
    [SerializeField] private bool AttemptHook;
    public bool attempthook => AttemptHook;

    [SerializeField] private Coroutine HookTimerCoroutine;
    // phase 2
    [SerializeField] private Coroutine Phase2Coroutine;
    [SerializeField] private Slider CastConfirmSlider;

    [SerializeField] private Slider LowerBoundarySlider;
    [SerializeField] private Slider UpperBoundarySlider;

    [SerializeField] private float ChangeDirectionTimer;
    [SerializeField] private float UpperBoundary;
    [SerializeField] private float LowerBoundary;

    [SerializeField] private float AnchorDirection;

    [SerializeField] private Item_Fish FishtoCatch;
    [SerializeField] private float AnchorSpeed;
    [SerializeField] private int AnchorChangeFrequency;
    [SerializeField] private float AnchorRange;

    [SerializeField] private bool Result;

    public override void StartMinigame()
    {
        PlayerManager.Instance.player.setstate(PlayerState.None);

        InputManager.Instance.OnCastAnchor += AttempttoHook;
        InputManager.Instance.OnCastConfirm += ConfirmAnchor;

        FishtoCatch = ItemManager.Instance.GetRandomFish();
        AnchorSpeed = FishtoCatch.AnchorSpeed;
        AnchorChangeFrequency = FishtoCatch.AnchorChangeFrequency;
        AnchorRange = FishtoCatch.AnchorRange;

        float rng1 = Random.Range(0.15f, 0.85f);
        LowerBoundary = rng1;
        UpperBoundary = rng1 + AnchorRange;
        LowerBoundarySlider.value = LowerBoundary;
        UpperBoundarySlider.value = UpperBoundary;
        ChangeDirectionTimer = Random.Range(3, 4);
        PlayerFishingRod.SetActive(true);
        int rng2 = Random.Range(3, 11);
        WaitingCoroutine = StartCoroutine(StartPhase1(rng2));
        setState(FishingState.Waiting);

        base.StartMinigame();

    }

    public IEnumerator StartPhase1(float timer)
    {
        Debug.Log("Waiting on Hook!");
        yield return new WaitForSeconds(timer);
        HookTimerCoroutine = StartCoroutine(HookWindow());
        yield break;
    }

    public IEnumerator HookWindow()
    {
        Debug.Log("Fish On Hook!");
        FishHooked = true;
        HookedIndicator.Play();

        yield return new WaitForSeconds(0.5f);

        if(fishingState == FishingState.Waiting)
        {
            StopMinigame();
        }

    }


    public void AttempttoHook()
    {
        if (fishingState != FishingState.Waiting) return;

        if(HookTimerCoroutine != null)
        {
            StopCoroutine(HookTimerCoroutine);
            HookTimerCoroutine = null;
        }

        AttemptHook = true;

        if(FishHooked)
        {
            StartCoroutine(phase2Delay());
        }
        else
        {
            StopMinigame();
        }
    }
    public IEnumerator phase2Delay()
    {
        yield return new WaitForSeconds(0.2f);
        StartPhase2();
    }


    public void StartPhase2()
    {
        TimeManager.Instance.pauseTimer();
        FishHooked = false;
        FishingUI.SetActive(true);
        CastConfirmSlider.value = 0;
        setState(FishingState.MiniGame);
        Phase2Coroutine = StartCoroutine(Phase2Slider());
    }

    public IEnumerator Phase2Slider()
    {
        while(fishingState == FishingState.MiniGame)
        {
            CastConfirmSlider.value = Mathf.MoveTowards(CastConfirmSlider.value, AnchorDirection, AnchorSpeed * Time.fixedDeltaTime);

            ChangeDirectionTimer -= Time.deltaTime;

            if (ChangeDirectionTimer <= 0)
            {
                ChangeAnchorDirection();
                ChangeDirectionTimer = Random.Range(1, AnchorChangeFrequency);
            }
            if (CastConfirmSlider.value >= 1 || CastConfirmSlider.value <= 0)
            {
                ChangeAnchorDirection();
            }
            yield return new WaitForFixedUpdate();
        }
    }


    public void ChangeAnchorDirection()
    {
        switch(AnchorDirection)
        {
            case 1:
                AnchorDirection = 0;
                break;
            case 0:
                AnchorDirection = 1;
                break;
        }
    }

    public void ConfirmAnchor()
    {
        if (fishingState != FishingState.MiniGame) return;
        
        if(CastConfirmSlider.value >= (LowerBoundary - 0.005f) && CastConfirmSlider.value <= (UpperBoundary + 0.005f)) //Extra boundary for fairness
        {
            Result = true;
        }
        else
        {
            Result = false;
        }
        FishingUI.SetActive(false);
        StopMinigame();
    }

    public void CaughtFish()
    {
        List<string> D = new List<string>();
        D.Add($"You've Caught a {FishtoCatch.Name}!");
        DialogueManager.Instance.StartDialogue(D, null);

        PlayerInventory.Instance.addItemtoInventory(FishtoCatch);
                Debug.Log($"You've Caught a {FishtoCatch.Name}!");
    }

    public void PlaySuccessAnim()
    {

    }
    public void PlayFailedAnim()
    {

    }

    public override void StopMinigame()
    {
        switch (Result)
        {
            case true:
                CaughtFish();
                PlaySuccessAnim();
                break;
            case false:
                PlayFailedAnim();
                break;
        }

        InputManager.Instance.OnCastAnchor -= AttempttoHook;
        InputManager.Instance.OnCastConfirm -= ConfirmAnchor;

        AttemptHook = false;
        PlayerFishingRod.SetActive(false);
        PlayerManager.Instance.player.setstate(PlayerState.Idle);
        TimeManager.Instance.resumeTimer();
        setState(FishingState.None);
        StopAllCoroutines();
        Result = false;
        base.StopMinigame();
        Debug.Log("Fishing Done!");
    }
}
public enum FishingState
{
    None,Waiting,MiniGame
}