using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DialogueManager;

public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;
    public static ItemManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadAllItems();
    }

    [SerializeField] private ItemList AllItems;
    [SerializeField] private FishList AllFishes;

    [SerializeField] private Dictionary<System.Type, List<Item>> ItemDictionary = new Dictionary<System.Type, List<Item>>();

    public void LoadAllItems()
    {
        ItemList IL = SaveLoadManager.Instance.LoadData<ItemList>("AllItems.JSON");
        AllItems = IL;

        FishList FL = SaveLoadManager.Instance.LoadData<FishList>("AllFishes.JSON");
        AllFishes = FL;


        if(FL !=null && FL.Fishes.Count >0)
        {
            ItemDictionary[typeof(Item_Fish)] = AllFishes.Fishes.Cast<Item>().ToList();
        }


        if (IL != null && IL.Items.Count > 0)
        {
            ItemDictionary[typeof(Item)] = AllItems.Items;
        }

    }

    public T GetItembyID<T>(int _ID) where T : Item
    {
        if (ItemDictionary.TryGetValue(typeof(T), out List<Item> items))
        {
            return items.FirstOrDefault(item => item.ID == _ID) as T;
        }

        return null;
    }
    public T GetItembyName<T>(string _Name) where T : Item
    {
        if (ItemDictionary.TryGetValue(typeof(T), out List<Item> items))
        {
            return items.FirstOrDefault(item => item.Name == _Name) as T;
        }

        return null;
    }

    public Item_Fish GetRandomFish()
    {
        if (AllFishes.Fishes.Count < 1) return null;

        Item_Fish I = AllFishes.Fishes[0];
        float rng = Random.Range(0f, 1f);
        int fishrng = Random.Range(0, AllFishes.Fishes.Count);


        for (int i = 0; i < 10; i++)
        {
           if(AllFishes.Fishes[fishrng].DropRate >= rng)
           {
                I = AllFishes.Fishes[fishrng];
                break;
           }
           else
           {
               fishrng = Random.Range(0, AllFishes.Fishes.Count);
               rng = Random.Range(0f, 1f);
           }
        }

        return I;
    }



}

[System.Serializable]
public class ItemList
{
    public List<Item> Items;
}

[System.Serializable]
public class FishList
{
    public List<Item_Fish> Fishes;
}

