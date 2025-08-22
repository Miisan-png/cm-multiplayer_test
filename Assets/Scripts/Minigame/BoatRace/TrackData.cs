using System.Collections.Generic;
using UnityEngine;

// Neccessary data to hold for each track like which corner
public class TrackData : MonoBehaviour
{
    [SerializeField] private CornerTile StartCorner;

    public CornerTile startcorner => StartCorner;

    [SerializeField] private List<GameObject> StartPositions;
    public List<GameObject> startpositions => StartPositions;


    [Header("NPC Handling Stats")]
    public float SafeDriftTriggerDistance;
    public float AggressiveDriftTriggerDistance;
    public float WideCornerThreshold;
    public float TightCornerThreshold;
    public float steerRange;
    public float steerDuration;
}
