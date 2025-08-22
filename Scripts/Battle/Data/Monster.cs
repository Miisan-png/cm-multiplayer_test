
using UnityEngine;

[System.Serializable]
public class Monster
{
    [SerializeField] private int ID;
    public int id => ID;
    [SerializeField] private string Name;
    public string name => Name;
    [SerializeField] private Element Element = Element.None;
    public Element element => Element;

    [SerializeField] private MonsterRarity Rarity;
    public MonsterRarity rarity => Rarity;

    [SerializeField] private int MaxEXP;
    public int maxexp => MaxEXP;
    [SerializeField] private int CurrentEXP;
    public int currentexp => CurrentEXP;
    [SerializeField] private int CurrentLevel;
    public int currentlevel => CurrentLevel;

    [SerializeField] private int SkillPower = 5;
    public int skillpower => SkillPower;
    [SerializeField] private int SkillIndex;
    public int skillindex => SkillIndex;

    [SerializeField] private int HP;
    public int hp => HP;

    public bool Equipped;

  
    public Monster(int iD, string name, Element element,MonsterRarity _Rarity, int hp,int skillIndex)
    {
        ID = iD;
        Name = name;
        Element = element;
        Rarity = _Rarity;
        HP = hp;
        Equipped = false;
        SkillIndex = skillIndex;
    }

    public void GainEXP(int exp)
    {
        // Calculate MaxEXP for the current level (before leveling up)
        MaxEXP = 10 * ((int)Rarity + 1) * CurrentLevel;

        CurrentEXP += exp;

        // Calculate how many level ups should occur
        int levelsGained = 0;
        while (CurrentEXP >= MaxEXP)
        {
            CurrentEXP -= MaxEXP;
            levelsGained++;

            // Update MaxEXP for the next level (since CurrentLevel increases)
            MaxEXP = 10 * ((int)Rarity + 1) * (CurrentLevel + levelsGained);
        }

        if (levelsGained > 0)
        {
            AssignLevel(CurrentLevel + levelsGained);
        }
    }

    public void AssignLevel(int level)
    {
        CurrentLevel = level;

        CurrentLevel = Mathf.Clamp(CurrentLevel, 1, 100);

        HP = CurrentLevel;
    }
}

public enum MonsterRarity
{
    common, uncommon, rare, super_rare, mystic
}
public enum Element
{
    None,      //0
    Heat,      //1
    Electric,  //2
    Wind,      //3
    Solar,     //4
    Hydro,     //5
    Sound      //6
}

