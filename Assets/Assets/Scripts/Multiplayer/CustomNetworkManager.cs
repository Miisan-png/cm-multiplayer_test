using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Steamworks;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    [Header("Game Settings")]
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    [SerializeField] private string BattleSceneName = "Battle_Base";
    
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();
    
    [Header("Battle Setup")]
    [SerializeField] private GameObject NetworkedBattleManagerPrefab;
    private NetworkedBattleManager battleManagerInstance;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if(SceneManager.GetActiveScene().name == "Battle_Lobby")
        {
            // Limit to 2 players for battle
            if (GamePlayers.Count >= 2)
            {
                Debug.Log("Lobby is full! Rejecting connection.");
                conn.Disconnect();
                return;
            }

            PlayerObjectController gameplayerinstance = Instantiate(GamePlayerPrefab);
            gameplayerinstance.ConnectionID = conn.connectionId;
            gameplayerinstance.PlayerIdNumber = GamePlayers.Count + 1;
            gameplayerinstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, gameplayerinstance.gameObject);
        }
        else if (SceneManager.GetActiveScene().name == BattleSceneName)
        {
            // Handle players joining during battle (if needed)
            Debug.Log("Player trying to join during battle - this might need special handling");
        }
    }

    // Called by LobbyController when host clicks Start Game
    [Server]
    public void StartNetworkBattle()
    {
        if (GamePlayers.Count != 2)
        {
            Debug.LogWarning("Cannot start battle - need exactly 2 players!");
            return;
        }

        // Check if both players are ready
        bool allReady = true;
        foreach (var player in GamePlayers)
        {
            if (!player.Ready)
            {
                allReady = false;
                break;
            }
        }

        if (!allReady)
        {
            Debug.LogWarning("Cannot start battle - not all players are ready!");
            return;
        }

        Debug.Log("Starting networked battle!");
        
        // Load battle scene for all clients
        ServerChangeScene(BattleSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == BattleSceneName)
        {
            // Battle scene loaded, set up the networked battle
            StartCoroutine(SetupNetworkedBattle());
        }
    }

    private IEnumerator SetupNetworkedBattle()
    {
        // Wait a frame for scene to fully load
        yield return null;

        // Spawn the NetworkedBattleManager
        if (NetworkedBattleManagerPrefab != null)
        {
            GameObject battleManagerObj = Instantiate(NetworkedBattleManagerPrefab);
            NetworkServer.Spawn(battleManagerObj);
            battleManagerInstance = battleManagerObj.GetComponent<NetworkedBattleManager>();
        }

        // Wait for all players to be ready
        yield return new WaitForSeconds(1f);

        // Create battle gloves for each player (you'll need to implement this based on your save system)
        BattleGlove player1Glove = CreatePlayerGlove(1);
        BattleGlove player2Glove = CreatePlayerGlove(2);

        // Start the networked battle
        if (battleManagerInstance != null)
        {
            battleManagerInstance.StartNetworkedBattle(player1Glove, player2Glove);
        }
    }

    // Create a basic battle glove for testing - you should load from player data instead
    private BattleGlove CreatePlayerGlove(int playerNumber)
    {
        BattleGlove glove = new BattleGlove();
        
        // Create a test monster for each player using the correct Monster constructor
        Monster testMonster = new Monster(
            playerNumber, // ID
            $"TestMonster{playerNumber}", // name
            playerNumber == 1 ? Element.Heat : Element.Hydro, // element
            MonsterRarity.common, // rarity
            10, // hp
            1 // skillIndex
        );
        testMonster.AssignLevel(1);
        
        glove.SetEquippedMonster(testMonster);
        
        // Add some cell monsters for testing
        for (int i = 1; i < 5; i++)
        {
            if (i <= 2) // Only add 2 cell monsters for simplicity
            {
                Monster cellMonster = new Monster(
                    (playerNumber * 10) + i, // ID
                    $"CellMonster{playerNumber}_{i}", // name
                    (Element)(i + 1), // element - Different elements
                    MonsterRarity.common, // rarity
                    8, // hp
                    1 // skillIndex
                );
                cellMonster.AssignLevel(1);
                glove.SetCellMonster(i, cellMonster);
            }
        }
        
        return glove;
    }

    public override void OnStopServer()
    {
        GamePlayers.Clear();
        battleManagerInstance = null;
        base.OnStopServer();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // Handle player disconnection
        PlayerObjectController disconnectedPlayer = null;
        foreach (var player in GamePlayers)
        {
            if (player.ConnectionID == conn.connectionId)
            {
                disconnectedPlayer = player;
                break;
            }
        }

        if (disconnectedPlayer != null)
        {
            GamePlayers.Remove(disconnectedPlayer);
            
            // If we're in battle and someone disconnects, handle it
            if (SceneManager.GetActiveScene().name == BattleSceneName && battleManagerInstance != null)
            {
                // End battle due to disconnection
                battleManagerInstance.ResetNetworkBattle();
                // Optionally return to lobby
                ServerChangeScene("Battle_Lobby");
            }
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client connected to server");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Client disconnected from server");
    }
}