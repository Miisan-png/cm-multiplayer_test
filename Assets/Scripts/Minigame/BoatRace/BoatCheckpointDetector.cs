using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BoatCheckpointDetector : MonoBehaviour
{
    [SerializeField] private GameObject LatestCheckpoint;
    public GameObject latestcheckpoint => LatestCheckpoint;

    [SerializeField] private List<GameObject> PassedCheckPoints = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Overworld_Checkpoint"))
        {
            AddCheckpoint(other.gameObject);
        }
    }
    public void AddCheckpoint(GameObject obj)
    {
        if (!PassedCheckPoints.Contains(obj))
        {
            LatestCheckpoint = obj;
            PassedCheckPoints.Add(obj);
        }
    }
    public void ResetDetector()
    {
        PassedCheckPoints.Clear();
    }

}
