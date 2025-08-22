using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MonsterManager : MonoBehaviour
{

    private static MonsterManager instance;
    public static MonsterManager Instance => instance;

    public void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LoadMonsterData($"AllMonsters.JSON");
    }

    public MonsterDatabase monsterDatabase; // Assign in Inspector
    [SerializeField] private List<Monster> currentMonsterList;
    public List<Monster> monsterlist => currentMonsterList;

    //Cleanup to use saveloadmanager
    void LoadMonsterData(string monsterlist)
    {
        MonsterList M = null;
        M = SaveLoadManager.Instance.LoadData<MonsterList>($"AllMonsters.JSON");

        currentMonsterList = M.monsters;

        Monster Mon = null;

        for (int i = 0; i < currentMonsterList.Count; i++)
        {
            Mon = currentMonsterList[i];
            currentMonsterList[i] = new Monster(Mon.id, Mon.name, Mon.element, Mon.rarity, Mon.hp, Mon.skillindex);
        }

        Debug.Log("Loaded Monsters");

    }
    public Monster GetMonsterByID(int ID)
    {
        Monster m = null;

        for (int i = 0; i < currentMonsterList.Count; i++)
        {
            if (currentMonsterList[i].id == ID)
            {
                m = currentMonsterList[i];
                break;
            }

        }

        if (m != null)
        {
            m = new Monster(m.id, m.name, m.element, m.rarity, m.hp, m.skillindex);
        }

        return m;
    }

    public Monster GetMonsterByName(string _name)
    {
        Monster m = null;

        for (int i = 0; i < currentMonsterList.Count; i++)
        {
            if (currentMonsterList[i].name == _name)
            {
                m = currentMonsterList[i];
                break;
            }

        }

        if (m != null)
        {
            m = new Monster(m.id, m.name, m.element, m.rarity, m.hp, m.skillindex);
        }

        return m;
    }

    public MonsterAsset GetMonsterAsset(string name)
    {
        return monsterDatabase.GetAssetsByName(name);
    }


    public string GetLocalizedElementString(Element element)
    {
        switch (SettingsManager.Instance.data.Language)
        {
            case GameLanguage.English:
                return element.ToString();
            case GameLanguage.Japanese:
                switch (element)
                {
                    case Element.Heat:
                        return "熱";
                    case Element.Electric:
                        return "電";
                    case Element.Wind:
                        return "風";
                    case Element.Solar:
                        return "陽";
                    case Element.Hydro:
                        return "水";
                    case Element.Sound:
                        return "音";
                }
                break;
            case GameLanguage.Mandarin:
                switch (element)
                {
                    case Element.Heat:
                        return "热";
                    case Element.Electric:
                        return "电";
                    case Element.Wind:
                        return "风";
                    case Element.Solar:
                        return "阳";
                    case Element.Hydro:
                        return "水";
                    case Element.Sound:
                        return "声";
                }
                break;
        }
        return element.ToString();
    }
}

[System.Serializable]
public class MonsterList
{
    public List<Monster> monsters;
}


