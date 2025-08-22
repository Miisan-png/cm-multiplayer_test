using UnityEngine;

public class GroundingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();
            PlayerBoat.InGroundingZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();
            PlayerBoat.InGroundingZone = false;
        }
    }
}
