using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Animator_Sprite : MonoBehaviour
{
    [SerializeField] Vector3 Origin;
    [SerializeField] List<Sprite> currentAnim = new List<Sprite>();

    [SerializeField] List<Sprite> idleAnim;
    [SerializeField] List<Sprite> atkAnim;
    [SerializeField] SpriteRenderer sr;

    [SerializeField] int fps;
    [SerializeField] private int animIndex;

    public bool ispaused;

    public Action onNudgeDone;

    //[SerializeField] List<Sprite> addedAnim = null;
    //[SerializeField] bool addedanimLoop;

    private Coroutine NudgeCoroutine;
    private Coroutine AnimCoroutine;


    private void Start()
    {
        Origin = transform.position;
    }

    public void SetAnim(List<Sprite> anim, bool loop)
    {
        if(anim.Count<1)
        {
            anim = idleAnim;
        }

       if(AnimCoroutine != null)
       {
            StopCoroutine(AnimCoroutine);
       }
        AnimCoroutine = StartCoroutine(PlayAnim(anim, loop));
    }
    public void SetAnim(List<Sprite> anim, bool loop,int frametoAction,Action actiontoUse)
    {
        if (anim.Count < 1)
        {
            anim = idleAnim;
        }

        if (AnimCoroutine != null)
        {
            StopCoroutine(AnimCoroutine);
        }
        AnimCoroutine = StartCoroutine(PlayAnim(anim, loop, frametoAction, actiontoUse));
    }


    public void Nudge(AnimDirection direction)
    {
        if (NudgeCoroutine != null)
        {
            StopCoroutine(NudgeCoroutine);
            NudgeCoroutine = null;
        }

        int nudgeAmount = 3;
        Vector3 Direction = new Vector3(transform.position.x, transform.position.y+nudgeAmount, transform.position.z);

        switch(direction)
        {
            case AnimDirection.Right:
                nudgeAmount = 3;
                Direction = new Vector3(transform.position.x + nudgeAmount, transform.position.y , transform.position.z);
                NudgeCoroutine =  StartCoroutine(move(Direction, 0.2f));
                break;
            case AnimDirection.Left:
                nudgeAmount = -3;
                Direction = new Vector3(transform.position.x + nudgeAmount, transform.position.y , transform.position.z);
                NudgeCoroutine = StartCoroutine(move(Direction, 0.2f));
                break;
            case AnimDirection.Up:
                nudgeAmount = 1;
                Direction = new Vector3(transform.position.x , transform.position.y + nudgeAmount, transform.position.z);
                NudgeCoroutine = StartCoroutine(move(Direction, 0.5f));
                break;
        }
    }

    public IEnumerator move(Vector3 NewPos, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        // First movement (to NewPos)
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            transform.position = Vector3.Lerp(startPos, NewPos, progress);
            yield return null;
        }

        // Ensure final position is exact
        transform.position = NewPos;

        yield return new WaitForSeconds(0.2f);

        // Second movement (back to Origin)
        elapsedTime = 0f;
        startPos = transform.position; // Reset start position

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            transform.position = Vector3.Lerp(startPos, Origin, progress);
            yield return null;
        }

        // Ensure final position is exact
        transform.position = Origin;
        yield return new WaitForSeconds(0.2f);
        onNudgeDone?.Invoke();
        NudgeCoroutine = null;
    }

    public IEnumerator PlayAnim(List<Sprite> anim, bool loop,int frametoAction,Action actiontoUse)
    {
        animIndex = 0;
        float frameDuration = 1f / fps; // Calculate the duration of each frame
        currentAnim = new List<Sprite>(anim);

        sr.sprite = currentAnim[animIndex];
        bool ActionPerformed = false;

        while (!ispaused)
        {
            yield return new WaitForSeconds(frameDuration);
            animIndex++;

            if(animIndex == frametoAction && !ActionPerformed)
            {
                actiontoUse();
                ActionPerformed = true;
            }

            if (animIndex >= currentAnim.Count)
            {
                if (loop)
                {
                    animIndex = 0;
                    //checkForAddedAnim(); // Just check, don't exit
                }
                else
                {
                    //checkForAddedAnim(); // Just check, don't exit
                    yield break; // Only exit if no added anim
                }
            }
            sr.sprite = currentAnim[animIndex];
        }
    }


    public IEnumerator PlayAnim(List<Sprite> anim, bool loop)
    {
        animIndex = 0;
        float frameDuration = 1f / fps; // Calculate the duration of each frame
        currentAnim = new List<Sprite>(anim);

        sr.sprite = currentAnim[animIndex];

        while (!ispaused)
        {
            yield return new WaitForSeconds(frameDuration);
            animIndex++;

            if (animIndex >= currentAnim.Count)
            {
                if (loop)
                {
                    animIndex = 0;
                    //checkForAddedAnim(); // Just check, don't exit
                }
                else
                {
                    //checkForAddedAnim(); // Just check, don't exit
                    yield break; // Only exit if no added anim
                }
            }
            sr.sprite = currentAnim[animIndex];
        }
    }




    public void ClearAnim()
    {
        StopAllCoroutines();
        currentAnim.Clear();
    }

    private void OnEnable()
    {
        SetAnim(idleAnim, true);
    }

    private void OnDisable()
    {
        ClearAnim();
    }

}

public enum AnimDirection
{
    Left,Right,Up
}