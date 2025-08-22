using UnityEngine;
using Mirror;
using Steamworks;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;

public class LobbyController : MonoBehaviour
{
    private static LobbyController instance;
    public static LobbyController Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public TextMeshProUGUI LobbyNameText;

    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;

    public Button StartGameButton;
    public TextMeshProUGUI ReadyButtonText;

    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Start()
    {
        // Initialize UI elements
        if (StartGameButton != null)
        {
            StartGameButton.interactable = false;
        }
        
        // Try to find local player if not assigned
        if (LocalPlayerController == null)
        {
            FindLocalPlayer();
        }
    }

    public void ReadyPlayer()
    {
        if (LocalPlayerController != null)
        {
            LocalPlayerController.ChangeReady();
        }
        else
        {
            Debug.LogWarning("LocalPlayerController is null! Trying to find it...");
            FindLocalPlayer();
            if (LocalPlayerController != null)
            {
                LocalPlayerController.ChangeReady();
            }
        }
    }

    public void UpdateButton()
    {
        if (LocalPlayerController != null && ReadyButtonText != null)
        {
            if(LocalPlayerController.Ready)
            {
                ReadyButtonText.text = "UNREADY";
            }
            else
            {
                ReadyButtonText.text = "READY";
            }
        }
    }

    public void CheckifAllReady()
    {
        if (Manager == null || Manager.GamePlayers == null) return;

        bool allready = false;

        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            if(player.Ready)
            {
                allready = true;
            }
            else
            {
                allready = false;
                break;
            }
        }

        if(allready && Manager.GamePlayers.Count >= 2) // Need at least 2 players
        {
            if(LocalPlayerController != null && LocalPlayerController.PlayerIdNumber == 1)
            {
                if (StartGameButton != null)
                {
                    StartGameButton.interactable = true;
                }
            }
            else
            {
                if (StartGameButton != null)
                {
                    StartGameButton.interactable = false;
                }
            }
        }
        else
        {
            if (StartGameButton != null)
            {
                StartGameButton.interactable = false;
            }
        }
    }

    // NEW: Start the networked battle
    public void StartNetworkedBattle()
    {
        if (LocalPlayerController != null && LocalPlayerController.PlayerIdNumber == 1)
        {
            // Only host can start the game
            if (Manager != null)
            {
                Manager.StartNetworkBattle();
            }
        }
    }

    public void UpdateLobbyName()
    {
        if (Manager != null)
        {
            var steamLobby = Manager.GetComponent<SteamLobby>();
            if (steamLobby != null)
            {
                CurrentLobbyID = steamLobby.CurrentLobbyID;
                if (LobbyNameText != null)
                {
                    LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
                }
            }
        }
    }

    public void UpdatePlayerList()
    {
        if (Manager == null) return;

        if(!PlayerItemCreated) { CreateHostPlayerItem();  }
        if(PlayerListItems.Count < Manager.GamePlayers.Count) { CreateCilentPlayerItem(); }
        if(PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }
        if (PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        // Try multiple ways to find the local player
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        
        if (LocalPlayerObject == null)
        {
            // Look for any PlayerObjectController with network authority
            PlayerObjectController[] allPlayers = FindObjectsOfType<PlayerObjectController>();
            foreach (var player in allPlayers)
            {
                if (player.isOwned) // Use isOwned instead of hasAuthority
                {
                    LocalPlayerObject = player.gameObject;
                    break;
                }
            }
        }

        if (LocalPlayerObject != null)
        {
            LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
        }
        else
        {
            Debug.LogWarning("Could not find LocalGamePlayer object!");
        }
    }

    public void CreateHostPlayerItem()
    {
        if (Manager == null || Manager.GamePlayers == null) return;

        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject newplayeritem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = newplayeritem.GetComponent<PlayerListItem>();
            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.Ready;
            NewPlayerItemScript.SetPlayerValues();

            if (PlayerListViewContent != null)
            {
                newplayeritem.transform.SetParent(PlayerListViewContent.transform);
                newplayeritem.transform.localScale = Vector3.one;
            }

            PlayerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateCilentPlayerItem()
    {
        if (Manager == null || Manager.GamePlayers == null) return;

        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID))
            {
                GameObject newplayeritem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem NewPlayerItemScript = newplayeritem.GetComponent<PlayerListItem>();
                NewPlayerItemScript.PlayerName = player.PlayerName;
                NewPlayerItemScript.ConnectionID = player.ConnectionID;
                NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
                NewPlayerItemScript.Ready = player.Ready;
                NewPlayerItemScript.SetPlayerValues();

                if (PlayerListViewContent != null)
                {
                    newplayeritem.transform.SetParent(PlayerListViewContent.transform);
                    newplayeritem.transform.localScale = Vector3.one;
                }

                PlayerListItems.Add(NewPlayerItemScript);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        if (Manager == null || Manager.GamePlayers == null) return;

        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach(PlayerListItem playerListItemscript in PlayerListItems)
            {
                if(playerListItemscript.ConnectionID == player.ConnectionID)
                {
                    playerListItemscript.PlayerName = player.PlayerName;
                    playerListItemscript.Ready = player.Ready;
                    playerListItemscript.SetPlayerValues();

                    if(player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckifAllReady();
    }

    public void RemovePlayerItem()
    {
        if (Manager == null || Manager.GamePlayers == null) return;

        List<PlayerListItem> playerlisitemtoremove = new List<PlayerListItem>();

        foreach(PlayerListItem playerlistitem in PlayerListItems)
        {
            if(!Manager.GamePlayers.Any(b => b.ConnectionID == playerlistitem.ConnectionID))
            {
                playerlisitemtoremove.Add(playerlistitem);
            }
        }
        if(playerlisitemtoremove.Count > 0)
        {
            foreach (PlayerListItem playerlisitemtoRemove in playerlisitemtoremove)
            {
                GameObject objecttoremove = playerlisitemtoRemove.gameObject;
                PlayerListItems.Remove(playerlisitemtoRemove);
                Destroy(objecttoremove);

                objecttoremove = null;
            }
        }
    }

    public void LeaveLobby()
    {
        if (LocalPlayerController != null)
        {
            LocalPlayerController.Quit((CSteamID)CurrentLobbyID);
        }
    }
}