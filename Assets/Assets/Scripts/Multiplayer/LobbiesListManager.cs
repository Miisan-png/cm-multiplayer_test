using UnityEngine;
using Steamworks;
using System.Collections.Generic;

public class LobbiesListManager : MonoBehaviour
{
    private static LobbiesListManager instance;
    public static LobbiesListManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private GameObject lobbydataitemprefab;
    [SerializeField] private GameObject lobbylistcontent;

    public List<GameObject> ListofLobbies = new List<GameObject>();


    public void GetListofLobbies()
    {
        SteamLobby.Instance.GetLobbiesList();
    }



    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result)
    {
        for(int i=0;i<lobbyIDs.Count;i++)
        {
            if (lobbyIDs[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject createditem = Instantiate(lobbydataitemprefab);
                createditem.GetComponent<LobbyDataEntry>().lobbyID = (CSteamID)lobbyIDs[i].m_SteamID;

                createditem.GetComponent<LobbyDataEntry>().lobbyname = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID, "name");

                createditem.GetComponent<LobbyDataEntry>().SetLobbyData();

                createditem.transform.SetParent(lobbylistcontent.transform);
                createditem.transform.localScale = Vector3.one;

                ListofLobbies.Add(createditem);
            }
        }
    }


    public void DestroyLobbies()
    {
        foreach(GameObject lobbyitem in ListofLobbies)
        {
            Destroy(lobbyitem);
        }
        ListofLobbies.Clear();
    }

}
