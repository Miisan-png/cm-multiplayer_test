using System.Collections.Generic;
using UnityEngine;

public class ContactOffsetSetter : MonoBehaviour
{
    [SerializeField] private List<ColliderSets> AllSetsOfColliders;
    [SerializeField] private bool setonStart;

    private void Start()
    {
        if(setonStart)
        {
            SetAllColliders();
        }
    }

    private void SetAllColliders()
    {
        if (AllSetsOfColliders.Count == 0 || AllSetsOfColliders == null) return;

        for (int i = 0; i < AllSetsOfColliders.Count; i++)
        {
            for (int u = 0; u < AllSetsOfColliders[i].Colliders.Count; u++)
            {
                AllSetsOfColliders[i].Colliders[u].contactOffset = AllSetsOfColliders[i].ContactOffset;
            }
        }
    }
}

[System.Serializable]
public class ColliderSets
{
    public List<Collider> Colliders;
    public float ContactOffset;
}