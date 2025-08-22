using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;

public class SimpleHostJoinUI : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button HostButton;
    public Button JoinButton;
    
    [Header("Status")]
    public TMPro.TextMeshProUGUI StatusText; // Optional status display
    
    private void Start()
    {
        // Set up button events
        if (HostButton != null)
            HostButton.onClick.AddListener(HostLobby);
            
        if (JoinButton != null)
            JoinButton.onClick.AddListener(JoinLobby);
            
        UpdateStatus("Choose Host or Join");
    }
    
    public void HostLobby()
    {
        Debug.Log("Hosting Steam lobby...");
        UpdateStatus("Creating lobby...");
        
        // Use your existing Steam lobby system to host
        if (SteamLobby.Instance != null)
        {
            SteamLobby.Instance.HostLobby();
            UpdateStatus("Lobby created! Waiting for players...");
        }
        else
        {
            Debug.LogError("SteamLobby.Instance is null!");
            UpdateStatus("Error: Steam lobby system not found!");
        }
    }
    
    public void JoinLobby()
    {
        Debug.Log("Attempting to join available lobby...");
        UpdateStatus("Searching for lobbies...");
        
        // Get list of available lobbies and join the first one
        if (SteamLobby.Instance != null)
        {
            // First get the list of lobbies
            SteamLobby.Instance.GetLobbiesList();
            
            // Wait a moment then try to join
            Invoke("TryJoinFirstLobby", 2f);
        }
        else
        {
            Debug.LogError("SteamLobby.Instance is null!");
            UpdateStatus("Error: Steam lobby system not found!");
        }
    }
    
    private void TryJoinFirstLobby()
    {
        if (LobbiesListManager.Instance != null && LobbiesListManager.Instance.ListofLobbies.Count > 0)
        {
            // Get the first available lobby
            var firstLobby = LobbiesListManager.Instance.ListofLobbies[0];
            var lobbyEntry = firstLobby.GetComponent<LobbyDataEntry>();
            
            if (lobbyEntry != null)
            {
                Debug.Log($"Joining lobby: {lobbyEntry.lobbyname}");
                UpdateStatus($"Joining lobby: {lobbyEntry.lobbyname}");
                
                // Join the lobby
                lobbyEntry.JoinLobby();
                
                UpdateStatus("Joined lobby! Click Ready when ready to battle.");
            }
        }
        else
        {
            UpdateStatus("No lobbies found! Make sure host is running.");
            Debug.LogWarning("No lobbies available to join!");
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (StatusText != null)
        {
            StatusText.text = message;
        }
        Debug.Log($"Status: {message}");
    }
}