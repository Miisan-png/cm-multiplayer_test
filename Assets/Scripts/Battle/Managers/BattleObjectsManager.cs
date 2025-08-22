
using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class BattleObjectsManager : MonoBehaviour //Use to spawn ,despawn correct human and monster models
{
    private static BattleObjectsManager instance;
    public static BattleObjectsManager Instance => instance;

    public void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    [SerializeField] private GameObject P1Human;
    [SerializeField] private GameObject P1MonsterPair;
    [SerializeField] private GameObject P1MonsterSolo;
    [SerializeField] private GameObject P1HumanGlove;
    [SerializeField] private GameObject P1MonsterGlove;

    [SerializeField] private GameObject P2Human;
    public GameObject p2human => P2Human;
    [SerializeField] private GameObject P2MonsterPair;
    [SerializeField] private GameObject P2MonsterSolo;
    public GameObject p2monstersolo => P2MonsterSolo;
    [SerializeField] private GameObject P2HumanGlove;
    [SerializeField] private GameObject P2MonsterGlove;

    [SerializeField] private Animator P1MonsterAnimator;
    public Animator p1monsteranimator => P1MonsterAnimator;
    [SerializeField] private Animator P2MonsterAnimator;
    public Animator p2monsteranimator => P2MonsterAnimator;

    [SerializeField] private GameObject p1ShieldVFX;
    [SerializeField] private List<ParticleSystem> p1ShieldVFXColor1;
    [SerializeField] private List<ParticleSystem> p1ShieldVFXColor2;

    [SerializeField] private GameObject p2ShieldVFX;
    [SerializeField] private List<ParticleSystem> p2ShieldVFXColor1;
    [SerializeField] private List<ParticleSystem> p2ShieldVFXColor2;

    public Animator GetAnimatorbyIndex(int index)
    {
        return index == 1 ? p1monsteranimator : p2monsteranimator;
    }
    private void Start()
    {
        BattleManager.Instance.OnBattleStart += InitializeModels;
        BattleManager.Instance.OnPhaseChange += OnPhaseChanged;
        BattleManager.Instance.OnBattleEnd += OnBattleEnded;
        BattleManager.Instance.MonsterDamaged += OnMonsterDamaged;
        BattleManager.Instance.PlayerSelectAction += OnPlayerAction;
    }
    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStart -= InitializeModels;
            BattleManager.Instance.OnPhaseChange -= OnPhaseChanged;
            BattleManager.Instance.OnBattleEnd -= OnBattleEnded;
            BattleManager.Instance.MonsterDamaged -= OnMonsterDamaged;
        }
    }
    private void OnPhaseChanged(battleState S)
    {
        switch (S)
        {
            case battleState.Start:
                break;
            case battleState.Select:
                ActivateHumans(true);
                break;
            case battleState.Roll:
                ResetPlayerShields();

                break;
            case battleState.Execution:
                ActivateHumans(false);
                break;
            case battleState.End:
                ActivateHumans(true);
                break;
            case battleState.Dialogue:
                break;
            case battleState.None:
                break;
        }
    }

    // Separate method for battle end handling
    private void OnBattleEnded()
    {
        OnPlayerWon(BattleManager.Instance.playerwon);
    }

    private void InitializeModels()
    {
        GetCurrentModels();
        ResetPlayerShields();

        P1Human.SetActive(true);
        P1MonsterPair.SetActive(true);     

        bool setactive = BattleManager.Instance.battleType == battleType.Wild;
        P2Human.SetActive(!setactive);
        P2MonsterPair.SetActive(!setactive);
        P2HumanGlove.SetActive(!setactive);
        P2MonsterSolo.SetActive(setactive);
        P2MonsterGlove.SetActive(setactive);
    }

    private void GetCurrentModels()
    {
        List<GameObject> AllObjects = new List<GameObject>();

        AllObjects.Add(P1Human);
        AllObjects.Add(P1HumanGlove);
        AllObjects.Add(P1MonsterGlove);
        AllObjects.Add(P1MonsterPair);
        AllObjects.Add(P1MonsterSolo);
        AllObjects.Add(P2Human);
        AllObjects.Add(P2HumanGlove);
        AllObjects.Add(P2MonsterGlove);
        AllObjects.Add(P2MonsterPair);
        AllObjects.Add(P2MonsterSolo);

        for (int i = 0; i < AllObjects.Count; i++)
        {
            foreach (Transform child in AllObjects[i].transform)
            {
                Destroy(child.gameObject);
            }
        }



        GameObject p1human = NPCManager.Instance.assetdatabase.GetAssetsByID(SaveLoadManager.Instance.LoadSlotInfo(SaveLoadManager.Instance.CurrentSaveSlot).Gender == playerGender.Male ? -2 : -1).HumanPrefab;
        p1human.layer = LayerMask.NameToLayer("Player");

        GameObject p2human = null;
        if (BattleManager.Instance.npcinbattle != null)
        {
            p2human = NPCManager.Instance.assetdatabase.GetAssetsByID(BattleManager.Instance.npcinbattle.ID).HumanPrefab;
            p2human.layer = LayerMask.NameToLayer("Player");
        }

        Instantiate(p1human, P1Human.transform);
        Instantiate(p1human, P1HumanGlove.transform);

        if(p2human != null)
        {
            Instantiate(p2human, P2Human.transform);
            Instantiate(p2human, P2HumanGlove.transform);
        }

        MonsterAsset p1Asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(BattleManager.Instance.GetGlovebyIndex(1).equippedmonster.id);
        MonsterAsset p2Asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(BattleManager.Instance.GetGlovebyIndex(2).equippedmonster.id);

        if(p1Asset == null || p2Asset == null)
        {
            return;
        }

        GameObject p1Monster = p1Asset.BigPrefab;
        GameObject p2Monster = p2Asset.BigPrefab;

        if(p1Asset.ShieldVFXScale != Vector3.zero)
        {
            p1ShieldVFX.transform.localScale = p1Asset.ShieldVFXScale;
        }
        else
        {
            p1ShieldVFX.transform.localScale = Vector3.one;
        }

        if (p2Asset.ShieldVFXScale != Vector3.zero)
        {
            p2ShieldVFX.transform.localScale = p1Asset.ShieldVFXScale;
        }
        else
        {
            p2ShieldVFX.transform.localScale = Vector3.one;
        }

        GameObject P1SpawnedMonster = null;

        if (p1Monster != null)
        {
            P1SpawnedMonster = Instantiate(p1Monster, P1MonsterPair.transform);
        }

        GameObject P2SpawnedMonster = null;
        if(p2Monster != null)
        {
            if(BattleManager.Instance.battleType == battleType.NPC)
            {
                P2SpawnedMonster = Instantiate(p2Monster, P2MonsterPair.transform);
                Instantiate(p2Monster, P2MonsterGlove.transform);
            }
            else
            {
                P2SpawnedMonster = Instantiate(p2Monster, P2MonsterSolo.transform);
            }
        }

        if(P1SpawnedMonster != null)
        {
            foreach (Transform child in P1SpawnedMonster.transform)
            {
                 P1MonsterAnimator = child.gameObject.GetComponent<Animator>();
            }
        }

        if (P2SpawnedMonster != null)
        {
            foreach (Transform child in P2SpawnedMonster.transform)
            {
                P2MonsterAnimator = child.gameObject.GetComponent<Animator>();
            }
        }

        
    }

    private void ActivateHumans(bool Activate)
    {
        P1Human.SetActive(Activate);

        if(BattleManager.Instance.battleType != battleType.Wild)
        {
            P2Human.SetActive(Activate);
        }
    }

    private void OnPlayerAction(int player,selectAction action)
    {
        if(action == selectAction.Defend)
        {
            GameObject Shieldstoactivate = player == 1 ? p1ShieldVFX : p2ShieldVFX;

            Shieldstoactivate.SetActive(true);
        }
    }

    private void ResetPlayerShields()
    {
        p1ShieldVFX.SetActive(false);
        p2ShieldVFX.SetActive(false);
    }

    private void OnMonsterDamaged(int mon,int dmg)
    {
        if (BattleManager.Instance.GetControllerByIndex(mon).selectaction == selectAction.Defend)
        {
            GameObject Shieldstoactivate = mon == 1 ? p1ShieldVFX : p2ShieldVFX;
            Shieldstoactivate.SetActive(true);
        }

        if (dmg == 0) return;
        int opponent = mon == 1 ? 2 : 1;
        MonsterAsset asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(BattleManager.Instance.GetGlovebyIndex(opponent).equippedmonster.id);
        if (asset == null || asset.HitVFX == null) return;

        GameObject vfxparent = P1MonsterPair;

        if(mon == 2)
        {
            if (BattleManager.Instance.battleType == battleType.Wild)
            {
                vfxparent = P2MonsterSolo;
            }
            else
            {
                vfxparent = P2MonsterPair;
            }
        }

      
        Instantiate(asset.HitVFX, vfxparent.transform);
    }

    private void OnPlayerWon(int winner)
    {
        GameObject human = winner == 1 ? P2Human : P1Human;
        GameObject monster = winner == 1 ? P2MonsterPair : P1MonsterPair;
        if (BattleManager.Instance.battleType == battleType.Wild)
        {
            monster = winner == 1 ? P2MonsterSolo : P1MonsterPair;
        }

        human.SetActive(false);
        monster.SetActive(false);
    }
}
