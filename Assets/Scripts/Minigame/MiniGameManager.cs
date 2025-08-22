using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    private static MiniGameManager instance;
    public static MiniGameManager Instance => instance;

    public void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        TimeManager.Instance.on8PM += ForceExitCurrentMinigame;
    }

    [SerializeField] private Minigame Fishingminigame;
    public Minigame fishingminigame => Fishingminigame;
    [SerializeField] private Minigame Arttest;
    public Minigame arttest => Arttest;
    [SerializeField] private Minigame Sciencetest;
    public Minigame sciencetest => Sciencetest;

    [SerializeField] private Minigame OngoingMinigame;
    [SerializeField] private gameState CachedState;

    public delegate void minigameStart();
    public event minigameStart onMiniGameStart;

    public delegate void minigameEnd();
    public event minigameEnd onMiniGameEnd;

    private Coroutine ExitCoroutine;

    public void StartMiniGame(Minigame minigametostart)
    {
        if (OngoingMinigame != null) return;

        if (ExitCoroutine != null)
        {
            StopCoroutine(ExitCoroutine);
        }

        minigametostart.StartMinigame();
        OngoingMinigame = minigametostart;
        CachedState = GameManager.Instance.gamestate;
        GameManager.Instance.setState(gameState.Minigame);
        InputManager.Instance.ActivateInputs(false);

        onMiniGameStart?.Invoke();
    }

    public void EndCurrentMinigame()
    {
        if (OngoingMinigame == null) return;

        OngoingMinigame = null;
        GameManager.Instance.setState(CachedState);

        if(ExitCoroutine != null)
        {
            StopCoroutine(ExitCoroutine);
        }

        ExitCoroutine = StartCoroutine(ExitDelay());

        onMiniGameEnd?.Invoke();
    }
    public void ForceExitCurrentMinigame()
    {
        if (OngoingMinigame == null) return;

        OngoingMinigame.StopMinigame();
        OngoingMinigame = null;
        GameManager.Instance.setState(CachedState);

        if (ExitCoroutine != null)
        {
            StopCoroutine(ExitCoroutine);
        }

        ExitCoroutine = StartCoroutine(ExitDelay());

        onMiniGameEnd?.Invoke();
    }

    private IEnumerator ExitDelay()
    {
        yield return null;
        InputManager.Instance.ActivateInputs(true);
    }

}
