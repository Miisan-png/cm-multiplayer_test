using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> Obstacles = new List<GameObject>();
    [SerializeField] private List<GameObject> ChildObstacles = new List<GameObject>();
    [SerializeField] private List<ParticleSystem> Indicators = new List<ParticleSystem>();
    [SerializeField] private float ActiveDuration;

    [SerializeField] private float CooldownDuration;
    [SerializeField] private float CooldownTimer;

    [SerializeField] private ObstacleSpawner_Detector spawnDetector;
    [SerializeField] private bool HasActivated;
    [SerializeField] private bool SpawnInSequence;
    [SerializeField] private float SequenceDelay = 0.2f;
    private Coroutine spawnSequence;
    private Coroutine spawnDelay;


    private void Start()
    {
        Minigame_BoatRace.Instance.onMiniGameStart += () => {
            DeactivateSpawner(0.1f);
        };
        spawnDetector.OnPlayerEnter += () => { 
            if(spawnSequence == null)
            {
                spawnSequence = StartCoroutine(SpawnSequence());
            }
        } ;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowIndicators();
        }
    }

    private void DeactivateSpawner(float timer)
    {
        StopAllCoroutines();
        spawnSequence = null; 
        spawnDelay = null;
        for (int i = 0; i < ChildObstacles.Count; i++)
        {
            ChildObstacles[i].SetActive(false);
        }
        for (int i = 0; i < Obstacles.Count; i++)
        {
            Obstacles[i].SetActive(false);
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StartCooldown(timer));
        }
    }

    private IEnumerator StartCooldown(float timer)
    {
        CooldownTimer = timer;
        while(CooldownTimer > 0)
        {
            CooldownTimer -= Time.deltaTime;
            yield return null;
        }
        CooldownTimer = 0;
        HasActivated = false;
    }

    private bool SpawnCondition()
    {
        return CooldownTimer <= 0 && !HasActivated;
    }

    private void ShowIndicators()
    {
        if (!SpawnCondition()) return;

        Vector3 pos = Vector3.zero;
        for (int i = 0; i < Obstacles.Count; i++)
        {
            Obstacles[i].SetActive(true);
        }
        for (int i = 0; i < Indicators.Count; i++)
        {
            Indicators[i].Play();
        }

        HasActivated = true;
    }

    private void HideIndicators()
    {
        for (int i = 0; i < Indicators.Count; i++)
        {
            Indicators[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void SpawnObstacles()
    {
        HideIndicators();

        if(SpawnInSequence)
        {
            if(spawnDelay==null)
            {
                spawnDelay = StartCoroutine(SpawnDelay(SequenceDelay));
            }
        }
        else
        {
            for (int i = 0; i < ChildObstacles.Count; i++)
            {
                ChildObstacles[i].SetActive(true);
            }

        }
        Debug.Log("spawnedd");
    }

    private IEnumerator SpawnDelay(float timer)
    {
        int i = 0;
        while(true)
        {
            if(i >=3)
            {
                yield break;
            }

            Obstacles[i].SetActive(true);
            yield return new WaitForSecondsRealtime(timer);
            i++;
        }

    }

    private IEnumerator SpawnSequence()
    {
        if (!HasActivated)
        {
            yield break;
        }

        SpawnObstacles();

        yield return new WaitForSecondsRealtime(ActiveDuration);

        //Play closing anim here in future

        DeactivateSpawner(CooldownDuration);
    }

}

