using UnityEngine;

[System.Serializable]
public class Item_Fish : Item
{
    public float AnchorSpeed;
    public int AnchorChangeFrequency;
    public float AnchorRange;
    public float DropRate;
    public Item_Fish(int iD, string name, string description, ItemType type) : base(iD, name, description, type)
    {
    }

    public override void UseItem()
    {
        Debug.Log("Monster Ate fish");
    }
}
