using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleController_Player : BattleController
{
    [SerializeField] private BattleUIController UIController;
    public BattleUIController uicontroller => UIController;
    [SerializeField] private int HoveredAction;
    public int hoveredaction => HoveredAction;
    [SerializeField] private int HoveredCell;
    public int hoveredcell => HoveredCell;
    [SerializeField] private Vector2 CachedInput;

    [SerializeField] private float navigateDelay = 0.1f;
    [SerializeField] private float navigateTimer = 0.1f;

    public Action OnActionHover;
    public Action OnCellHover;
    public Action OnActionSelected;
    public Action<int> OnCellSelected;

    private new void Start()
    {
        base.Start();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        BattleInputManager.Instance.OnNavigateSelect += OnNavigateSelect;
        BattleInputManager.Instance.OnConfirm += OnConfirmPressed;
        BattleInputManager.Instance.OnPause += OnPausePressed;

        BattleManager.Instance.OnBattleStart += OnBattleStart;
        BattleManager.Instance.OnBattleEnd += OnBattleEnd;
        BattleManager.Instance.OnRollEnd += OnRollEnd;
        BattleManager.Instance.OnPhaseChange += OnPhaseChange;
    }

    private void UnsubscribeAllEvents()
    {
        if (BattleInputManager.Instance != null)
        {
            BattleInputManager.Instance.OnNavigateSelect -= OnNavigateSelect;
            BattleInputManager.Instance.OnConfirm -= OnConfirmPressed;
            BattleInputManager.Instance.OnPause -= OnPausePressed;
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStart -= OnBattleStart;
            BattleManager.Instance.OnBattleEnd -= OnBattleEnd;
            BattleManager.Instance.OnRollEnd -= OnRollEnd;
            BattleManager.Instance.OnPhaseChange -= OnPhaseChange;
        }

        UnsubscribeUIControllerEvents();
    }

    private void UnsubscribeUIControllerEvents()
    {
        if (UIController != null)
        {
            base.OnPlayerDamaged -= UIController.UpdateHealthUI;
            base.OnCellCharged -= OnCellChargedCallback;
            OnActionHover -= UIController.selectactionui.OnActionHover;
            OnCellHover -= UIController.selectactionui.OnCellHover;
            OnActionSelected -= UIController.selectactionui.OnActionSelected;
            OnCellSelected -= UIController.selectactionui.OnCellSelected;
            OnEffectAdded -= UIController.UpdateEffectsIcon;
            OnEffectDispel -= UIController.UpdateEffectsIcon;
        }
    }

    private void OnCellChargedCallback()
    {
        if (UIController != null)
        {
            UIController.UpdateCells(true);
        }
    }

    private void OnNavigateSelect(Vector2 input)
    {
        CachedInput = input;
    }

    private void OnBattleStart()
    {
        UIController = BattleUIManager.Instance.GetControllerbyID(playerindex);
        SubscribeUIControllerEvents();
        UIController.OnStartPhase();
    }

    private void SubscribeUIControllerEvents()
    {
        if (UIController != null)
        {
            base.OnPlayerDamaged += UIController.UpdateHealthUI;
            base.OnCellCharged += OnCellChargedCallback;
            OnActionHover += UIController.selectactionui.OnActionHover;
            OnCellHover += UIController.selectactionui.OnCellHover;
            OnActionSelected += UIController.selectactionui.OnActionSelected;
            OnCellSelected += UIController.selectactionui.OnCellSelected;
            OnEffectAdded += UIController.UpdateEffectsIcon;
            OnEffectDispel += UIController.UpdateEffectsIcon;
        }
    }

    private void OnBattleEnd()
    {
        UnsubscribeUIControllerEvents();
        UIController = null;
    }

    private void OnRollEnd()
    {
        if (UIController != null)
        {
            UIController.glovecontroller.OnRollEnd();
        }
    }

    private void OnPhaseChange(battleState state)
    {
        if (UIController == null) return;

        switch (state)
        {
            case battleState.Select:
                OnSelectActionActivate();
                UIController.OnSelectPhase();
                break;
            case battleState.Roll:
                UIController.OnRollPhase();
                UIController.glovecontroller.OnRollPhase();
                break;
            case battleState.Execution:
                UIController.OnExecutionPhase();
                break;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeAllEvents();
    }

    private void Update()
    {
        HandleNavigationInput();
        HandleDebugInputs();
    }

    private void HandleNavigationInput()
    {
        if (CachedInput != Vector2.zero)
        {
            if (navigateTimer >= 0)
            {
                navigateTimer -= Time.unscaledDeltaTime;
            }
            else
            {
                navigateTimer = navigateDelay;
                HandleNavigation(CachedInput);
                HandleCellNavigation(CachedInput);
            }
        }
        else
        {
            navigateTimer = 0;
        }
    }

    private void HandleDebugInputs()
    {
        // Debug input handling for testing effects
        if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyDebugEffect(Element.Solar);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyDebugEffect(Element.Sound);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyDebugEffect(Element.Wind);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ApplyDebugEffect(Element.Heat);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ApplyDebugEffect(Element.Electric);
        if (Input.GetKeyDown(KeyCode.Alpha6)) ApplyDebugEffect(Element.Hydro);
    }

    private void ApplyDebugEffect(Element element)
    {
        if (ChargeEffectsDatabase.Instance.alleffects.TryGetValue(element, out Dictionary<int, ChargeEffects> activationDict))
        {
            ChargeEffects effect = activationDict[1];
            ChargeEffects effectCloned = new ChargeEffects(effect.ActivationEffect, effect.PersistentEffect, effect.DispelEffect, 
                effect.timing, effect.ElementID, effect.EffectID, effect.turns);

            effectCloned.AssignContext(new ChargeEffectContext(
                glove.equippedmonster,
                glove.cellmonsters[1],
                BattleManager.Instance.GetEquippedMonster(1),
                this.glove,
                BattleManager.Instance.GetGlovebyIndex(1),
                this,
                BattleManager.Instance.GetControllerByIndex(1)
            ));

            BattleManager.Instance.GetControllerByIndex(1).AddEffect(effectCloned);
        }
    }

    private bool IsSlotActivated(int index)
    {
        index -= 1;
        return index < slotsactivated.Length && slotsactivated[index];
    }

    private void HandleOnAttackPressed()
    {
        if (base.selectaction != selectAction.Attack)
        {
            HoveredCell = 7;
            return;
        }

        for (int i=1;i < 6;i++)
        {
            if(IsSlotActivated(i))
            {
                HoveredCell = i;
                break;
            }
        }
    }

    private void HandleCellNavigation(Vector2 navigation)
    {
        if (navigation.x == 0) return;


        if (base.selectaction != selectAction.Attack)
        {
            HoveredCell = 7;
            return;
        }

        bool isPlayer1 = (base.playerindex == 1);
        float effectiveX = isPlayer1 ? navigation.x : -navigation.x; // Reverse X for Player 2

        switch (HoveredCell)
        {
            case 1:
                if (effectiveX > 0.2f)
                {
                    if (IsSlotActivated(2)) HoveredCell = 2;
                    else if (IsSlotActivated(3)) HoveredCell = 3;
                    else if (IsSlotActivated(4)) HoveredCell = 4;
                    else if (IsSlotActivated(5)) HoveredCell = 5;
                    else if (ConfirmCondition()) HoveredCell = 6;
                    else HoveredCell = 7;
                }
                break;
            case 2:
                if (effectiveX > 0.2f)
                {
                    if (IsSlotActivated(3)) HoveredCell = 3;
                    else if (IsSlotActivated(4)) HoveredCell = 4;
                    else if (IsSlotActivated(5)) HoveredCell = 5;
                    else if (ConfirmCondition()) HoveredCell = 6;
                    else HoveredCell = 7;
                }
                else if (effectiveX < -0.2f)
                {
                    if (IsSlotActivated(1)) HoveredCell = 1;
                }
                break;
            case 3:
                if (effectiveX > 0.2f)
                {
                    if (IsSlotActivated(4)) HoveredCell = 4;
                    else if (IsSlotActivated(5)) HoveredCell = 5;
                    else if (ConfirmCondition()) HoveredCell = 6;
                    else HoveredCell = 7;
                }
                else if (effectiveX < -0.05f)
                {
                    if (IsSlotActivated(2)) HoveredCell = 2;
                    else if (IsSlotActivated(1)) HoveredCell = 1;
                }
                break;
            case 4:
                if (effectiveX > 0.2f)
                {
                    if (IsSlotActivated(5)) HoveredCell = 5;
                    else if (ConfirmCondition()) HoveredCell = 6;
                    else HoveredCell = 7;
                }
                else if (effectiveX < -0.2f)
                {
                    if (IsSlotActivated(3)) HoveredCell = 3;
                    else if (IsSlotActivated(2)) HoveredCell = 2;
                    else if (IsSlotActivated(1)) HoveredCell = 1;
                }
                break;
            case 5:
                if (effectiveX > 0.2f)
                {
                    if (ConfirmCondition()) HoveredCell = 6;
                    else HoveredCell = 7;

                }
                else if (effectiveX < -0.2f)
                {
                    if (IsSlotActivated(4)) HoveredCell = 4;
                    else if (IsSlotActivated(3)) HoveredCell = 3;
                    else if (IsSlotActivated(2)) HoveredCell = 2;
                    else if (IsSlotActivated(1)) HoveredCell = 1;
                }
                break;
            case 6:
                if (effectiveX > 0.2f)
                {
                    HoveredCell = 7;
                }
                else if (effectiveX < -0.2f)
                {
                    if (IsSlotActivated(5)) HoveredCell = 5;
                    else if (IsSlotActivated(4)) HoveredCell = 4;
                    else if (IsSlotActivated(3)) HoveredCell = 3;
                    else if (IsSlotActivated(2)) HoveredCell = 2;
                    else if (IsSlotActivated(1)) HoveredCell = 1;
                }
                break;
            case 7:
                if (effectiveX < -0.2f)
                {
                    if (ConfirmCondition()) HoveredCell = 6;
                    else if (IsSlotActivated(5)) HoveredCell = 5;
                    else if (IsSlotActivated(4)) HoveredCell = 4;
                    else if (IsSlotActivated(3)) HoveredCell = 3;
                    else if (IsSlotActivated(2)) HoveredCell = 2;
                    else if (IsSlotActivated(1)) HoveredCell = 1;
                }
                break;
        }

        OnCellHover?.Invoke();
    }

    private void OnSelectActionActivate()
    {
        if (base.SelectActionCondition(selectAction.Attack))
        {
            HoveredAction = 1;
        }
        else if (base.SelectActionCondition(selectAction.Defend))
        {
            HoveredAction = 2;
        }
        else
        {
            HoveredAction = 3;
        }
    }


    private void HandleNavigation(Vector2 navigation)
    {
        if (navigation.y == 0) return;

        if (BattleManager.Instance.state == battleState.Select)
        {
            if (base.selectaction == selectAction.Attack) return;

            switch(HoveredAction)
            {
                default:
                    HoveredAction = 3;
                    break;
                case 1:
                    if(navigation.y < 0.2f)
                    {
                        if(base.SelectActionCondition(selectAction.Defend))
                        {
                            HoveredAction = 2;
                        }
                        else
                        {
                            HoveredAction = 3;
                        }
                    }
                    break;
                case 2:
                    if (navigation.y > 0.2f)
                    {
                        if (base.SelectActionCondition(selectAction.Attack))
                        {
                            HoveredAction = 1;
                        }
                    }
                    else if (navigation.y < 0.2f)
                    {
                        HoveredAction = 3;
                    }
                    break;
                case 3:
                    if (navigation.y > 0.2f)
                    {
                        if (base.SelectActionCondition(selectAction.Defend))
                        {
                            HoveredAction = 2;
                        }
                        else
                        {
                            if (base.SelectActionCondition(selectAction.Attack))
                            {
                                HoveredAction = 1;
                            }
                        }
                    }
                    break;
            }
        }
        OnActionHover?.Invoke();
    }

    private void OnConfirmPressed()
    {
        if(BattleManager.Instance.state == battleState.Roll && BattleManager.Instance.canroll) //Handle Roll Phase
        {
            PlayerConfirm();
            UIController.glovecontroller.OnRollConfirm();
            return;
        }

        if (base.selectaction != selectAction.Attack && BattleManager.Instance.state == battleState.Select) //Handle Select Phase
        {
            switch (HoveredAction)
            {
                case 1:
                    PlayerAttack();
                    break;
                case 2:
                    PlayerDefend();
                    break;
                case 3:
                    PlayerSkip();
                    break;
                case 4:
                    PlayerConfirm();
                    break;
            }
            OnActionSelected?.Invoke();
        }
        else if (BattleManager.Instance.state == battleState.Select)
        {
            switch (HoveredCell)
            {
                case 1:
                    base.HandleChargeCell(0);
                    break;
                case 2:
                    base.HandleChargeCell(1);
                    break;
                case 3:
                    base.HandleChargeCell(2);
                    break;
                case 4:
                    base.HandleChargeCell(3);
                    break;
                case 5:
                    base.HandleChargeCell(4);
                    break;
                case 6:
                    PlayerConfirm();
                    break;
                case 7:
                    PlayerNone();
                    break;
            }
            OnCellSelected?.Invoke(HoveredCell);
        }
    }

    private void OnPausePressed()
    {
        BattleManager.Instance.SkipStartingCutscene();
    }

    public void PlayerConfirm()
    {
        base.HandleConfirm();
    }
    public void PlayerNone()
    {
        base.HandleSelectAction(selectAction.None);
        OnSelectActionActivate();
    }
    public void PlayerSkip()
    {
        base.HandleSelectAction(selectAction.Skip);
        base.HandleConfirm();
    }

    public void PlayerAttack()
    {
        base.HandleSelectAction(selectAction.Attack);
        HandleOnAttackPressed();
    }

    public void PlayerDefend()
    {
        base.HandleSelectAction(selectAction.Defend);
        base.HandleConfirm();
    }

    public void PlayerCharge(int slot)
    {
        base.HandleChargeCell(slot);
    }
}
