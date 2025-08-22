using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Interactable interactable;
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject monsterPrefabtoHold;
    [SerializeField] private Monster monstertoHold;
    [SerializeField] private BattleGlove GlovetoHold;

    [SerializeField] private bool hasSpawned;
    public bool hasspawned => hasSpawned;

    private void Start()
    {
        interactable.onInteraction += () =>
        {
            TrySpawnMonster();
        };
    }

    public void TrySpawnMonster()
    {
        if(PlayerInventory.Instance.playerglove.equippedmonster == null || PlayerInventory.Instance.playerglove.equippedmonster.id == 0)
        {
            Debug.Log("No Monster Equipped");
            return;
        }

        List<Monster> ML = null;
        ML = MonsterManager.Instance.monsterlist;
        int rng = Random.Range(1, ML.Count+1);
        Monster m = MonsterManager.Instance.GetMonsterByID(rng);

        if(m == null)
        {
            Debug.Log("Monster not found.");
            return;
        }

        monstertoHold = m;

        hasSpawned = true;
        model.SetActive(false);

        monsterPrefab = null;

        if (MonsterManager.Instance.monsterDatabase.GetAssetsByID(m.id) != null)
        {
            monsterPrefab = MonsterManager.Instance.monsterDatabase.GetAssetsByID(m.id).BigPrefab;
        }
    
        if(monsterPrefab != null)
        {
            GameObject obj = Instantiate(monsterPrefab, transform);
            monsterPrefabtoHold = obj;
        }
        TimeManager.Instance.pauseTimer();
        interactable.switchoffPopup();
        startMonsterBattle();
    }

    public void startMonsterBattle()
    {
        BattleGlove monsterglove = new BattleGlove();
        monsterglove.SetEquippedMonster(monstertoHold);

        for (int i = 0; i < monstertoHold.currentlevel; i++)
        {
            switch (i)
            {
                case 2:
                    monsterglove.SetCellMonster(1, monstertoHold);
                    break;
                case 4:
                    monsterglove.SetCellMonster(2, monstertoHold);
                    break;
                case 6:
                    monsterglove.SetCellMonster(3, monstertoHold);
                    break;
                case 8:
                    monsterglove.SetCellMonster(4, monstertoHold);
                    break;
            }
        }
        GlovetoHold = monsterglove;
        //start battle
        GameManager.Instance.LoadBattleScene(PlayerInventory.Instance.playerglove, monsterglove, battleType.Wild);
        gameObject.SetActive(false);
    }


    public void resetSpawner(Transform newpos)
    {
        removeMonster();
        hasSpawned = false;
        model.SetActive(true);
        transform.position = newpos.position;
        transform.SetParent(newpos);
        interactable.resetInteraction();

        gameObject.SetActive(true);
    }

    public void removeMonster()
    {
        if(monsterPrefabtoHold!=null)
        {
            Destroy(monsterPrefabtoHold);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PlayerPet"))
        {
            PlayerPet pet = collision.gameObject.GetComponent<PlayerPet>();

            if(pet != null && pet.launched)
            {
                TrySpawnMonster();
            }
        }
    }
}
