using NUnit.Framework;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class RacingCameraManager : MonoBehaviour
{
    private static RacingCameraManager instance;
    public static RacingCameraManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SubsribetoBoat();
    }

    private void SubsribetoBoat()
    {
        if (AllPlayerBoats.Count == 0) return;
        for(int i=0;i<AllPlayerBoats.Count;i++)
        {
            AllPlayerBoats[i].SwitchFrtCam += (int player) => { SetCamera(true, player); };
            AllPlayerBoats[i].SwitchBkCam += (int player) => { SetCamera(false, player); };
            SetCamera(false, AllPlayerBoats[i].playernumber);
        }
    }

    public enum cameraState
    {
        Back,Front
    }

    [SerializeField] private List<BoatController_Player> AllPlayerBoats;
    [SerializeField] private Camera mainCamera;

    private void SetCamera(bool FrtCam,int player)
    {
        if (AllPlayerBoats[player - 1].playercamera != null)
        {
            AllPlayerBoats[player - 1].playercamera.SetCamera(FrtCam);
        }
    }
}
