using UnityEngine;

public class PlayerWaterDetector : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] public float waterSurfaceY;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            waterSurfaceY = other.bounds.max.y;
            player.setstate(PlayerState.Swimming);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            player.setstate(PlayerState.Airborne);
        }
    }
}
