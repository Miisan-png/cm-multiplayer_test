using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PokeDexDatabase", menuName = "Game/PokeDexDatabase")]

public class PokeDexDatabase : ScriptableObject
{
    [SerializeField] private List<PokeDexData> DexData;
    public List<PokeDexData> dexdata => DexData;
    public PokeDexData GetDataByID(int ID)
    {
        PokeDexData asset = DexData[0];

        for (int i = 0; i < DexData.Count; i++)
        {
            if (ID == DexData[i].ID)
            {
                asset = DexData[i];
            }

        }
        return asset;
    }

    public PokeDexData GetDataByName(string n)
    {
        PokeDexData asset = DexData[0];

        for (int i = 0; i < DexData.Count; i++)
        {
            if (n == DexData[i].Name)
            {
                asset = DexData[i];
            }

        }
        return asset;
    }
}
[System.Serializable]
public class PokeDexData
{
    public int ID;
    public string Name;
    public Sprite PageSprite;
    public Sprite EnName;
    public Sprite JpName;
    public Sprite CnName;
    public Vector2 NameVector;
}
