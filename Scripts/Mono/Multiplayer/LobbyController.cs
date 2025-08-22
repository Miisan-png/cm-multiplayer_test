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

    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }

    public void UpdateButton()
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

    public void CheckifAllReady()
    {
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

        if(allready)
        {
            if(LocalPlayerController.PlayerIdNumber == 1)
            {
                StartGameButton.interactable = true;
            }
            else
            {
                StartGameButton.interactable = false;
            }
        }
        else
        {
            StartGameButton.interactable = false;
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if(!PlayerItemCreated) { CreateHostPlayerItem();  }
        if(PlayerListItems.Count < Manager.GamePlayers.Count) { CreateCilentPlayerItem(); }
        if(PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }
        if (PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject newplayeritem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = newplayeritem.GetComponent<PlayerListItem>();
            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.Ready;
            NewPlayerItemScript.SetPlayerValues();

            newplayeritem.transform.SetParent(PlayerListViewContent.transform);
            newplayeritem.transform.localScale = Vector3.one;

            PlayerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateCilentPlayerItem()
    {
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

                newplayeritem.transform.SetParent(PlayerListViewContent.transform);
                newplayeritem.transform.localScale = Vector3.one;

                PlayerListItems.Add(NewPlayerItemScript);
            }
        }
    }

    public void UpdatePlayerItem()
    {
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
        LocalPlayerController.Quit((CSteamID)CurrentLobbyID);
    }


}
