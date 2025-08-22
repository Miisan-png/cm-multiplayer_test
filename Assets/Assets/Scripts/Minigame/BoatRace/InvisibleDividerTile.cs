using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleDividerTile : MonoBehaviour
{
    [SerializeField] private List<BoatController> interactableBoats;
    [SerializeField] private string SeenLayerName;
    [SerializeField] private string UnseenLayerName;

    public void AddBoattoList(BoatController boat)
    {
        if (!interactableBoats.Contains(boat))
        {
            interactableBoats.Add(boat);
            if(boat.playernumber == 1)
            {
                this.gameObject.layer = LayerMask.NameToLayer($"{SeenLayerName}");
            }
        }
    }
    public void RemoveBoatfromList(BoatController boat)
    {
        if(interactableBoats.Contains(boat))
        {
            interactableBoats.Remove(boat);
            if (boat.playernumber == 1)
            {
                this.gameObject.layer = LayerMask.NameToLayer($"{UnseenLayerName}");
            }
        }
    }
    public void ResetInvisDivider()
    {
        interactableBoats.Clear();
        this.gameObject.layer = LayerMask.NameToLayer($"{UnseenLayerName}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();
            if(interactableBoats.Contains(PlayerBoat))
            {
                PlayerBoat.OnHitDivider();
                Debug.Log("Player Exists and Hits");
            }
        }
    }

}
