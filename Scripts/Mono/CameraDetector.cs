using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraDetector : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cameratohold;
    [SerializeField] private Sprite BGSpriteMorning;
    public Sprite bgspritemorning => BGSpriteMorning;
    [SerializeField] private Sprite BGSpriteNoon;
    public Sprite bgspritenoon => BGSpriteNoon;
    [SerializeField] private Sprite BGSpriteNight;
    public Sprite bgspritenight => BGSpriteNight;
    [SerializeField] private List<GameObject> ObjectstoSpawn;

    [SerializeField] private GameObject SpawnPoint;
    public Transform spawnpoint => SpawnPoint.transform;

    [SerializeField] private GameObject ControlForward;
    public Transform forwardpoint => ControlForward.transform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerCameraDetector"))
        {
            StartCoroutine(Delay(() => { activateCam(true); } ));
        }      
    }

    public void activateCam(bool teleport)
    {
        if(CameraManager.instance.currentcam == cameratohold) return;

        Sprite bgsprite = BGSpriteMorning;

        switch (TimeManager.Instance.state)
        {
            case DayState.Afternoon:
                if(BGSpriteNoon != null)
                {
                    bgsprite = BGSpriteNoon;
                }
                break;
            case DayState.Night:
                if(BGSpriteNight != null)
                {
                    bgsprite = BGSpriteNight;
                }
                break;
        }

        CameraManager.instance.SwitchtoCam(cameratohold, bgsprite, ObjectstoSpawn, this, ControlForward);

        if(teleport)
        {
            StartCoroutine(Delay(() => { PlayerManager.Instance.teleportPlayer(SpawnPoint); }));
        }
    }

    private IEnumerator Delay(Action A)
    {
        yield return null;
        A();
    }
}
