using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AreaManager;


public class AreaTransitioner : MonoBehaviour
{
    [SerializeField] private AreaTransitioner otherTransitioner;
    [SerializeField] private GameObject startingPoint;
    [SerializeField] private Areas areatoGo;
    [SerializeField] protected Interactable _Interactable;

    [SerializeField] private CameraDetector camtoGo;

    protected void Start()
    {
        _Interactable.onInteraction += () => {
            teleport(PlayerManager.Instance.player);
        };
    }

    private void OnTriggerEnter(Collider Other)
    {
        if(Other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnTouchedInteractable(_Interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
        }
    }

    protected virtual void teleport(Player p)
    {
        UIManager.Instance.startFade(0,() => {
            if(areatoGo != Areas.None)
            {
                AreaManager.Instance.setArea(areatoGo);
            }

            p.Teleport(otherTransitioner.startingPoint.transform);
            camtoGo.activateCam(false);
            TimeManager.Instance.resumeTimer();
            _Interactable.resetInteraction();
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
            UIManager.Instance.endFade();
        });

    }

    protected virtual void teleport(Player p,Action A)
    {
        UIManager.Instance.startFade(0, () => {
            if (areatoGo != Areas.None)
            {
                AreaManager.Instance.setArea(areatoGo);
            }

            p.Teleport(otherTransitioner.startingPoint.transform);
            camtoGo.activateCam(false);
            TimeManager.Instance.resumeTimer();
            _Interactable.resetInteraction();
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
            A();
            UIManager.Instance.endFade();
        });

    }
}
