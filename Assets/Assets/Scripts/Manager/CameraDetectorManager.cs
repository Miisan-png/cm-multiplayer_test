using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CameraDetectorManager : MonoBehaviour
{
    [SerializeField] private Dictionary<int, CameraDetector> DetectorDictionary = new Dictionary<int, CameraDetector>();
    [SerializeField] private int TotalDetectorIndex;
    [SerializeField] private int LastDetectorIndex;

    public void OnDetectorActivate(CameraDetector D)
    {
        if(DetectorDictionary.TryGetValue(LastDetectorIndex,out D))
        {
            DetectorDictionary[LastDetectorIndex] = D;
        }

    }


}
