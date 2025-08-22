using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ArtTestUI : MonoBehaviour
{
    [SerializeField] private RectTransform HorizontalArrow;
    [SerializeField] private RectTransform VerticalArrow;
    [SerializeField] private RectTransform HorizontalArrowImage;
    [SerializeField] private RectTransform VerticalArrowImage;

    [SerializeField] private List<RectTransform> HorizontalObjects;
    [SerializeField] private int HorizontalIndex;

    [SerializeField] private List<RectTransform> VerticalObjects;
    [SerializeField] private int VerticalIndex;

    private Tween ImageAnimation;

    public void ResetUI()
    {
        HorizontalIndex = 0;
        VerticalIndex = 0;
        HorizontalArrow.DOAnchorPos(HorizontalObjects[HorizontalIndex].anchoredPosition, 0, false);
        VerticalArrow.DOAnchorPos(VerticalObjects[VerticalIndex].anchoredPosition, 0, false);
    }

    public void HandleUI(Vector2 Input)
    {
        if (Input.x > 0.1f)
        {
            HorizontalIndex++;
            HorizontalIndex = Mathf.Clamp(HorizontalIndex, 0, HorizontalObjects.Count - 1);
            HorizontalArrow.DOAnchorPos(HorizontalObjects[HorizontalIndex].anchoredPosition, 0.1f, false);
            HandleImageAnimation(true);
        }
        else if (Input.x < -0.1f)
        {
            HorizontalIndex--;
            HorizontalIndex = Mathf.Clamp(HorizontalIndex, 0, HorizontalObjects.Count - 1);
            HorizontalArrow.DOAnchorPos(HorizontalObjects[HorizontalIndex].anchoredPosition, 0.1f, false);
            HandleImageAnimation(true);
        }
        if (Input.y > 0.1f)
        {
            VerticalIndex--;
            VerticalIndex = Mathf.Clamp(VerticalIndex, 0, VerticalObjects.Count - 1);
            VerticalArrow.DOAnchorPos(VerticalObjects[VerticalIndex].anchoredPosition, 0.1f, false);
            HandleImageAnimation(false);
        }
        else if (Input.y < -0.1f)
        {
            VerticalIndex++;
            VerticalIndex = Mathf.Clamp(VerticalIndex, 0, VerticalObjects.Count - 1);
            VerticalArrow.DOAnchorPos(VerticalObjects[VerticalIndex].anchoredPosition, 0.1f, false);
            HandleImageAnimation(false);
        }
    }

    private void HandleImageAnimation(bool Horizontal)
    {
        if (ImageAnimation != null)
        {
            ImageAnimation.Kill(); // Just kill the animation without completing it
            ImageAnimation = null;
        }

        // Reset to original position before starting new animation
        if (Horizontal)
        {
            HorizontalArrowImage.anchoredPosition = new Vector2(HorizontalArrowImage.anchoredPosition.x, 0);
            ImageAnimation = HorizontalArrowImage.DOAnchorPosY(15f, 0.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            VerticalArrowImage.anchoredPosition = new Vector2(VerticalArrowImage.anchoredPosition.x, 0);
            ImageAnimation = VerticalArrowImage.DOAnchorPosY(15f, 0.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}