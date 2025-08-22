using System;
using System.Collections;
using UnityEngine;

public class BattleController_NPC : BattleController
{
    [SerializeField] private BattleUIController UIController;
    private bool canroll;

    private new void Start()
    {
        base.Start();
        base.OnSlotsChanged += OnRollNumber;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        BattleManager.Instance.OnBattleStart += OnBattleStart;
        BattleManager.Instance.OnBattleEnd += OnBattleEnd;
        BattleManager.Instance.OnPhaseChange += OnPhaseChange;
    }

    private void UnsubscribeAllEvents()
    {
        base.OnSlotsChanged -= OnRollNumber;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStart -= OnBattleStart;
            BattleManager.Instance.OnBattleEnd -= OnBattleEnd;
            BattleManager.Instance.OnPhaseChange -= OnPhaseChange;
        }

        if (UIController != null)
        {
            base.OnPlayerDamaged -= UIController.UpdateHealthUI;
            base.OnCellCharged -= OnCellChargedCallback;
            OnEffectAdded -= UIController.UpdateEffectsIcon;
            OnEffectDispel -= UIController.UpdateEffectsIcon;
        }
    }

    private void OnCellChargedCallback()
    {
        if (UIController != null)
        {
            UIController.UpdateCells(false);
        }
    }

    private void OnBattleStart()
    {
        UIController = BattleUIManager.Instance.GetControllerbyID(playerindex);
        base.OnPlayerDamaged += UIController.UpdateHealthUI;
        base.OnCellCharged += OnCellChargedCallback;
        OnEffectAdded += UIController.UpdateEffectsIcon;
        OnEffectDispel += UIController.UpdateEffectsIcon;

        UIController.HideAllIdentity();
        UIController.OnStartPhase();
        canroll = false;
    }

    private void OnBattleEnd()
    {
        UnsubscribeControllerEvents();
        UIController = null;
    }

    private void UnsubscribeControllerEvents()
    {
        if (UIController != null)
        {
            base.OnPlayerDamaged -= UIController.UpdateHealthUI;
            base.OnCellCharged -= OnCellChargedCallback;
            OnEffectAdded -= UIController.UpdateEffectsIcon;
            OnEffectDispel -= UIController.UpdateEffectsIcon;
        }
    }

    private void OnPhaseChange(battleState state)
    {
        switch (state)
        {
            case battleState.Start:
                canroll = false;
                break;
            case battleState.Select:
                canroll = false;
                StartCoroutine(SelectDelay());
                UIController.OnSelectPhase();
                break;
            case battleState.Roll:
                StartCoroutine(RollDelay());
                UIController.OnRollPhase();
                break;
            case battleState.Execution:
                UIController.OnExecutionPhase();
                break;
        }
    }

    private IEnumerator RollDelay()
    {
        yield return new WaitForSeconds(3f);
        canroll = true;
    }

    private void OnRollNumber()
    {
        if (BattleManager.Instance.state != battleState.Roll || !canroll) return;

        int currentsuccesfulrolls = 0;
        int selfactivated = 0;
        int equippedmonsters = 0;

        for (int i = 0; i < 5; i++)
        {
            if (base.glove.cellmonsters[i] != null && base.glove.cellmonsters[i].id != 0)
            {
                equippedmonsters++;
                if (CompareHelper.ElementMatchesInt(base.numberslots[i].number, base.glove.cellmonsters[i].element) && !base.slotsactivated[i])
                {
                    currentsuccesfulrolls++;
                }
            }

            if (base.slotsactivated[i])
            {
                selfactivated++;
            }
        }

        if (currentsuccesfulrolls == equippedmonsters)
        {
            base.HandleConfirm();
            return;
        }

        int slotsneeded = CalculateSlotsNeeded(equippedmonsters, selfactivated);

        if (currentsuccesfulrolls >= slotsneeded || (equippedmonsters == 1 && (currentsuccesfulrolls == 1 || selfactivated == 1)))
        {
            base.HandleConfirm();
        }
    }

    private int CalculateSlotsNeeded(int equippedmonsters, int selfactivated)
    {
        if (equippedmonsters <= 0) return 0;
        if (equippedmonsters == 1) return 1;

        int maxPossible = Mathf.Max(1, equippedmonsters - 1);
        int slotsneeded = UnityEngine.Random.Range(1, maxPossible + 1);

        if (selfactivated >= 2) slotsneeded = Mathf.Max(1, slotsneeded - 1);
        if (BattleManager.Instance.rolltimer <= 5) slotsneeded = Mathf.Max(1, slotsneeded - 1);

        return Mathf.Min(slotsneeded, equippedmonsters);
    }

    private void MakeSelectAction()
    {
        if (BattleManager.Instance.state != battleState.Select) return;

        int target = base.playerindex == 1 ? 2 : 1;
        BattleController enemy = BattleManager.Instance.GetControllerByIndex(target);
        (int selfactivated, int selfmonsters) = CountActivatedAndMonsters(this);
        (int enemyactivated, int enemymonsters) = CountActivatedAndMonsters(enemy);

        if (TryAttackAction(selfactivated, selfmonsters, enemy)) return;
        if (TryDefendAction(enemyactivated, enemymonsters, enemy)) return;
        if (TryFinishAction(selfactivated, enemy)) return;

        DefaultAction();
    }

    private (int activated, int monsters) CountActivatedAndMonsters(BattleController controller)
    {
        int activated = 0;
        int monsters = 0;

        for (int i = 0; i < 5; i++)
        {
            if (controller.slotsactivated[i]) activated++;
            if (controller.glove.cellmonsters[i] != null && controller.glove.cellmonsters[i].id != 0) monsters++;
        }

        return (activated, monsters);
    }

    private bool TryAttackAction(int selfactivated, int selfmonsters, BattleController enemy)
    {
        float attackChance = Mathf.Clamp((float)selfactivated / selfmonsters - 0.1f, 0f, 1f);
        if (FlipCoin(attackChance) && base.SelectActionCondition(selectAction.Attack))
        {
            base.HandleSelectAction(selectAction.Attack);
            ChargeAllCells();
            base.HandleConfirm();
            return true;
        }
        return false;
    }

    private bool TryDefendAction(int enemyactivated, int enemymonsters, BattleController enemy)
    {
        if (enemyactivated >= 1)
        {
            float defenseChance = (base.hp > enemy.hp && base.hp > 5) ? 0.2f :
                Mathf.Clamp((float)enemyactivated / enemymonsters - 0.1f, 0f, 1f);

            if (FlipCoin(defenseChance) && base.SelectActionCondition(selectAction.Defend))
            {
                base.HandleSelectAction(selectAction.Defend);
                base.HandleConfirm();
                return true;
            }
        }
        return false;
    }

    private bool TryFinishAction(int selfactivated, BattleController enemy)
    {
        if (enemy.hp <= 5 && selfactivated >= 1 && base.SelectActionCondition(selectAction.Attack))
        {
            base.HandleSelectAction(selectAction.Attack);
            ChargeAllCells();
            base.HandleConfirm();
            return true;
        }
        return false;
    }

    private void DefaultAction()
    {
        base.HandleSelectAction(selectAction.Skip);
        base.HandleConfirm();
    }

    private void ChargeAllCells()
    {
        for (int i = 0; i < 5; i++)
        {
            base.HandleChargeCell(i);
        }
    }

    private bool FlipCoin(float successRate) => UnityEngine.Random.value < successRate;

    private IEnumerator SelectDelay()
    {
        yield return new WaitForSeconds(2f);
        MakeSelectAction();
    }

    private void OnDestroy()
    {
        UnsubscribeAllEvents();
    }
}