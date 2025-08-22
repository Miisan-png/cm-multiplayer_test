using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Minigame_BoatRace : Minigame
{
    private static Minigame_BoatRace instance;
    public static Minigame_BoatRace Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private CornerTile StartCorner; //Just assign a start and it will auto work
    [SerializeField] private List<TrackData> AllTrackData;
    [SerializeField] private TrackData CurrentData;
    public TrackData currentdata => CurrentData;
    public Action<TrackData> OnSwitchCourse;

    [SerializeField] private List<BoatController> AllBoats;
    [SerializeField] private List<GameObject> StartPoints;
    [SerializeField] private List<InvisibleDividerTile> AllDividers;
    [SerializeField] private List<StartingLine> AllStartingLines;

    [SerializeField] private int lapstoWin;
    public int lapstowin => lapstoWin;
    public Action OnLapsUpdated;

    private Dictionary<int, int> PlayerLapDictionary = new Dictionary<int, int>();

    [SerializeField] private int PlayerWon;
    public Action<int> OnPlayerFinish;

    [SerializeField] private float startCountdown;
    private Coroutine startCoroutine;
    public Action OnRaceStart;

    [SerializeField] private bool raceStarted;
    public bool racestarted => raceStarted;

    private void Start()
    {
        SelectCourse(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartMinigame();
        }

    }
    public override void StartMinigame()
    {
        base.StartMinigame();

        raceStarted = false;

        PlayerWon = 0;

        for (int i = 0; i < AllBoats.Count; i++)
        {
            if (!PlayerLapDictionary.TryGetValue(AllBoats[i].playernumber, out int lapcount))
            {
                PlayerLapDictionary.Add(AllBoats[i].playernumber, 0);
            }
            else
            {
                PlayerLapDictionary[AllBoats[i].playernumber] = 0;
            }
            AllBoats[i].setState(BoatState.none); // Added line to clear drift stamina
        }

        OnLapsUpdated?.Invoke();
        if (startCoroutine == null)
        {
            startCoroutine = StartCoroutine(countdowntoStart());
        }
    }

    public void SelectCourse(int coursenumber)
    {
        for (int i = 0; i < AllTrackData.Count; i++)
        {
            AllTrackData[i].gameObject.SetActive(false);
        }

        if (AllTrackData[coursenumber] != null)
        {
            StartCorner = AllTrackData[coursenumber].startcorner;
            StartPoints = AllTrackData[coursenumber].startpositions;
            AllTrackData[coursenumber].gameObject.SetActive(true);

            CurrentData = AllTrackData[coursenumber];

            OnSwitchCourse?.Invoke(CurrentData);

            Debug.Log("Changed Course");
        }
    }

    public override void StopMinigame()
    {
        base.StopMinigame();
        for (int i = 0; i < AllBoats.Count; i++)
        {
            AllBoats[i].setState(BoatState.none);
        }
        raceStarted = false;
    }

    private IEnumerator countdowntoStart()
    {

        for (int i = 0; i < AllBoats.Count; i++)
        {
            AllBoats[i].teleport(StartPoints[i].transform.position, StartPoints[i].transform.rotation, BoatState.none);
            //AllBoats[i].ResetStamina();
            AllBoats[i].InGroundingZone = false;
        }

        float timer = 0.3f;

        for (int i = 0; i < AllDividers.Count; i++) //Reset
        {
            AllDividers[i].ResetInvisDivider();
        }
        for (int i = 0; i < AllStartingLines.Count; i++)
        {
            AllStartingLines[i].ResetStartingLine();
        }


        while (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < AllBoats.Count; i++) //Spawn 
        {
            StartCorner.checkForSpawnables(AllBoats[i]);
        }

        yield return new WaitForSecondsRealtime(0.5f);
        yield return new WaitForSecondsRealtime(startCountdown);

        for (int i = 0; i < AllBoats.Count; i++)
        {
            StartCorner.AssigntoNextCorner(AllBoats[i]);
            AllBoats[i].AssignNextCorner(StartCorner, StartCorner.nextcorner[0], false);
            AllBoats[i].UnAssignCurrentCorner();
            AllBoats[i].checkpointdetector.ResetDetector();
            AllBoats[i].checkpointdetector.AddCheckpoint(StartPoints[i]);
            AllBoats[i].setState(BoatState.idle);
        }

        OnRaceStart?.Invoke();
        raceStarted = true;
        startCoroutine = null;
    }

    public void UpdatePlayerLaps(int player) //Gets called in Starting Line
    {
        if (State != MinigameState.Active) return;

        if (PlayerLapDictionary.TryGetValue(player, out int lapcount))
        {
            PlayerLapDictionary[player]++;
        }

        OnLapsUpdated?.Invoke();

        OnReachStartingLine(player);
    }

    public int GetPlayerLaps(int player)
    {
        int lap = 0;

        if (PlayerLapDictionary.TryGetValue(player, out int lapcount))
        {
            lap = lapcount;
        }

        return lap;
    }

    private void ChecktoFinishMinigame()
    {
        if (PlayerWon != 0)
        {
            StopMinigame();
        }
    }

    public void OnPlayerCrash(int crashedplayer)
    {
        if (crashedplayer == 2)
        {
            PlayerWon = 1;
            OnPlayerFinish?.Invoke(1);
        }
        else if (crashedplayer == 1)
        {
            PlayerWon = 2;
            OnPlayerFinish?.Invoke(2);
        }
        ChecktoFinishMinigame();
    }

    public void OnReachStartingLine(int player)
    {
        if (PlayerLapDictionary.TryGetValue(player, out int lapcount))
        {
            if (lapcount >= lapstoWin)
            {
                PlayerWon = player;
                OnPlayerFinish?.Invoke(player);
            }
        }

        ChecktoFinishMinigame();
    }

}
