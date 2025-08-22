using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkedBattleManager : NetworkBehaviour
{
    private static NetworkedBattleManager instance;
    public static NetworkedBattleManager Instance => instance;

    public override void OnStartServer()
    {
        instance = this;
    }

    [Header("Networked Battle Setup")]
    [SerializeField] private BattleGlove P1NetworkGlove;
    [SerializeField] private BattleGlove P2NetworkGlove;
    
    // Network sync variables for battle state
    [SyncVar(hook = nameof(OnBattleStateChanged))] 
    private battleState networkBattleState = battleState.None;
    
    [SyncVar(hook = nameof(OnPlayerToMoveFirstChanged))]
    private int networkPlayerToMoveFirst = 0;
    
    [SyncVar] private float networkRollTimer;
    [SyncVar] private float networkSelectTimer;
    
    // Track ready states
    [SyncVar] private bool player1Ready = false;
    [SyncVar] private bool player2Ready = false;
    [SyncVar] private bool player1RollConfirmed = false;
    [SyncVar] private bool player2RollConfirmed = false;
    [SyncVar] private bool player1SelectConfirmed = false;
    [SyncVar] private bool player2SelectConfirmed = false;

    // Network events
    public System.Action<battleState> OnNetworkBattleStateChanged;
    public System.Action<int> OnNetworkPlayerToMoveFirstChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Called by CustomNetworkManager when both players are connected
    [Server]
    public void StartNetworkedBattle(BattleGlove p1Glove, BattleGlove p2Glove)
    {
        P1NetworkGlove = p1Glove;
        P2NetworkGlove = p2Glove;
        
        // Initialize battle on server
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.StartBattle(P1NetworkGlove, P2NetworkGlove, battleType.NPC);
        }
        
        // Sync initial state
        networkBattleState = battleState.Start;
        RpcInitializeBattleOnClients();
    }

    [ClientRpc]
    private void RpcInitializeBattleOnClients()
    {
        if (!isServer && BattleManager.Instance != null)
        {
            // Clients initialize their local battle manager
            BattleManager.Instance.StartBattle(P1NetworkGlove, P2NetworkGlove, battleType.NPC);
        }
    }

    // Sync battle state changes
    [Server]
    public void ServerSetBattleState(battleState newState)
    {
        networkBattleState = newState;
    }

    private void OnBattleStateChanged(battleState oldState, battleState newState)
    {
        OnNetworkBattleStateChanged?.Invoke(newState);
    }

    [Server]
    public void ServerSetPlayerToMoveFirst(int playerIndex)
    {
        networkPlayerToMoveFirst = playerIndex;
    }

    private void OnPlayerToMoveFirstChanged(int oldPlayer, int newPlayer)
    {
        OnNetworkPlayerToMoveFirstChanged?.Invoke(newPlayer);
    }

    // Roll phase networking
    [Command(requiresAuthority = false)]
    public void CmdPlayerConfirmRoll(int playerIndex, NetworkConnectionToClient sender = null)
    {
        if (playerIndex == 1)
            player1RollConfirmed = true;
        else if (playerIndex == 2)
            player2RollConfirmed = true;

        // Set first player to move
        if (networkPlayerToMoveFirst == 0)
        {
            networkPlayerToMoveFirst = playerIndex;
        }

        // Check if both confirmed or timer expired
        if (player1RollConfirmed && player2RollConfirmed)
        {
            RpcBothPlayersRollConfirmed();
        }
    }

    [ClientRpc]
    private void RpcBothPlayersRollConfirmed()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SkipNextState(); // Move to Select phase
        }
    }

    // Select phase networking
    [Command(requiresAuthority = false)]
    public void CmdPlayerConfirmSelect(int playerIndex, selectAction selectedAction, NetworkConnectionToClient sender = null)
    {
        if (playerIndex == 1)
            player1SelectConfirmed = true;
        else if (playerIndex == 2)
            player2SelectConfirmed = true;

        // Sync the action selection
        RpcPlayerSelectedAction(playerIndex, selectedAction);

        // Check if both confirmed
        if (player1SelectConfirmed && player2SelectConfirmed)
        {
            RpcBothPlayersSelectConfirmed();
        }
    }

    [ClientRpc]
    private void RpcPlayerSelectedAction(int playerIndex, selectAction action)
    {
        // Update local battle controllers with the action
        if (BattleManager.Instance != null)
        {
            var controller = BattleManager.Instance.GetControllerByIndex(playerIndex);
            if (controller != null)
            {
                // Force set the select action (you may need to add a public setter)
                // controller.SetSelectAction(action);
            }
        }
    }

    [ClientRpc]
    private void RpcBothPlayersSelectConfirmed()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SkipNextState(); // Move to Execution phase
        }
    }

    // Damage and battle end networking
    [Command(requiresAuthority = false)]
    public void CmdPlayerTakeDamage(int playerIndex, int damage, NetworkConnectionToClient sender = null)
    {
        RpcPlayerTakeDamage(playerIndex, damage);
        
        // Check if battle should end
        var controller = BattleManager.Instance?.GetControllerByIndex(playerIndex);
        if (controller != null && controller.hp <= 0)
        {
            RpcBattleEnd(playerIndex == 1 ? 2 : 1); // Other player wins
        }
    }

    [ClientRpc]
    private void RpcPlayerTakeDamage(int playerIndex, int damage)
    {
        if (BattleManager.Instance != null)
        {
            var controller = BattleManager.Instance.GetControllerByIndex(playerIndex);
            controller?.TakeDamage(damage);
        }
    }

    [ClientRpc]
    private void RpcBattleEnd(int winnerIndex)
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnMonsterDeath(winnerIndex == 1 ? 2 : 1);
        }
    }

    // Reset for new battle
    [Server]
    public void ResetNetworkBattle()
    {
        player1Ready = false;
        player2Ready = false;
        player1RollConfirmed = false;
        player2RollConfirmed = false;
        player1SelectConfirmed = false;
        player2SelectConfirmed = false;
        networkPlayerToMoveFirst = 0;
        networkBattleState = battleState.None;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}