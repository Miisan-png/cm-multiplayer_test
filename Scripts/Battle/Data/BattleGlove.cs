
//Data for all equipped monsters and other things
using UnityEngine;
[System.Serializable]
public class BattleGlove
{
    [SerializeField] private Monster EquippedMonster;
    public Monster equippedmonster => EquippedMonster;
    [SerializeField] private Monster[] CellMonsters = new Monster[5];
    public Monster[] cellmonsters => CellMonsters;

    public void SetEquippedMonster(Monster Monster)
    {
        if (EquippedMonster != null)
        {
            EquippedMonster.Equipped = false;
        }

        EquippedMonster = Monster;
        EquippedMonster.Equipped = true;

        CellMonsters[0] = EquippedMonster;
    }

    public void SetCellMonster(int Slot,Monster Monster)
    {
        if (CellMonsters == null)
        {
            CellMonsters = new Monster[5];
        }

        // Prevent invalid slot access
        if (Slot <= 0 || Slot >= CellMonsters.Length)
        {
            return;
        }

        if (CellMonsters[Slot] != null && CellMonsters[Slot].id != 0)
        {
            CellMonsters[Slot].Equipped = false;
            CellMonsters[Slot] = null;

            CellMonsters[Slot] = Monster;
            CellMonsters[Slot].Equipped = true;
        }
        else
        {
            CellMonsters[Slot] = Monster;
            CellMonsters[Slot].Equipped = true;
        }

        CompactMonsterArray();
    }

    public void RemoveCellMonster(int Slot)
    {
        // Prevent invalid slot access
        if (Slot <= 0 || Slot >= CellMonsters.Length)
        {
            return;
        }

        if (CellMonsters[Slot] != null && CellMonsters[Slot].id != 0)
        {
            CellMonsters[Slot].Equipped = false;
            CellMonsters[Slot] = null;
        }
        CompactMonsterArray();
    }

    /// Shifts all monsters to the left to fill empty slots (null or id=0).
    private void CompactMonsterArray()
    {
        int writeIndex = 1;

        for (int readIndex = 1; readIndex < CellMonsters.Length; readIndex++)
        {
            if (CellMonsters[readIndex] != null && CellMonsters[readIndex].id != 0)
            {
                CellMonsters[writeIndex] = CellMonsters[readIndex];
                writeIndex++;
            }
        }

        for (; writeIndex < CellMonsters.Length; writeIndex++)
        {
            CellMonsters[writeIndex] = null;
        }
    }

    public void InitializeCellMonster()
    {
        CellMonsters = new Monster[5];
    }

}
