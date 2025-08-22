using UnityEngine;

public class Player_Animator : MonoBehaviour
{
    [SerializeField] private Player player;
    private bool running => player.isrunning;

    [SerializeField] private Animator animator;

    private void Start()
    {
        player.OnStateChanged += (PlayerState State) => { 
        
        switch(State)
            {
                case PlayerState.Idle:
                    animator.SetBool("IsWalking",false);
                    break;
                case PlayerState.Walk:
                    animator.SetBool("IsWalking", true);
                    break;
            }
        };
    }
    private void FixedUpdate()
    {
        if(player.state!= PlayerState.None)
        {
            animator.SetBool("IsRunning", running);
        }
    }
}
