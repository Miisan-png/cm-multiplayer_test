using DG.Tweening;
using System.Numerics;
using UnityEngine;

public class MirrorAnimator : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private Material Mat;
    [SerializeField] private Color ActivatedColor;
    private Tween ColorLerpTweener;
    private void LerpCamera(bool Activate)
    {
        if(ColorLerpTweener != null)
        {
            ColorLerpTweener.Kill();
        }

        float originalfloat = cam.farClipPlane;

        if(Activate)
        {
            ColorLerpTweener = Mat.DOColor(ActivatedColor, 0.5f);
        }
        else
        {
            ColorLerpTweener = Mat.DOColor(Color.clear, 0.3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            LerpCamera(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            LerpCamera(false);
        }
    }
}
