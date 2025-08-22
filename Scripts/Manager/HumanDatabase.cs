using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCDatabase", menuName = "Game/NPCDatabase")]

public class HumanDatabase : ScriptableObject
{
    [SerializeField] private List<HumanAsset> humanAssets;
    public List<HumanAsset> humanassets => humanAssets;

    public HumanAsset GetAssetsByID(int ID)
    {
        HumanAsset asset = null;

        for (int i = 0; i < humanAssets.Count; i++)
        {
            if (ID == humanAssets[i].ID)
            {
                asset = humanAssets[i];
            }

        }
        return asset;
    }

}

[System.Serializable]
public class HumanAsset
{
    public int ID;
    public string Name;
    public string NameCN;
    public string NameJP;

    public string Move1Name;
    public string Move1NameCN;
    public string Move1NameJP;

    public GameObject HumanPrefab;
}
