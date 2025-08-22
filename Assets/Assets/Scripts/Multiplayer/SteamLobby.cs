using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.Generic;

public class SteamLobby : MonoBehaviour
{
    private static SteamLobby instance;
    public static SteamLobby Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;
    protected Callback<LobbyChatUpdate_t> LobbyChatUpdated;

    public List<CSteamID> lobbyIDs = new List<CSteamID>();


    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    [SerializeField] private CustomNetworkManager manager;


    private void Start()
    {
        if (!SteamManager.Initialized) return;

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
        LobbyChatUpdated = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;

        Debug.Log("Lobby created success!");

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),HostAddressKey,SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");

    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        //Everyone
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        //Cilent
        if (NetworkServer.active) return;

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        manager.StartClient();
    }

    public void JoinLobby(CSteamID LobbyID)
    {
        SteamMatchmaking.JoinLobby(LobbyID);
    }


    public void GetLobbiesList()
    {
        if (lobbyIDs.Count > 0) { lobbyIDs.Clear(); }

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();
    }
    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        Debug.Log("hello");
        // Check if someone left the lobby
        if ((callback.m_rgfChatMemberStateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0)
        {
            CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);

            // Get current number of lobby members
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

            // If lobby is empty, close it
            if (memberCount == 0 && SteamMatchmaking.GetLobbyOwner(lobbyId) == SteamUser.GetSteamID())
            {
                SteamMatchmaking.DeleteLobbyData(lobbyId, "name");
            }
        }
    }

    void OnGetLobbyList(LobbyMatchList_t result)
    {
        if (LobbiesListManager.Instance.ListofLobbies.Count > 0) { LobbiesListManager.Instance.DestroyLobbies(); }

        for(int i=0;i<result.m_nLobbiesMatching;i++)
        {
            CSteamID lobbyid = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyid);
            SteamMatchmaking.RequestLobbyData(lobbyid);
        }
    }

    void OnGetLobbyData(LobbyDataUpdate_t result)
    {
        LobbiesListManager.Instance.DisplayLobbies(lobbyIDs, result);
    }
}
