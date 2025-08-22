using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Steamworks;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if(SceneManager.GetActiveScene().name == "Battle_Lobby")
        {
            PlayerObjectController gameplayerinstance = Instantiate(GamePlayerPrefab);
            gameplayerinstance.ConnectionID = conn.connectionId;
            gameplayerinstance.PlayerIdNumber = GamePlayers.Count + 1;
            gameplayerinstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, gameplayerinstance.gameObject);
        }

    }

}
