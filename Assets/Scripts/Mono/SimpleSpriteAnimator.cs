using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SimpleSpriteAnimator : MonoBehaviour
{
    private Tweener AnimationTweener;
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Image Image;

    [SerializeField] private List<Sprite> Sprites;
    [SerializeField] private float SpriteChangeInterval;
    [SerializeField] private float delay;
    private int animationIndex;

    private void OnEnable()
    {
        StartAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void StartAnimation()
    {
        StopAnimation();
        AnimationTweener = DOTween.To(() => 0, x => { }, 0, SpriteChangeInterval)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() =>
                        {
                            if (Sprites.Count == 0) return; 

                            if (Sprites[animationIndex] != null)
                            {
                                if (SR != null)
                                {
                                    SR.sprite = Sprites[animationIndex];
                                }

                                if (Image != null)
                                {
                                    Image.sprite = Sprites[animationIndex];
                                }
                            }

                            animationIndex = (animationIndex + 1) % Sprites.Count;
                        }).SetDelay(delay);
    }
    private void StopAnimation()
    {
        if (AnimationTweener != null)
        {
            AnimationTweener.Complete();
            AnimationTweener.Kill();
            AnimationTweener = null;
        }
    }

}
