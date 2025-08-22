using NUnit.Framework;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class BoatCamera : MonoBehaviour
{
    private void Start()
    {
        HandleOnTunnelExit();

        Minigame_BoatRace.Instance.onMiniGameStart += HandleOnTunnelExit;
    }

    [SerializeField] private CinemachineCamera BkCamtoUse;
    [SerializeField] private CinemachineCamera FrtCamtoUse;
    [SerializeField] private CinemachineCamera BkCamDefault;
    [SerializeField] private CinemachineCamera FrtCamDefault;
    [SerializeField] private CinemachineCamera BkCamTunnel;
    [SerializeField] private CinemachineCamera FrtCamTunnel;


    public void SetCamera(bool Front)
    {
        if (Front)
        {
            FrtCamtoUse.Priority = 5;
            BkCamtoUse.Priority = 0;
        }
        else
        {
            FrtCamtoUse.Priority = 0;
            BkCamtoUse.Priority = 5;
        }
    }

    public void HandleOnTunnelEntry()
    {
        BkCamDefault.Priority = 0;
        FrtCamDefault.Priority = 0;

        BkCamtoUse = BkCamTunnel;
        FrtCamtoUse = FrtCamTunnel;

        SetCamera(false);
    }

    public void HandleOnTunnelExit()
    {
        BkCamTunnel.Priority = 0;
        FrtCamTunnel.Priority = 0;

        BkCamtoUse = BkCamDefault;
        FrtCamtoUse = FrtCamDefault;

        SetCamera(false);
    }
}
