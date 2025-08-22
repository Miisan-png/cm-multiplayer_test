using UnityEngine;
using System.Collections.Generic;

public class BattleMonsterController : MonoBehaviour
{

    [SerializeField] private List<ParticleSystem> AllVFX;
    [SerializeField] private int MonsterIndex;

    public void ActivateVFX()
    {
        if (AllVFX == null || AllVFX.Count < 1) return;
        foreach (ParticleSystem vfx in AllVFX)
        {
            vfx.Stop();
        }
        foreach (ParticleSystem vfx in AllVFX)
        {
            vfx.Play();
        }
    }

    public void AnnounceAttackName(int AttackIndex)
    {
        BattleUIManager.Instance.AnnounceAttackName(MonsterIndex, AttackIndex);
    }

    public void DeactivateAttackName()
    {
        BattleUIManager.Instance.DeactivateAttackName();
    }
}
