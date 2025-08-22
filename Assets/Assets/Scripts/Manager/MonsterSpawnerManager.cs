using System.Collections.Generic;
using UnityEngine;
using static CalendarManager;

public class MonsterSpawnerManager : MonoBehaviour
{
    private static MonsterSpawnerManager instance;
    public static MonsterSpawnerManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private List<MonsterSpawner> currentSpawners = new List<MonsterSpawner>();
    [SerializeField] private List<GameObject> spawnPoints;
    [SerializeField] private List<AreaSpawnPoints> currentDayPoints;
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private GameObject spawnerParent;

    public void RefreshSpawners()
    {
        // Ensure the pool has enough spawners
        while (currentSpawners.Count < spawnPoints.Count)
        {
            GameObject obj = Instantiate(spawnerPrefab, spawnerParent.transform);
            MonsterSpawner spawner = obj.GetComponent<MonsterSpawner>();
            obj.SetActive(false); // Deactivate initially
            currentSpawners.Add(spawner);
        }

        // Assign spawn points and activate required spawners
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            currentSpawners[i].resetSpawner(spawnPoints[i].transform);
        }

        // Deactivate extra spawners
        for (int i = spawnPoints.Count; i < currentSpawners.Count; i++)
        {
            currentSpawners[i].gameObject.SetActive(false);
        }
    }

    public void DisableAllSpawners()
    {
        foreach (var spawner in currentSpawners)
        {
            spawner.gameObject.SetActive(false);
        }
    }

    public void SetCurrentSpawnPoints()
    {
        if (CalendarManager.Instance.daystate == CalendarManager.DayState.Weekend)
        {
            DisableAllSpawners();
            return;
        }

        CalendarDay today = CalendarManager.Instance.day;

        if(currentDayPoints!=null && currentDayPoints.Count>0)
        {
            for (int i = 0; i < currentDayPoints.Count; i++)
            {
                if (currentDayPoints[i].daytoSpawn == today)
                {
                    spawnPoints = currentDayPoints[i].spawnPoints;
                }
            }
        }

        RefreshSpawners();
        
    }

    private void Start()
    {
        GameManager.Instance.onDayStart += () => { SetCurrentSpawnPoints(); };
        GameManager.Instance.onDayEnd += () => { DisableAllSpawners(); };
    }
}
