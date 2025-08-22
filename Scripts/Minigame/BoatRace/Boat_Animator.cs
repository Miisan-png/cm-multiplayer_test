using System.Collections.Generic;
using UnityEngine;

public class Boat_Animator : MonoBehaviour
{
    [SerializeField] private BoatController player;

    private void Start()
    {
        player.onDrift += Ondrift;
        player.onDriftEnd += Offdrift;
        player.onThrottleStart += Onthrottle;
        player.onThrottleEnd += Offthrottle;
        player.onStunStart += OnStun;
        player.onStunEnd += OffStun;
    }
    
    [Header("Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private string IdleAnimation;
    [SerializeField] private string LDriftAnimation;
    [SerializeField] private string RDriftAnimation;
    [SerializeField] private string StunAnimation;

    [Header("VFX")]
    [SerializeField] private ParticleSystem smallSplash;
    [SerializeField] private ParticleSystem bigSplash;
    [SerializeField] private List<ParticleSystem> waterSurface;


    private bool PlayCondition()
    {
        return Minigame_BoatRace.Instance.state == MinigameState.Active;
    }

    private void Onthrottle()
    {
        if (!PlayCondition()) return;
        smallSplash.Play();
        for(int i =0;i<waterSurface.Count;i++)
        {
            waterSurface[i].Play();
        }
    }

    private void Offthrottle()
    {
        smallSplash.Stop();
        for (int i = 0; i < waterSurface.Count; i++)
        {
            waterSurface[i].Stop();
        }
    }

    private void Ondrift(TurnDirection direction)
    {
        if (!PlayCondition()) return;
        bigSplash.Play();

        if(direction == TurnDirection.Right)
        {
            animator.Play(RDriftAnimation);
        }
        else
        {
            animator.Play(LDriftAnimation);
        }
    }

    private void Offdrift()
    {
        bigSplash.Stop();
        animator.Play(IdleAnimation);
    }

    private void OnStun()
    {
        if (!PlayCondition()) return;
        bigSplash.Stop();
        smallSplash.Stop();
        animator.Play(StunAnimation);
    }

    private void OffStun()
    {
        animator.Play(IdleAnimation);
    }

}
