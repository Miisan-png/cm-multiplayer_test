using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    [SerializeField] private int PlayerIndex;
    public int playerindex => PlayerIndex;

    [SerializeField] private int CurrentHP;
    public int hp => CurrentHP;

    [SerializeField] private int MaxHP;
    public int maxhp => MaxHP;

    [SerializeField] private List<ElementWrapper> NumberPool;
    public List<ElementWrapper> numberpool => NumberPool;

    [SerializeField] private Monster[] ChargedMonster = new Monster[5];
    public List<Monster> chargedmonster => ChargedMonster.ToList();

    [SerializeField] private BattleGlove Glove;
    public BattleGlove glove => Glove;

    [SerializeField] private ElementWrapper[] CurrentSlots = new ElementWrapper[5];
    public ElementWrapper[] numberslots => CurrentSlots;
    [SerializeField] private bool[] SlotsActivated = new bool[5];
    public bool[] slotsactivated => SlotsActivated;

    [SerializeField] private int RowIndex;
    [SerializeField] private int ColumnIndex;

    [SerializeField] private Element[] SlotsAltered;
    public Element[] slotsaltered => SlotsAltered;

    [SerializeField] private Dictionary<ChargeEffects, int> EffectsDictionary;
    public Dictionary<ChargeEffects, int> effects => EffectsDictionary;

    [SerializeField] private int DamagetoDeal;
    public int damagetodeal => DamagetoDeal;

    [SerializeField] private selectAction SelectAction;
    public selectAction selectaction => SelectAction;

    [SerializeField] private int DefendCooldown;
    public int defendcooldown => DefendCooldown;

    public Action OnConfirmed;
    private bool Confirmed;
    public bool confirmed => Confirmed;

    [SerializeField] protected float RerollTimer;
    protected float originalrerolltimer;
    public float rerolltimer => RerollTimer;

    [SerializeField] protected float SkiptoTime = 5f;
    protected float originalskiptimer;
    public float skiptotime => SkiptoTime;

    public Action OnSlotsChanged;
    public Action OnCellCharged;
    public Action ChargedMonsterReset;
    public Action OnSelectActionChanged;
    public Action OnPlayerDamaged;
    public Action OnEffectAdded;
    public Action OnEffectDispel;

    protected void Start()
    {
        originalrerolltimer = rerolltimer;
        originalskiptimer = skiptotime;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        BattleManager.Instance.OnPhaseChange += OnPhaseChange;
        BattleManager.Instance.OnRollEnd += OnBattleRollEnd;
        BattleManager.Instance.OnSelecEnd += OnBattleSelectEnd;
        BattleManager.Instance.OnExecuteEnd += OnBattleExecuteEnd;
        BattleManager.Instance.MonsterDamaged += OnMonsterDamaged;
    }

    private void UnsubscribeAllEvents()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnPhaseChange -= OnPhaseChange;
            BattleManager.Instance.OnRollEnd -= OnBattleRollEnd;
            BattleManager.Instance.OnSelecEnd -= OnBattleSelectEnd;
            BattleManager.Instance.OnExecuteEnd -= OnBattleExecuteEnd;
            BattleManager.Instance.MonsterDamaged -= OnMonsterDamaged;
        }
    }

    private void OnPhaseChange(battleState state)
    {
        switch (state)
        {
            case battleState.Start:
                break;
            case battleState.Roll:
                OnBattleRoll();
                break;
            case battleState.Execution:
                break;
            case battleState.End:
                break;
            case battleState.Dialogue:
                break;
            case battleState.None:
                HandleOnBattleEnd();
                break;
            case battleState.Select:
                OnBattleSelect();
                break;
        }
    }

    private void OnMonsterDamaged(int Target, int damage)
    {
        int target = PlayerIndex == 1 ? 2 : 1;
        if (Target == target)
        {
            CheckForEffectActivation();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeAllEvents();
    }

    public void InitializeController(BattleGlove newglove)
    {
        Glove = newglove;
        EffectsDictionary = new Dictionary<ChargeEffects, int>();
        CurrentHP = Glove.equippedmonster.hp;
        MaxHP = Glove.equippedmonster.hp;

        int totalmonsters = 0;

        for (int i = 0; i < Glove.cellmonsters.Length; i++)
        {
            if (Glove.cellmonsters[i] != null && Glove.cellmonsters[i].id != 0)
            {
                totalmonsters++;
            }
        }

        MaxHP = CurrentHP;
        SlotsActivated = new bool[5];
        SlotsAltered = new Element[totalmonsters];
        DefendCooldown = 0;
        ResetTimers();
        InitializeNumberPool();
    }

    private void ResetTimers()
    {
        RerollTimer = originalrerolltimer;
        SkiptoTime = originalskiptimer;
    }

    public void TakeDamage(int dmg)
    {
        CurrentHP -= dmg;
        if (CurrentHP <= 0) CurrentHP = 0;
        OnPlayerDamaged?.Invoke();
    }

    private void InitializeNumberPool()
    {
        NumberPool = new List<ElementWrapper>();
        var BaseElements = new List<Element> { Element.Electric, Element.Hydro, Element.Heat, Element.Wind, Element.Solar, Element.Sound };
        int target = PlayerIndex == 1 ? 2 : 1;
        BattleGlove EnemyGlove = BattleManager.Instance.GetGlovebyIndex(target);

        foreach (var element in BaseElements)
        {
            NumberPool.Add(new ElementWrapper(CompareHelper.GetNumberfromElement(element), element, false));
        }

        //add based on self glove
        for(int i=0;i<Glove.equippedmonster.currentlevel;i++)
        {
            NumberPool.Add(new ElementWrapper(CompareHelper.GetNumberfromElement(Glove.equippedmonster.element), Glove.equippedmonster.element, false));
        }

        //Add based on enemy glove
        for (int i = 0; i < EnemyGlove.equippedmonster.currentlevel; i++)
        {
            NumberPool.Add(new ElementWrapper(CompareHelper.GetNumberfromElement(EnemyGlove.equippedmonster.element), EnemyGlove.equippedmonster.element, false));
        }

        for (int i = 0; i < SlotsActivated.Length; i++)
        {
            SlotsActivated[i] = false;
        }
    }

    private void OnBattleRoll()
    {
        ClearEffectsFirstTurn();
        ChargedMonster = new Monster[5];
        Confirmed = false;
        DamagetoDeal = 0;
        ChargedMonsterReset?.Invoke();
        RepeatExistingEffects(EffectTiming.BeforeRoll);
    }

    private void OnBattleRollEnd()
    {
        MatchSlotNumbers();
        RepeatExistingEffects(EffectTiming.AfterRoll);
    }

    protected void OnBattleSelect()
    {
        ChargedMonster = new Monster[5];
        SelectAction = selectAction.None;
        Confirmed = false;

        DefendCooldown = Mathf.Max(0, DefendCooldown - 1);
        RepeatExistingEffects(EffectTiming.BeforeSelect);
        ChargedMonsterReset?.Invoke();
    }

    private void OnBattleSelectEnd()
    {
        SetThisTurnDamage();
        ConsumeSlotsActivation();
        HandleAfterSelectAction();
        RepeatExistingEffects(EffectTiming.AfterSelect);
    }

    private void OnBattleExecuteEnd()
    {
        ConsumeEffectsDuration();
    }


    #region Effects_Override
    public void AlterSlotBehaviour(int index, bool activate, Element effectelement)
    {
        if (activate && SlotsAltered[index] != Element.None) return;

        if (activate)
        {
            SlotsAltered[index] = effectelement;
        }
        else
        {
            SlotsAltered[index] = Element.None;
        }
        Debug.Log($"Slot{index} altered{activate}");
    }

    public void AlterSkiptoTime(float time)
    {
        SkiptoTime = time;
    }
    public void AlterRerollTimer(float newTimer)
    {
        RerollTimer = newTimer;

        if (RerollTimer <= 0.5f)
        {
            RerollTimer = 0.5f;
        }
    }
    #endregion

    #region EffectsHandler
    private bool EffectsActivationCondition()
    {
        return true;
    }
    private void CheckForEffectActivation()
    {
        if (!EffectsActivationCondition()) return;

        List<Element> ActivatedElements = new List<Element>();

        for (int i = 1; i < CurrentSlots.Length; i++) //Only monsters other than equipped monster can activate effect
        {
            if (ChargedMonster[i] == null || ChargedMonster[i].element == Element.None)
            {
                continue;
            }
            else if ((CompareHelper.ElementMatchesInt(CurrentSlots[i].number, ChargedMonster[i].element)||CompareHelper.ElementMatchesInt(CurrentSlots[i].number, glove.equippedmonster.element)) && !ActivatedElements.Contains(ChargedMonster[i].element))
            {
                if (ChargeEffectsDatabase.Instance.alleffects.TryGetValue(ChargedMonster[i].element, out Dictionary<int, ChargeEffects> activationDict))
                {
                    ChargeEffects effect = activationDict[ChargedMonster[i].skillindex];

                    ChargeEffects effectCloned = new ChargeEffects(effect.ActivationEffect, effect.PersistentEffect, effect.DispelEffect, effect.timing, effect.ElementID, effect.EffectID, effect.turns);

                    int target = playerindex == 1 ? 2 : 1;

                    effectCloned.AssignContext(
                        new ChargeEffectContext(
                            glove.equippedmonster,
                            ChargedMonster[i],
                            BattleManager.Instance.GetEquippedMonster(target),
                            this.glove,
                            BattleManager.Instance.GetGlovebyIndex(target),
                            this,
                            BattleManager.Instance.GetControllerByIndex(target)      
                            )      
                        );

                    BattleManager.Instance.GetControllerByIndex(target).AddEffect(effectCloned);

                    ActivatedElements.Add(effectCloned.ElementID);
                }
            }
        }
    }

    public void AddEffect(ChargeEffects effecttoadd)
    {
        var keys = EffectsDictionary.Keys.ToList();
        foreach (var key in keys)
        {
            if (key.ElementID == effecttoadd.ElementID && key.EffectID == effecttoadd.EffectID)
            {
                if (EffectsDictionary[key] > 0)
                {
                    EffectsDictionary[key] = key.turns; //Refresh 3 turns
                    key.FirstTurn = true;
                    Debug.Log($"{key.ElementID} effect charges updated, new Turns = {EffectsDictionary[key]}");
                    OnEffectAdded?.Invoke();
                    return;
                }
                else
                {
                    EffectsDictionary.Remove(key); // Remove expired effects and add a fresh one below.
                    continue;
                }
            }
        }

        for (int i = 0; i < SlotsAltered.Length; i++)
        {
            if (SlotsAltered[i] == effecttoadd.ElementID)
            {
                //Already has a slot with the same effect
                return;
            }
        }

        Debug.Log($"Player{PlayerIndex}added {effecttoadd.ElementID} effect");

        effecttoadd.ActivationEffect.Invoke(effecttoadd.context); // Activate

        EffectsDictionary[effecttoadd] = effecttoadd.turns;
        Debug.Log($"Effect added: {effecttoadd.ElementID} effect, {EffectsDictionary[effecttoadd]}");
        OnEffectAdded?.Invoke();
    }

    private void ConsumeEffectsDuration()
    {
        if (EffectsDictionary.Count == 0) return;
        // First, collect the keys you want to update
        var keys = EffectsDictionary.Keys.ToList();

        foreach (var key in keys)
        {
            if (EffectsDictionary[key] > 0 && !key.FirstTurn)
            {
                EffectsDictionary[key] -= 1;
                Debug.Log($"{key.ElementID} Effect used 1 turn from Player{playerindex}");

                if (EffectsDictionary[key] <= 0)
                {
                    Debug.Log($"{key.ElementID} Effect expired from player{playerindex}");
                    DispelEffect(key.ElementID, key.context.MonsterActivated.skillindex);
                }
            }
        }
    }

    public void RepeatExistingEffects(EffectTiming _timing) // Only for those effects that need to happen again every round like rerolling
    {
        if (EffectsDictionary.Count == 0) return;
        // First, collect the keys you want to update
        var keys = EffectsDictionary.Keys.ToList();

        foreach (var key in keys)
        {
            if (EffectsDictionary[key] > 0 && key.timing == _timing && !key.FirstTurn)
            {
                ChargeEffects effectwithContext = UpdateEffectContext(key,key.context.MonsterActivated);
                effectwithContext.PersistentEffect.Invoke(effectwithContext.context); // Activate again
            }
        }
    }

    public void DispelEffect(Element ID,int skill)
    {
        var keys = EffectsDictionary.Keys.ToList();
        var toRemove = new List<ChargeEffects>();

        foreach (var key in keys)
        {
            if (key.ElementID == ID)
            {
                ChargeEffects effectwithContext = UpdateEffectContext(key, key.context.MonsterActivated);
                effectwithContext.DispelEffect.Invoke(effectwithContext.context);

                EffectsDictionary[key] = 0; //Remove all charges
                toRemove.Add(key);
            }
        }

        foreach (var key in toRemove)
        {
            EffectsDictionary.Remove(key);
            OnEffectDispel?.Invoke();
        }
    }

    private void ClearEffectsFirstTurn()
    {
        if (EffectsDictionary.Count == 0) return;
        var keys = EffectsDictionary.Keys.ToList();

        foreach (var key in keys)
        {
            key.FirstTurn = false;
            continue;
        }
    }

    private ChargeEffects UpdateEffectContext(ChargeEffects effecttoupdate,Monster monsteractivated)
    {
        ChargeEffects effectCloned = new ChargeEffects(effecttoupdate.ActivationEffect, effecttoupdate.PersistentEffect, effecttoupdate.DispelEffect, effecttoupdate.timing, effecttoupdate.ElementID,effecttoupdate.EffectID,effecttoupdate.turns);
        int target = effecttoupdate.context.TargetController.playerindex;
        int self = target == 1 ? 2 : 1;

        effectCloned.AssignContext(new ChargeEffectContext(
            BattleManager.Instance.GetEquippedMonster(self),
            monsteractivated,
            BattleManager.Instance.GetEquippedMonster(target),
            BattleManager.Instance.GetGlovebyIndex(self),
            BattleManager.Instance.GetGlovebyIndex(target),
            BattleManager.Instance.GetControllerByIndex(self),
            BattleManager.Instance.GetControllerByIndex(target)
        ));

        return effectCloned;
    }
    #endregion

    private bool ChargeCondition(int slot)
    {
        return !Confirmed &&
               BattleManager.Instance.state == battleState.Select &&
               SelectAction == selectAction.Attack &&
               SlotsActivated[slot];
    }
    protected void HandleChargeCell(int slot)
    {
        if (!ChargeCondition(slot)) return;

        if (ChargedMonster[slot] != null && ChargedMonster[slot].id != 0)
        {
            ChargedMonster[slot] = null;
        }
        else
        {
            if (Glove.cellmonsters[slot] != null && glove.cellmonsters[slot].id != 0)
            {
                ChargedMonster[slot] = Glove.cellmonsters[slot];
            }
        }

        OnCellCharged?.Invoke();
    }

    private void MatchSlotNumbers()
    {
        for (int i = 0; i < 5; i++)
        {
            if (Glove.cellmonsters[i]!=null && glove.cellmonsters[i].id != 0)
            {
                SlotsActivated[i] = CompareHelper.ElementMatchesInt(CurrentSlots[i].number, Glove.cellmonsters[i].element) || CompareHelper.ElementMatchesInt(CurrentSlots[i].number, Glove.equippedmonster.element);
            }
        }
    }

    private void SetThisTurnDamage()
    {
        DamagetoDeal = GetTotalDamage();
    }

    public int GetTotalDamage()
    {
        int damage = 0;
        int chargedslot = 0;

        // Calculate base damage from activated slots
        for (int i = 0; i < 5; i++)
        {
            if (SlotsActivated[i] && ChargedMonster[i] != null && ChargedMonster[i].id != 0)
            {
                damage += (i == 0) ? 2 : 1;
                chargedslot++;
            }
        }

        // Calculate bonus damage based on skillpower and charged slots
        int skillpower = Glove.equippedmonster.skillpower;
        if (skillpower > 0 && chargedslot > 0)
        {
            int[] bonusTiers = { 0, 1, 3, 5, 7, 10 }; // Bonus damage for each tier
            int applicableTier = Math.Min(chargedslot, skillpower);
            damage += bonusTiers[applicableTier];
        }
        return damage;
    }

    private void ConsumeSlotsActivation()
    {
        for(int i=0;i<ChargedMonster.Length;i++)
        {
            if (ChargedMonster[i]!= null && ChargedMonster[i].id != 0)
            {
                SlotsActivated[i] = false;
            }
        }
    }

    private void HandleAfterSelectAction()
    {
        switch (SelectAction)
        {
            case selectAction.None:
                SelectAction = selectAction.Skip;
                break;
            case selectAction.Skip:
                break;
            case selectAction.Attack:
                break;
            case selectAction.Defend:
                DefendCooldown = 2;
                break;
        }
    }

    public bool GetMatchedNumber(int index)
    {
        return CompareHelper.ElementMatchesInt(CurrentSlots[index].number, Glove.cellmonsters[index].element) || CompareHelper.ElementMatchesInt(CurrentSlots[index].number, Glove.equippedmonster.element);
    }

    public bool ConfirmCondition()
    {
        if (BattleManager.Instance.state != battleState.Roll &&
            BattleManager.Instance.state != battleState.Select)
        {
            return false;
        }

        if (!Confirmed)
        {
            if (BattleManager.Instance.state == battleState.Roll && BattleManager.Instance.canroll)
            {
                return true;
            }
            else if (selectaction == selectAction.Attack)
            {
                return CheckforChargedMonster();
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckforChargedMonster()
    {
        foreach (var monster in chargedmonster)
        {
            if (monster != null && monster.id != 0)
            {
                return true;
            }
        }
        return false;
    }

    protected void HandleConfirm()
    {
        if (!ConfirmCondition()) return;

        if(BattleManager.Instance.state == battleState.Roll)
        {
            BattleManager.Instance.ConfirmMove(playerindex);
            Confirmed = true;
            OnConfirmed?.Invoke();
        }
        else if (BattleManager.Instance.state == battleState.Select)
        {
            if(SelectAction != selectAction.None)
            {
                BattleManager.Instance.ConfirmSelect();

                Confirmed = true;
                OnConfirmed?.Invoke();
            }
        }
    }

    public bool SelectActionCondition(selectAction move)
    {
        // Original condition
        if (Confirmed || SelectAction == move)
            return false;

        switch (move)
        {
            case selectAction.Skip:
                break;
            case selectAction.Attack:
                bool hasActivatedslot = false;
                for (int i = 0; i < SlotsActivated.Length; i++)
                {
                    if (SlotsActivated[i])
                    {
                        hasActivatedslot = true;
                        break; // No need to check further if we found one
                    }
                }

                if (!hasActivatedslot)
                {
                    Debug.Log("No slots activated, cannot attack!");
                    return false;
                }

                for (int i = 0; i < CurrentSlots.Length; i++)
                {
                    if (CurrentSlots[i].number == 0 && Glove.cellmonsters[i] != null && Glove.cellmonsters[i].id != 0)
                    {
                        var keys = EffectsDictionary.Keys.ToList();
                        foreach (var key in keys)
                        {
                            if (key.ElementID == Element.Heat && key.EffectID == 1 && EffectsDictionary[key] > 0)
                            {
                                Debug.Log("In Fire Effect!");
                                return false;
                            }
                        }
                        break;
                    }
                }
                break;
            case selectAction.Defend:
                if(DefendCooldown > 0)
                {
                    Debug.Log("Defend On Cooldown!");
                    return false;
                }

                for(int i=0;i<CurrentSlots.Length;i++)
                {
                    if (CurrentSlots[i].number == 0 && Glove.cellmonsters[i] != null && Glove.cellmonsters[i].id != 0)
                    {
                        var keys = EffectsDictionary.Keys.ToList();
                        foreach (var key in keys)
                        {
                            if (key.ElementID == Element.Heat && key.EffectID == 1 && EffectsDictionary[key] > 0)
                            {
                                Debug.Log("In Fire Effect!");
                                return false;
                            }
                        }
                        break;
                    }
                }
                break;
        }

        return true;
    }

    protected void HandleSelectAction(selectAction move)
    {
        if (!SelectActionCondition(move)) return;

        SelectAction = move;

        switch (SelectAction)
        {
            case selectAction.None:
                ChargedMonster = new Monster[5];
                break;
            case selectAction.Skip:
                ChargedMonster = new Monster[5];
                break;
            case selectAction.Attack:
                ChargedMonster = new Monster[5];
                break;
            case selectAction.Defend:
                ChargedMonster = new Monster[5];
                break;
        }
        OnSelectActionChanged?.Invoke();
        OnCellCharged?.Invoke(); //Refresh cell status
    }
    public void AddtoNumberPool(Element elementtoadd,bool fake)
    {
        if (NumberPool == null)
        {
            InitializeNumberPool();
        }

        int numbertoadd = 0;

        switch (elementtoadd)
        {
            case Element.Heat:
                numbertoadd = 5;
                break;
            case Element.Electric:
                numbertoadd = 2;
                break;
            case Element.Wind:
                numbertoadd = 6;
                break;
            case Element.Solar:
                numbertoadd = 7;
                break;
            case Element.Hydro:
                numbertoadd = 3;
                break;
            case Element.Sound:
                numbertoadd = 8;
                break;
        }

        NumberPool.Add(new ElementWrapper(numbertoadd, elementtoadd,fake));

    }
    public void RemoveNumberfromPool(Element elementtoadd, bool fake)
    {
        if (NumberPool == null || NumberPool.Count == 0) return;

        // Find the first matching ElementWrapper where both element and fake bool match
        var wrapperToRemove = NumberPool.FirstOrDefault(wrapper =>
            wrapper.element == elementtoadd && wrapper.fake == fake);

        if (wrapperToRemove != null)
        {
            NumberPool.Remove(wrapperToRemove);
        }
    }

    public void  RerollNumberSlots()
    {
        ElementWrapper[] wrapper = new ElementWrapper[5];

        for (int i = 0; i < Glove.cellmonsters.Length; i++)
        {
            if (SlotsActivated[i]) //dont reroll
            {
                wrapper[i] = CurrentSlots[i];
                continue;
            }
            else if (Glove.cellmonsters[i] != null && Glove.cellmonsters[i].id != 0) //reroll
            {
                ElementWrapper wrappertouse = NumberPool[UnityEngine.Random.Range(0, NumberPool.Count)];
                wrapper[i] = new ElementWrapper(wrappertouse.number,wrappertouse.element,wrappertouse.fake);
                continue;
            }
            else
            {
                wrapper[i] = new ElementWrapper(0, Element.None, false);
            }
        }
        CurrentSlots = wrapper;

        OnSlotsChanged?.Invoke();
    }

    private void EditCurrentSlotNumber(int slot, int newnumber)
    {
        int old = CurrentSlots[slot].number;
        CurrentSlots[slot].number = newnumber;
        slotsactivated[slot] = CompareHelper.ElementMatchesInt(CurrentSlots[slot].number, glove.cellmonsters[slot].element);
        Debug.Log($"Slot{slot} changed from {old} to {CurrentSlots[slot].number}");
    }

    public int RerollRandomSlot()
    {
        List<int> eligibleSlots = new List<int>();

        for (int i = 0; i < SlotsActivated.Length; i++)
        {
            if (SlotsActivated[i] &&
                Glove.cellmonsters[i] != null &&
                Glove.cellmonsters[i].id != 0 &&
                (SlotsAltered == null || SlotsAltered.Length <= i || SlotsAltered[i] == Element.None))
            {
                eligibleSlots.Add(i);
            }
        }

        if (eligibleSlots.Count == 0)
        {
            Debug.Log("No eligible slots to reroll");
            return -1;
        }

        int slotToReroll = eligibleSlots[UnityEngine.Random.Range(0, eligibleSlots.Count)];

        ElementWrapper newWrapper = NumberPool[UnityEngine.Random.Range(0, NumberPool.Count)];
        EditCurrentSlotNumber(slotToReroll, newWrapper.number);

        return slotToReroll;
    }

    private void HandleOnBattleEnd()
    {
        ChargedMonster = new Monster[5];
        Confirmed = false;
        DamagetoDeal = 0;

        for(int i=0;i<CurrentSlots.Length;i++)
        {
            CurrentSlots[i].number = 1;
        }
    }

}

[System.Serializable]
public class ElementWrapper
{
    public int number;
    public Element element;
    public bool fake;

    public ElementWrapper(int number, Element element, bool fake)
    {
        this.number = number;
        this.element = element;
        this.fake = fake;
    }
}

