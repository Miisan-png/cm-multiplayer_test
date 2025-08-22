using UnityEngine;

[System.Serializable]
public abstract class Item 
{
    public int ID;
    public string Name;
    public string Description;
    public ItemType Type;

    public abstract void UseItem();

    public Item(int iD, string name, string description, ItemType type)
    {
        ID = iD;
        Name = name;
        Description = description;
        Type = type;
    }
}
public enum ItemType
{
    LurePotion,Fish
}