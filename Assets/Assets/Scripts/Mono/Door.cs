using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private float teleportDelay;
    [SerializeField] private GameObject doorModel;
    [SerializeField] private CameraDetector OutDetector;
    [SerializeField] private CameraDetector InDetector;
    [SerializeField] private Interactable interactable;
    [SerializeField] private DoorDirection state = DoorDirection.Out;
    [SerializeField] private List<Door> LinkedDoors = new List<Door>();
    [SerializeField] private bool interacting;
    private Coroutine doorAnimation;

    private void OnTriggerEnter(Collider Other)
    {
        if (Other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnTouchedInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(interactable);
        }
    }

    private void Start()
    {
        interactable.onInteraction += HandleOnInteract;
    }
    private enum DoorDirection
    {
        Out,In
    }
    private void setState(DoorDirection D)
    {
        state = D;
    }

    private void HandleOnInteract()
    {
        StartCoroutine(delay(teleportDelay, HandleDetectors));
    }

    private void PlayDoorAnimation()
    {
        if(doorAnimation!=null)
        {
            StopCoroutine(doorAnimation);
            doorAnimation = null;
        }

        doorModel.SetActive(false);
        doorAnimation = StartCoroutine(delay(0.6f, () => { doorModel.SetActive(true); }));
    }

    private IEnumerator delay(float timer, Action A)
    {
        yield return new WaitForSeconds(timer);
        A();
    }

    private void HandleDetectors()
    {
        DoorDirection S = DoorDirection.Out;
        switch(state)
        {
            case DoorDirection.Out:
                InDetector.activateCam(true);
                setState(DoorDirection.In);
                S = DoorDirection.In;
                break;
            case DoorDirection.In:
                OutDetector.activateCam(true);
                setState(DoorDirection.Out);
                S = DoorDirection.Out;
                break;
        }

        if(LinkedDoors.Count>0)
        {
            for(int i=0;i<LinkedDoors.Count;i++)
            {
                LinkedDoors[i].setState(S);
            }
        }
        interactable.resetInteraction();
    }

}
