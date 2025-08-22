using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Game/MonsterDatabase")]
public class MonsterDatabase : ScriptableObject
{
    [SerializeField] private List<MonsterAsset> monsterAssets;

    public MonsterAsset GetAssetsByID(int ID)
    {
        MonsterAsset asset = null;

        for (int i=0;i<monsterAssets.Count;i++)
        {
            if(ID == monsterAssets[i].ID)
            {
                asset = monsterAssets[i];
            }

        }
        return asset;
    }

    public MonsterAsset GetAssetsByName(string n)
    {
        MonsterAsset asset = null;

        for (int i = 0; i < monsterAssets.Count; i++)
        {
            if (n == monsterAssets[i].Name)
            {
                asset = monsterAssets[i];
            }

        }
        return asset;
    }
}

[System.Serializable]
public class MonsterAsset
{
    public int ID; 
    public string Name;
    public string NameCN;
    public string NameJP;

    public float CameraIntroZoom;
    public float CameraIntroPan;
    public float CameraMainFOV;
    public float CameraMainPan;
    public Vector3 ShieldVFXScale;

    public string Move1Name;
    public string Move1NameCN;
    public string Move1NameJP;

    public AudioClip attackSound;
    public AudioClip damagedSound;
    public GameObject SmallPrefab;
    public GameObject BigPrefab;
    public GameObject AttackVFX;
    public GameObject HitVFX;
    public List<Sprite> PixelArt;
    public List<Sprite> PixelArt2;
}
