using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    private static NPCManager instance;
    public static NPCManager Instance => instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        LoadNPCData();
    }

    [SerializeField] private HumanDatabase AssetDatabase;
    public HumanDatabase assetdatabase => AssetDatabase;
    [SerializeField] private NPCList AllNpcs;
    [SerializeField] private GameObject OverworldParent;

    private void LoadNPCData()
    {
        NPCList All = null;
        All = SaveLoadManager.Instance.LoadData<NPCList>("AllNPCData.json");
        AllNpcs = All;
    }

    public NPC GetNPCbyName(string name)
    {
        NPC npctoGet = null;

        for(int i =0;i<AllNpcs.npcs.Count;i++)
        {
            if(name == AllNpcs.npcs[i].Name)
            {
                npctoGet = AllNpcs.npcs[i];
            }
        }

        return npctoGet;
    }

    public List<NPC_Overworld> SearchForNPC()
    {
        NPC_Overworld[] CurrentNPCs = OverworldParent.GetComponentsInChildren<NPC_Overworld>(true);

        if (CurrentNPCs.Length == 0)
        {
            Debug.Log("No NPCs Found!");
            return new List<NPC_Overworld>();
        }
        List<NPC_Overworld> NPCList = new List<NPC_Overworld>(CurrentNPCs);

        return NPCList;
    }
}
[System.Serializable]
public class NPCList
{
    public List<NPC> npcs;
}