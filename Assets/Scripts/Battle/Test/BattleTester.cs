using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BattleTester : MonoBehaviour
{
    [SerializeField] public NPC NPCData;
    //IN an actual battle, get glove and controller from inventory
    [SerializeField] public BattleGlove glove;
    [SerializeField] public BattleController controller;
    [SerializeField] private int monsterCycle;
    [SerializeField] Monster HoveredMonster;

    [Header("SelectUI")]
    [SerializeField] TextMeshProUGUI MonsterHovered;
    [SerializeField] TextMeshProUGUI EquippedMonster;
    [SerializeField] List<TextMeshProUGUI> cells;

    [Header("BattleUI")]
    [SerializeField] TextMeshProUGUI PowerLevel;
    [SerializeField] TextMeshProUGUI SelectedMove;
    [SerializeField] TextMeshProUGUI CellCharged;
    [SerializeField] TextMeshProUGUI MonsterHP;
    [SerializeField] TextMeshProUGUI StatMods;

    private void Start()
    {
        HoveredMonster = MonsterManager.Instance.GetMonsterByID(monsterCycle);

        glove.InitializeCellMonster();

        CycleSelectedMonster();

        UpdateCellList();
        UpdateEquippedMonster();
    }

    public void CycleSelectedMonster()
    {
        if (BattleManager.Instance.state != battleState.None) return;
        monsterCycle++;

        Monster monster = MonsterManager.Instance.GetMonsterByID(monsterCycle);
        if (monster == null)
        {
            monsterCycle = 1;
            monster = MonsterManager.Instance.GetMonsterByID(monsterCycle);
        }

        HoveredMonster = monster;
        HoveredMonster.AssignLevel(1);

        MonsterHovered.text = $"{HoveredMonster.name}\nLvl {HoveredMonster.currentlevel}\nHP: {HoveredMonster.hp}\nElement: {HoveredMonster.element}\nSkillPower: {HoveredMonster.skillpower}";
    }

    public void AlterHoveredMonsterLevel(bool Plus)
    {
        if (BattleManager.Instance.state != battleState.None) return;
        int level = Plus ? 1 : -1;

        if(HoveredMonster !=null)
        {
            level += HoveredMonster.currentlevel;
            HoveredMonster.AssignLevel(level);
        }


        MonsterHovered.text = $"{HoveredMonster.name}\nLvl {HoveredMonster.currentlevel}\nHP: {HoveredMonster.hp}\nElement: {HoveredMonster.element}\nSkillPower: {HoveredMonster.skillpower}";
    }

    public void SetCells(int index)
    {
        if (BattleManager.Instance.state != battleState.None) return;

        Monster newInstance = new Monster(HoveredMonster.id, HoveredMonster.name, HoveredMonster.element,HoveredMonster.rarity, HoveredMonster.hp, HoveredMonster.skillindex);

        newInstance.AssignLevel(HoveredMonster.currentlevel);

        glove.SetCellMonster(index, newInstance);

        UpdateCellList();
    }
    private void UpdateCellList()
    {  
        for (int i = 1; i < glove.cellmonsters.Length; i++) //Skips first member as it is same as equipped monster
        {
            Monster monster = glove.cellmonsters[i];
            if (monster != null && monster.id != 0)
            {
                cells[i-1].text = $"Cell{i + 1}: {monster.name}\nLvl: {monster.currentlevel}\nHP: {monster.hp}\nElement: {monster.element}";
            }
            else
            {
                cells[i-1].text = $"Cell{i + 1}: (Empty)";
            }
        }
    }

    public void SetEquippedMonster()
    {
        if (BattleManager.Instance.state != battleState.None) return;

        Monster newInstance = new Monster(HoveredMonster.id, HoveredMonster.name, HoveredMonster.element, HoveredMonster.rarity, HoveredMonster.hp, HoveredMonster.skillindex);
        newInstance.AssignLevel(HoveredMonster.currentlevel);
        glove.SetEquippedMonster(newInstance);

        UpdateEquippedMonster();
        UpdateCellList();
    }

    private void UpdateEquippedMonster()
    {
        if(glove.equippedmonster != null)
        {
            EquippedMonster.text = $"Equipped Monster(Cell 1):\n{glove.equippedmonster.name}\nLvl {glove.equippedmonster.currentlevel}\nElement: {glove.equippedmonster.element}";
        }
        else
        {
            EquippedMonster.text = $"Equipped Monster(Cell 1):\n (Empty)";
        }
  
    }

}