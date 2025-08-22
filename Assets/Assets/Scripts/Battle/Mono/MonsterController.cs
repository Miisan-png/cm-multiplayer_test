using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField] private int OwnerIndex;
    public int ownerIndex => OwnerIndex;

    [SerializeField] private Animator animator;

    private void OnMonsterAttack()
    {
        animator.SetTrigger(1);
    }
}
