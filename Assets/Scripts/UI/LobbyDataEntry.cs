using UnityEngine;
using Steamworks;
using TMPro;

public class LobbyDataEntry : MonoBehaviour
{
    public CSteamID lobbyID;
    public string lobbyname;
    public TextMeshProUGUI lobbynametext;

    public void SetLobbyData()
    {
        if(lobbyname == "")
        {
            lobbynametext.text = "Empty";
        }
        else
        {
            lobbynametext.text = lobbyname;
        }

    }

    public void JoinLobby()
    {
        SteamLobby.Instance.JoinLobby(lobbyID);
    }


}
