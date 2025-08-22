using System;
using UnityEngine;

public class ObstacleSpawner_Detector : MonoBehaviour
{
    public Action OnPlayerEnter;
    [SerializeField] private SphereCollider Detector;
    public SphereCollider detector => Detector;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke();
        }
    }
}
