using System.Collections.Generic;
using UnityEngine;
using static CalendarManager;

public class AreaSpawnPoints : MonoBehaviour
{
    public List<GameObject> spawnPoints;
    [SerializeField] public CalendarDay daytoSpawn;
}
