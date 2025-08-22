using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSpriteAnimator : MonoBehaviour
{
    [SerializeField] private List<Sprite> IdleAnim;
    [SerializeField] private List<Sprite> CurrentAnim;
    [SerializeField] private int frameIndex = 0;
    [SerializeField] private float FPS = 1;
    [SerializeField] private SpriteRenderer SR;

    public void PlayAnimation(int animationIndex)
    {
        StopAllCoroutines();
        frameIndex = 0;
        switch(animationIndex)
        {
            case 1:
                List<Sprite> newanim = new List<Sprite>();
                for(int i=0;i<IdleAnim.Count;i++)
                {
                    newanim.Add(IdleAnim[i]);
                }

                CurrentAnim = newanim;
                break;
        }

        StartCoroutine(animationSequence());
    }

    private IEnumerator animationSequence()
    {
        SR.sprite = CurrentAnim[0];
        yield return new WaitForSeconds(FPS);

        while (true)
        {
            if(frameIndex < CurrentAnim.Count-1)
            {
                frameIndex++;
            }
            else
            {
                frameIndex = 0;
            }

            SR.sprite = CurrentAnim[frameIndex];

            yield return new WaitForSeconds(FPS);
        }
    }

}
