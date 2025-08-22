using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private static PlayerInventory instance;
    public static PlayerInventory Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        loadPlayerMonsters();
        loadPlayerItems();

        SaveLoadManager.Instance.OnAutoSave += () => {
            SaveLoadManager.Instance.SaveData<MonsterList>(MonsterInventory, "PlayerMonsters.JSON");
            SaveLoadManager.Instance.SaveData<ItemList>(ItemInventory, "PlayerItems.JSON");
            SaveLoadManager.Instance.SaveData<BattleGlove>(PlayerGlove, "PlayerGlove.JSON");
            Debug.Log("TryingtoSave");
        };
    }


    public Action<Monster> onMonsterGet;
    public Action<Item> onItemGet;

    [SerializeField] private MonsterList MonsterInventory;
    public List<Monster> monsterinventory => MonsterInventory.monsters;

    [SerializeField] private ItemList ItemInventory;
    public List<Item> iteminventory => ItemInventory.Items;

    [SerializeField] private BattleGlove PlayerGlove;
    public BattleGlove playerglove => PlayerGlove;

    [SerializeField] private playerGender PlayerGender;
    public playerGender playergender => PlayerGender;

    public void SetGender(playerGender gender)
    {
        PlayerGender = gender;
    }


    public void addMonstertoInventory(Monster M)
    {
        MonsterInventory.monsters.Add(M);

        onMonsterGet?.Invoke(M);
    }

    public void addItemtoInventory(Item _Item)
    {
        if(ItemInventory == null)
        {
            ItemInventory = new ItemList();
       
        }
        if(ItemInventory.Items == null)
        {
            ItemInventory.Items = new List<Item>();
        }

        ItemInventory.Items.Add(_Item);

        onItemGet?.Invoke(_Item);
    }

    public void loadPlayerMonsters()
    {
    MonsterInventory.monsters = new List<Monster>();
    MonsterList m = SaveLoadManager.Instance.LoadData<MonsterList>("PlayerMonsters.JSON");
    if (m == null) {
        m = new MonsterList { monsters = new List<Monster>() };
    }
    MonsterInventory = m;

    if(MonsterInventory != null && MonsterInventory.monsters != null)
    {
        foreach (Monster M in MonsterInventory.monsters)
        {
            M.AssignLevel(M.currentlevel);
        }
    }

    // Safety handling | Null Reference Exception

    BattleGlove playerglove = SaveLoadManager.Instance.LoadData<BattleGlove>("PlayerGlove.JSON");
    if (playerglove == null) {
        playerglove = new BattleGlove();
    }
    PlayerGlove = playerglove;

    
    if (PlayerGlove != null && PlayerGlove.equippedmonster != null && PlayerGlove.equippedmonster.id != 0)
    {
            PlayerGlove.SetEquippedMonster(PlayerGlove.equippedmonster);
            PlayerGlove.equippedmonster.AssignLevel(PlayerGlove.equippedmonster.currentlevel);
    }

    if (PlayerGlove != null && PlayerGlove.cellmonsters != null)
        {
            for (int i = 1; i < 5; i++)
            {
                if (PlayerGlove.cellmonsters[i] != null && PlayerGlove.cellmonsters[i].id != 0)
                {
                    PlayerGlove.cellmonsters[i].AssignLevel(PlayerGlove.cellmonsters[i].currentlevel);
                }
            }
        }
    }

    public void loadPlayerItems()
    {
    ItemInventory.Items = new List<Item>();
    ItemList i = SaveLoadManager.Instance.LoadData<ItemList>("PlayerItems.JSON");
    if (i == null) {
        i = new ItemList { Items = new List<Item>() };
    }
    ItemInventory = i;
    }
}
public enum playerGender
{
     Female,Male
}