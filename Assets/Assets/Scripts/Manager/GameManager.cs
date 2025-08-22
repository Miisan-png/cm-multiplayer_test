using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        gameState state = GameState;
        GameState = gameState.None;
        setState(state);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if(MainMenuUIController.Instance != null)
        {
            MainMenuUIController.Instance.InitializeUI();
        }
    }

    [SerializeField] private gameState GameState;
    public gameState gamestate => GameState;

    private bool SettingsLoaded;
    public bool settingsloaded => SettingsLoaded;

    private Coroutine MainMenuLoadCoroutine;
    private Coroutine MainMenuUnloadCoroutine;
    private Coroutine OverworldLoadCoroutine;
    private Coroutine OverworldUnloadCoroutine;
    private Coroutine BattleStartCoroutine;
    private Coroutine BattleEndCoroutine;
    private Coroutine SettingStartCoroutine;
    private Coroutine SettingEndCoroutine;

    public Action OnSettingsStart;
    public Action OnSettingsEnd;


    private bool DayStarted;
    public bool daystarted => DayStarted;

    [SerializeField] private float GameStartedTime;
    public float totalPlayTime => Time.realtimeSinceStartup - GameStartedTime;

    public void StartGame()
    {
        setState(gameState.Overworld);
        GameStartedTime = Time.realtimeSinceStartup;
    }

    private bool ChangeStateCondition(gameState state)
    {
        return GameState != state;
    }

    public void setState(gameState state)
    {
        if (!ChangeStateCondition(state)) return;

        GameState = state;

        OnGameStateChange?.Invoke(GameState);
    }
    public Action<gameState> OnGameStateChange;

    public delegate void ondayStart();
    public event ondayStart onDayStart;

    public void startDay()
    {
        if (gamestate == gameState.Battle || DayStarted) return;
        setState(gameState.Overworld);
        onDayStart?.Invoke();
        DayStarted = true;
    }

    private bool LoadBattleSceneCondition(BattleGlove p1glove, BattleGlove p2glove)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "Battle_Base")
            {
                return false;
            }
        }

        return BattleManager.Instance.StartNewBattleCondition(p1glove, p2glove) && GameState != gameState.Battle;
    }

    private bool LoadSettingsCondition()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "SettingScene")
            {
                return false;
            }
        }

        return GameState == gameState.MainMenu || GameState == gameState.Overworld;
    }

    public bool LoadMainMenuCondition()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "MainMenuScene")
            {
                return false;
            }
        }
        return true;
    }
    public bool LoadOverworldCondition()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "Overworld_Base")
            {
                return false;
            }
        }
        return true;
    }


    public void LoadMainMenuScene(Action A)
    {
        if (!LoadMainMenuCondition()) return;

        if (MainMenuLoadCoroutine != null)
        {
            StopCoroutine(MainMenuLoadCoroutine);
        }

        MainMenuLoadCoroutine = StartCoroutine(LoadMainMenuSceneAsync(A));
    }

    private IEnumerator LoadMainMenuSceneAsync(Action A)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenuScene", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        setState(gameState.MainMenu);
        MainMenuLoadCoroutine = null;

        A();
    }
    public void LoadOverworldScene(Action A)
    {
        if (!LoadOverworldCondition()) return;

        if (OverworldLoadCoroutine != null)
        {
            StopCoroutine(OverworldLoadCoroutine);
        }

        OverworldLoadCoroutine = StartCoroutine(LoadOverworldSceneAsync(A));
    }

    private IEnumerator LoadOverworldSceneAsync(Action A)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Overworld_Base", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        setState(gameState.Overworld);
        OverworldLoadCoroutine = null;

        yield return null;

        A();
    }

    public void LoadSettingScene(Action A)
    {
        if (!LoadSettingsCondition()) return;

        if(SettingStartCoroutine != null)
        {
            StopCoroutine(SettingStartCoroutine);
        }

        SettingStartCoroutine = StartCoroutine(LoadSettingSceneAsync(A));
    }
    private IEnumerator LoadSettingSceneAsync(Action A)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SettingScene", LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SettingsLoaded = true;
        SettingStartCoroutine = null;
        A();
        OnSettingsStart?.Invoke();
    }

    public void LoadBattleScene(BattleGlove p1glove, BattleGlove p2glove, battleType battletype)
    {
        if (!LoadBattleSceneCondition(p1glove, p2glove)) return;

        if(BattleStartCoroutine != null)
        {
            StopCoroutine(BattleStartCoroutine);
        }

        UIManager.Instance.startFade(1.5f, () => { });
        BattleStartCoroutine = StartCoroutine(LoadBattleSceneAsync(p1glove,  p2glove, battletype));
    }
    private IEnumerator LoadBattleSceneAsync(BattleGlove p1glove, BattleGlove p2glove, battleType battletype)
    {
        yield return new WaitForSeconds(3f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Battle_Base", LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        setState(gameState.Battle);
        UIManager.Instance.endFade();
        BattleManager.Instance.StartBattle(p1glove, p2glove, battletype);

        BattleStartCoroutine = null;
    }
    public void UnloadMainMenuScene(Action A)
    {
        if (MainMenuUnloadCoroutine != null)
        {
            StopCoroutine(MainMenuUnloadCoroutine);
        }

        MainMenuUnloadCoroutine = StartCoroutine(UnloadMainMenuSceneAsync(A));
    }

    private IEnumerator UnloadMainMenuSceneAsync(Action A)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync("MainMenuScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        MainMenuUnloadCoroutine = null;
        A();
    }
    public void UnloadOverworldScene(Action A)
    {
        if (OverworldUnloadCoroutine != null)
        {
            StopCoroutine(OverworldUnloadCoroutine);
        }

        OverworldUnloadCoroutine = StartCoroutine(UnloadOverworldSceneAsync(A));
    }

    private IEnumerator UnloadOverworldSceneAsync(Action A)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync("Overworld_Base");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OverworldUnloadCoroutine = null;
        A();

        DayStarted = false;
    }
    public void UnloadSettingScene(Action A)
    {
        if (SettingEndCoroutine != null)
        {
            StopCoroutine(SettingEndCoroutine);
        }
        SettingsLoaded = false;
        SettingEndCoroutine = StartCoroutine(UnloadSettingAsync(A));
    }

    private IEnumerator UnloadSettingAsync(Action A)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync("SettingScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SettingEndCoroutine = null;
        A();
        OnSettingsEnd?.Invoke();
    }

    private bool LastBattleResult;
    public bool lastbattleresult => LastBattleResult;
    public void UnLoadBattleScene(bool result)
    {
        if(BattleEndCoroutine != null)
        {
            StopCoroutine(BattleEndCoroutine);
        }
        LastBattleResult = result;
        UIManager.Instance.startFade(0,() => {  });
        BattleEndCoroutine = StartCoroutine(UnloadBattleSceneAsync());
    }
    private IEnumerator UnloadBattleSceneAsync()
    {
        yield return new WaitForSeconds(2f);

        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync("Battle_Base");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        setState(gameState.Overworld);

        UIManager.Instance.endFade();

        BattleEndCoroutine = null;
    }

    public delegate void ondayEnd();
    public event ondayEnd onDayEnd;
    public void endDay()
    {
        if (TimeManager.Instance.state != DayState.Night) return;
        onDayEnd?.Invoke();
        DayStarted = false;
    }
}
public enum gameState
{
   None,MainMenu, Overworld, Battle, Minigame
}
