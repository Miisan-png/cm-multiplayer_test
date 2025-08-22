using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerTile : MonoBehaviour
{
    [SerializeField] private TurnDirection DirectionToTurn;
    public TurnDirection directiontoturn => DirectionToTurn;

    [SerializeField] private List<CornerTile> NextCorners;
    public List<CornerTile> nextcorner => NextCorners;

    [SerializeField] private List<BoatController> interactableBoats;

    [SerializeField] private bool Alternatepath = false;
    public bool alternatepath => Alternatepath;

    [Header("Optional References")]
    [SerializeField] private StartingLine StartingLine;
    [SerializeField] private List<InvisibleDividerTile> DividertoActivate;
    [SerializeField] private List<InvisibleDividerTile> DividertoDeactivate;

    public void AddBoattoList(BoatController boat)
    {
        if (!interactableBoats.Contains(boat))
        {
            interactableBoats.Add(boat);
        }
    }
    public void RemoveBoatfromList(BoatController boat)
    {
        if (interactableBoats.Contains(boat))
        {
            interactableBoats.Remove(boat);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();

            if(Alternatepath)
            {
                AddBoattoList(PlayerBoat);
                PlayerBoat.OnEnterAlternateCorner?.Invoke(this.gameObject);
            }


            if (interactableBoats.Contains(PlayerBoat))
            {
                PlayerBoat.AssignNextCorner(this,nextcorner[0],true);

                checkForSpawnables(PlayerBoat);
                AssigntoNextCorner(PlayerBoat);
                TrySpawnStartingLine(PlayerBoat);
            }
        }
    }

    public void checkForSpawnables(BoatController playerboat)
    {
        if(DividertoActivate != null && DividertoActivate.Count>0)
        {
            for(int i=0;i<DividertoActivate.Count;i++)
            {
                DividertoActivate[i].AddBoattoList(playerboat);
            }
        }

        if (DividertoDeactivate != null && DividertoDeactivate.Count > 0)
        {
            for (int i = 0; i < DividertoDeactivate.Count; i++)
            {
                DividertoDeactivate[i].RemoveBoatfromList(playerboat);
            }
        }
    }

    public void AssigntoNextCorner(BoatController playerboat)
    {

        for (int i = 0; i < NextCorners.Count; i++)
        {
            NextCorners[i].AddBoattoList(playerboat);
        }
    }

    private void TrySpawnStartingLine(BoatController playerboat)
    {
        if (StartingLine != null)
        {
            StartingLine.AddBoattoList(playerboat);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();

            if(Alternatepath && PlayerBoat.allowalternatepath)
            {
                PlayerBoat.UnAssignCurrentCorner();
            }
            else if(!Alternatepath)
            {
                PlayerBoat.UnAssignCurrentCorner();
            }
           
            RemoveBoatfromList(PlayerBoat);
        }
    }

}
public enum TurnDirection
{
    Right,Left
}