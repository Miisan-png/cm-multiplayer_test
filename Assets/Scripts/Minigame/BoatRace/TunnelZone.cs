using System;
using System.Collections.Generic;
using UnityEngine;

public class TunnelZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            BoatController_Player player = other.gameObject.GetComponent<BoatController_Player>();

            if(player != null)
            {
                player.OnEnterTunnel?.Invoke();
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            BoatController_Player player = other.gameObject.GetComponent<BoatController_Player>();

            if (player != null)
            {
                player.OnExitTunnel?.Invoke();
            }
        }
    }
}
