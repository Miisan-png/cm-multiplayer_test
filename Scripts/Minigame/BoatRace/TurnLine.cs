using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TurnLine : MonoBehaviour
{
    [SerializeField] private MeshRenderer Mesh;
    [SerializeField] private Material NormalMat;
    [SerializeField] private Material HighlightMat;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BoatController_Player PlayerBoat = other.gameObject.GetComponent<BoatController_Player>();
            if (PlayerBoat == null) return;
            if(!PlayerBoat.indrift)
            {
                PlayerBoat.OnTurnLine = true;
            }
            Mesh.material = HighlightMat;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(TurnOffMat());
        }
    }

    private IEnumerator TurnOffMat()
    {
        yield return new WaitForSecondsRealtime(1f);
        Mesh.material = NormalMat;
    }
}
