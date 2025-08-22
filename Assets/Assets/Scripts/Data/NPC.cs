using UnityEngine;
[System.Serializable]
public class NPC
{
    public int ID;
    public string Name;
    public string Description;


    public NPC(int _ID,string _Name,string _Description)
    {
        ID = _ID;
        Name = _Name;
        Description = _Description;
    }


}
