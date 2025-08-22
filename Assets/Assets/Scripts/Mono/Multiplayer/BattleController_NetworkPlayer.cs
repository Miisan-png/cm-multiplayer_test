using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleController_NetworkPlayer : BattleController
{
    [SerializeField] private BattleUIController UIController;
    public BattleUIController uicontroller => UIController;
    
    [SerializeField] private int NetworkPlayerID; // 1 or 2
    public int networkPlayerID => NetworkPlayerID;
    
    [SerializeField] private bool IsLocalPlayer = false;
    public bool isLocalPlayer => IsLocalPlayer;
    
    // Input handling (only for local player)
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

    public void SetNetworkPlayerID(int playerID, bool isLocal)
    {
        NetworkPlayerID = playerID;
        IsLocalPlayer = isLocal;
        // Note: playerindex is read-only, we'll use NetworkPlayerID instead
    }

    private new void Start()
    {
        base.Start();
        
        // Only subscribe to input events if this is the local player
        if (IsLocalPlayer)
        {
            SubscribeToInputEvents();
        }
        
        SubscribeToBattleEvents();
    }

    private void SubscribeToInputEvents()
    {
        if (BattleInputManager.Instance != null)
        {
            BattleInputManager.Instance.OnNavigateSelect += OnNavigateSelect;
            BattleInputManager.Instance.OnConfirm += OnConfirmPressed;
            BattleInputManager.Instance.OnPause += OnPausePressed;
        }
    }

    private void SubscribeToBattleEvents()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStart += OnBattleStart;
            BattleManager.Instance.OnBattleEnd += OnBattleEnd;
            BattleManager.Instance.OnRollEnd += OnRollEnd;
            BattleManager.Instance.OnPhaseChange += OnPhaseChange;
        }
    }

    private void UnsubscribeAllEvents()
    {
        if (IsLocalPlayer && BattleInputManager.Instance != null)
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
            
            if (IsLocalPlayer)
            {
                OnActionHover -= UIController.selectactionui.OnActionHover;
                OnCellHover -= UIController.selectactionui.OnCellHover;
                OnActionSelected -= UIController.selectactionui.OnActionSelected;
                OnCellSelected -= UIController.selectactionui.OnCellSelected;
            }
            
            OnEffectAdded -= UIController.UpdateEffectsIcon;
            OnEffectDispel -= UIController.UpdateEffectsIcon;
        }
    }

    private void OnCellChargedCallback()
    {
        if (UIController != null)
        {
            UIController.UpdateCells(IsLocalPlayer);
        }
    }

    private void OnNavigateSelect(Vector2 input)
    {
        if (!IsLocalPlayer) return;
        CachedInput = input;
    }

    private void OnBattleStart()
    {
        UIController = BattleUIManager.Instance.GetControllerbyID(NetworkPlayerID);
        SubscribeUIControllerEvents();
        UIController.OnStartPhase();
    }

    private void SubscribeUIControllerEvents()
    {
        if (UIController != null)
        {
            base.OnPlayerDamaged += UIController.UpdateHealthUI;
            base.OnCellCharged += OnCellChargedCallback;
            
            if (IsLocalPlayer)
            {
                OnActionHover += UIController.selectactionui.OnActionHover;
                OnCellHover += UIController.selectactionui.OnCellHover;
                OnActionSelected += UIController.selectactionui.OnActionSelected;
                OnCellSelected += UIController.selectactionui.OnCellSelected;
            }
            
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
        if (UIController != null && IsLocalPlayer)
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
                if (IsLocalPlayer)
                {
                    OnSelectActionActivate();
                }
                UIController.OnSelectPhase();
                break;
            case battleState.Roll:
                UIController.OnRollPhase();
                if (IsLocalPlayer)
                {
                    UIController.glovecontroller.OnRollPhase();
                }
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
        if (!IsLocalPlayer) return;
        
        HandleNavigationInput();
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

    // Copy navigation logic from BattleController_Player
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

    private void HandleCellNavigation(Vector2 navigation)
    {
        // Copy cell navigation logic from BattleController_Player
        // (Simplified for space - implement full navigation as needed)
        if (navigation.x == 0) return;

        if (base.selectaction != selectAction.Attack)
        {
            HoveredCell = 7;
            return;
        }

        // Add full cell navigation logic here...
        OnCellHover?.Invoke();
    }

    private void OnConfirmPressed()
    {
        if (!IsLocalPlayer) return;

        if(BattleManager.Instance.state == battleState.Roll && BattleManager.Instance.canroll)
        {
            NetworkPlayerConfirm();
            if (UIController != null)
            {
                UIController.glovecontroller.OnRollConfirm();
            }
            return;
        }

        if (base.selectaction != selectAction.Attack && BattleManager.Instance.state == battleState.Select)
        {
            switch (HoveredAction)
            {
                case 1:
                    NetworkPlayerAttack();
                    break;
                case 2:
                    NetworkPlayerDefend();
                    break;
                case 3:
                    NetworkPlayerSkip();
                    break;
                case 4:
                    NetworkPlayerConfirm();
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
                    NetworkPlayerConfirm();
                    break;
                case 7:
                    NetworkPlayerNone();
                    break;
            }
            OnCellSelected?.Invoke(HoveredCell);
        }
    }

    private void OnPausePressed()
    {
        if (!IsLocalPlayer) return;
        BattleManager.Instance.SkipStartingCutscene();
    }

    // Network-aware action methods
    public void NetworkPlayerConfirm()
    {
        base.HandleConfirm();
        
        // Send to network
        if (BattleManager.Instance.state == battleState.Roll)
        {
            NetworkedBattleManager.Instance?.CmdPlayerConfirmRoll(NetworkPlayerID);
        }
        else if (BattleManager.Instance.state == battleState.Select)
        {
            NetworkedBattleManager.Instance?.CmdPlayerConfirmSelect(NetworkPlayerID, base.selectaction);
        }
    }

    public void NetworkPlayerNone()
    {
        base.HandleSelectAction(selectAction.None);
        OnSelectActionActivate();
    }

    public void NetworkPlayerSkip()
    {
        base.HandleSelectAction(selectAction.Skip);
        NetworkPlayerConfirm();
    }

    public void NetworkPlayerAttack()
    {
        base.HandleSelectAction(selectAction.Attack);
        HandleOnAttackPressed();
    }

    public void NetworkPlayerDefend()
    {
        base.HandleSelectAction(selectAction.Defend);
        NetworkPlayerConfirm();
    }

    private void HandleOnAttackPressed()
    {
        if (base.selectaction != selectAction.Attack)
        {
            HoveredCell = 7;
            return;
        }

        for (int i = 1; i < 6; i++)
        {
            if (IsSlotActivated(i))
            {
                HoveredCell = i;
                break;
            }
        }
    }

    private bool IsSlotActivated(int index)
    {
        index -= 1;
        return index < base.slotsactivated.Length && base.slotsactivated[index];
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

    public void PlayerCharge(int slot)
    {
        base.HandleChargeCell(slot);
    }
}