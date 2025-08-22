using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    [SerializeField] private float BoostAmount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();
            PlayerBoat.HandleSpeedZone(BoostAmount);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController PlayerBoat = other.gameObject.GetComponent<BoatController>();
            PlayerBoat.HandleSpeedZone(0);
        }
    }
}
