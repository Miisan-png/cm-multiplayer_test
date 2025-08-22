using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TestBattleManager : MonoBehaviour
{
    private static TestBattleManager instance;
    public static TestBattleManager Instance => instance;

    public void Awake()
    {
        instance = this;
    }

    [SerializeField] private BattleTester tester1; // Holds a glove and a controller so that i can test start a battle
    [SerializeField] private BattleTester tester2;

    [SerializeField] private bool UsingUI;
    [SerializeField] private GameObject ExitBtn;
    [SerializeField] private GameObject BattleUI;
    [SerializeField] private GameObject SelectUI;

    [SerializeField] private TextMeshProUGUI P1Stats;
    [SerializeField] private TextMeshProUGUI P2Stats;

    [SerializeField] private List<TextMeshProUGUI> P1Cells;
    [SerializeField] private List<GameObject> P1CellsH;
    [SerializeField] private List<TextMeshProUGUI> P2Cells;
    [SerializeField] private List<GameObject> P2CellsH;

    [SerializeField] private battleState statetotrack => BattleManager.Instance.state;
    [SerializeField] private TextMeshProUGUI BattleStateText;

    [SerializeField] private Queue<KeyValuePair<string, float>> BattleMessages = new Queue<KeyValuePair<string, float>>();
    [SerializeField] private TextMeshProUGUI TextBox;
    private Coroutine textcoroutine;

    [SerializeField] private int Timer =>  BattleManager.Instance.state == battleState.Select ? (int)BattleManager.Instance.selecttimer+1 : (int)BattleManager.Instance.rolltimer + 1;
    [SerializeField] private TextMeshProUGUI TimerText;

    [SerializeField] private List<TextMeshProUGUI> P1Slots;
    [SerializeField] private List<TextMeshProUGUI> P2Slots;

    [SerializeField] private TextMeshProUGUI P1DamageStacks;
    [SerializeField] private TextMeshProUGUI P2DamageStacks;

    [SerializeField] private GameObject P1Confirm;
    [SerializeField] private GameObject P2Confirm;

    [SerializeField] private TextMeshProUGUI P1SelectDecision;
    [SerializeField] private TextMeshProUGUI P2SelectDecision;

    [SerializeField] private TextMeshProUGUI P1DefendCooldown;
    [SerializeField] private TextMeshProUGUI P2DefendCooldown;

    [SerializeField] private TextMeshProUGUI P1MoveFirst;
    [SerializeField] private TextMeshProUGUI P2MoveFirst;

    [SerializeField] private GameObject P1SelectActionUI;
    private void Start()
    {
        if (!UsingUI) return;

        BattleManager.Instance.PlayerSelectAction += (int playermoved,selectAction ActionPerformed) => {
            AddtoTextBox($"Player{playermoved}'s monster used {ActionPerformed}!", 2f);
        };
        BattleManager.Instance.MonsterDamaged += (int monthattookdamage,int dmg) => { 
            UpdatePlayerStats();
            AddtoTextBox($"Player{monthattookdamage}'s monster took {dmg} damage.", 2.5f);
        };
        BattleManager.Instance.OnBattleStart += () => { ResetConfirm(); UpdatePlayerStats(); UpdateCells(); AddtoTextBox($"Battle Start! {BattleManager.Instance.GetEquippedMonster(1).name} VS {BattleManager.Instance.GetEquippedMonster(2).name} ", 2f); };

        BattleManager.Instance.OnPhaseChange += (battleState state) =>
        {
            switch (state)
            {
                case battleState.None:

                    break;
                case battleState.End:

                    break;
                case battleState.Start:
                    TimerText.gameObject.SetActive(false);
                    P1SelectActionUI.SetActive(false);
                    ResetSlots();
                    UpdateSelectAction();
                    break;
                case battleState.Roll:
                    ResetConfirm();
                    AddtoTextBox($"Entered Roll Phase!", 1.5f);
                    TimerText.gameObject.SetActive(true);

                    break;
                case battleState.Execution:
                    AddtoTextBox($"Entered Execution Phase!", 1.5f);
                    UpdateCells();
                    UpdateSlotNumbers();
                    ResetConfirm();
                    P1SelectActionUI.SetActive(false);
                    TimerText.gameObject.SetActive(false);
                    break;
                case battleState.Dialogue:

                    break;
                case battleState.Select:
                    ResetConfirm();
                    UpdateSlotNumbers();
                    AddtoTextBox($"Entered Select Phase!", 1.5f);
                    TimerText.gameObject.SetActive(true);
                    P1SelectActionUI.SetActive(true);
                    break;
            }
        };

        BattleManager.Instance.MonsterDied += (int mon) => {
            int winner = mon == 1 ? 2 : 1;
            AddtoTextBox($"Player{mon}'s monster died!,Player{winner} Won!", 3f);
        };


        tester1.controller.OnConfirmed += () => { HandleonConfirmMove(1); };
        tester2.controller.OnConfirmed += () => { HandleonConfirmMove(2); };

        tester1.controller.OnSlotsChanged += UpdateSlotNumbers;
        tester2.controller.OnSlotsChanged += UpdateSlotNumbers;

        tester1.controller.OnCellCharged += UpdateCells;
        tester2.controller.OnCellCharged += UpdateCells;

        tester1.controller.ChargedMonsterReset += UpdateCells;
        tester2.controller.ChargedMonsterReset += UpdateCells;

        tester1.controller.OnSelectActionChanged += UpdateSelectAction;
        tester2.controller.OnSelectActionChanged += UpdateSelectAction;
    }

    private void Update()
    {
        if (!UsingUI) return;

        TimerText.text = Timer.ToString();
        BattleStateText.text = $"BattleState: {statetotrack}";

    }


    private void HandleonConfirmMove(int player)
    {
        if(player == 1)
        {
            P1Confirm.SetActive(true);
        }
        else
        {
            P2Confirm.SetActive(true);
        }
    }

    private void ResetConfirm()
    {
        P1Confirm.SetActive(false);
        P2Confirm.SetActive(false);
    }


    public void AddtoTextBox(string message, float messageDuration)
    {
        BattleMessages.Enqueue(new KeyValuePair<string, float>(message, messageDuration));

        // Only start coroutine if this was the first message
        if (textcoroutine == null && BattleMessages.Count == 1)
        {
            textcoroutine = StartCoroutine(HandleBattleMessages());
        }
    }

    public IEnumerator HandleBattleMessages()
    {
        try
        {
            while (BattleMessages.Count > 0)
            {
                var currentMessage = BattleMessages.Peek(); // Get next message without removing
                string messageText = currentMessage.Key;
                float displayTime = currentMessage.Value;

                TextBox.text = messageText;
                float timer = displayTime;

                // Wait for the specified duration
                while (timer > 0)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }

                // Remove the message after displaying
                BattleMessages.Dequeue();
            }

            TextBox.text = ""; // Clear when no messages left
        }
        finally
        {
            textcoroutine = null; // Ensure cleanup
        }
    }

    private void UpdatePlayerStats()
    {
        P1Stats.text = $"PLAYER 1\nHP:{tester1.controller.hp}";
        P2Stats.text = $"PLAYER 2\nHP:{tester2.controller.hp}";
    }    

    private void UpdateSlotNumbers()
    {
        var keys = tester1.controller.effects.Keys.ToList();
        bool P1SlotsHidden = false;
        foreach (var key in keys)
        {
            if (key.ElementID == Element.Electric && key.EffectID == 1 && tester1.controller.effects[key] > 0 && BattleManager.Instance.state == battleState.Roll)
            {
                P1SlotsHidden = true;
            }
        }
        keys = tester2.controller.effects.Keys.ToList();
        bool P2SlotsHidden = false;
        foreach (var key in keys)
        {
            if (key.ElementID == Element.Electric && key.EffectID == 1 && tester2.controller.effects[key] > 0 && BattleManager.Instance.state == battleState.Roll)
            {
                P2SlotsHidden = true;
            }
        }


        for (int i=0;i < 5; i++)
        {
            if(P1SlotsHidden)
            {
                P1Slots[i].text = $"HIDDEN";
                P1Slots[i].color = Color.black;
                continue;
            }
            if(tester1.controller.glove.cellmonsters[i] != null && tester1.controller.glove.cellmonsters[i].id != 0 && tester1.controller.slotsaltered[i] == Element.None) //if not altered
            {
                if (tester1.controller.numberslots[i].fake && BattleManager.Instance.state == battleState.Roll)
                {
                    P1Slots[i].text = $"{CompareHelper.GetNumberfromElement(tester1.controller.glove.cellmonsters[i].element)}";
                    P1Slots[i].color = Color.gray;
                }
                else
                {
                    P1Slots[i].text = $"{tester1.controller.numberslots[i].number}";
                    P1Slots[i].color = tester1.controller.GetMatchedNumber(i) ? Color.white : Color.black;
                }   
            }
            else if(tester1.controller.numberslots[i].number != 0)
            {
                switch (tester1.controller.slotsaltered[i])
                {
                    case Element.None:
                        break;
                    case Element.Heat:
                        break;
                    case Element.Electric:
                        if (BattleManager.Instance.state != battleState.Execution)
                        {
                            P1Slots[i].text = $"HIDDEN";
                            P1Slots[i].color = Color.black;
                        }
                        else
                        {
                            P1Slots[i].text = $"{tester1.controller.numberslots[i].number}";
                            P1Slots[i].color = tester1.controller.GetMatchedNumber(i) ? Color.white : Color.black;
                        }
                            break;
                    case Element.Wind:
                        P1Slots[i].text = $"{tester1.controller.numberslots[i].number}";
                        P1Slots[i].color = Color.green;
                        break;
                    case Element.Solar:
                        if(BattleManager.Instance.state != battleState.Execution)
                        {
                            int RealNumber = tester1.controller.numberslots[i].number;
                            int NumbertoUse = RealNumber;

                            int rng = Random.Range(0, 10);

                            for (int u = 0; u < 10; u++)
                            {
                                if (RealNumber != rng)
                                {
                                    NumbertoUse = rng;
                                }
                                rng = Random.Range(0, 10);
                            }

                            P1Slots[i].text = $"{NumbertoUse}";
                            P1Slots[i].color = Color.grey;
                        }
                        else
                        {

                            P1Slots[i].text = $"{tester1.controller.numberslots[i].number}";
                            P1Slots[i].color = tester1.controller.GetMatchedNumber(i) ? Color.white : Color.black;
                        }
                            break;
                    case Element.Hydro:
                        break;
                    case Element.Sound:
                        break;
                }
            }
            else
            {
                P1Slots[i].text = $"";
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (P2SlotsHidden)
            {
                P2Slots[i].text = $"HIDDEN";
                P2Slots[i].color = Color.black;
                continue;
            }
            if (tester2.controller.glove.cellmonsters[i]!= null && tester2.controller.glove.cellmonsters[i].id != 0 && tester2.controller.slotsaltered[i] == Element.None)
            {
                if (tester2.controller.numberslots[i].fake && BattleManager.Instance.state == battleState.Roll)
                {
                    P2Slots[i].text = $"{CompareHelper.GetNumberfromElement(tester2.controller.glove.cellmonsters[i].element)}";
                    P2Slots[i].color = Color.gray;
                }
                else
                {
                    P2Slots[i].text = $"{tester2.controller.numberslots[i].number}";
                    P2Slots[i].color = tester2.controller.GetMatchedNumber(i) ? Color.white : Color.black;
                }
            }
            else if (tester2.controller.numberslots[i].number != 0)
            {
                switch (tester2.controller.slotsaltered[i])
                {
                    case Element.None:
                        break;
                    case Element.Heat:
                        break;
                    case Element.Electric:
                        if (BattleManager.Instance.state != battleState.Execution)
                        {
                            P2Slots[i].text = $"HIDDEN";
                            P2Slots[i].color = Color.black;
                        }
                        else
                        {
                            P2Slots[i].text = $"{tester2.controller.numberslots[i].number}";
                            P2Slots[i].color = tester2.controller.GetMatchedNumber(i) ? Color.white : Color.black;
                        }
                            break;
                    case Element.Wind:
                        P2Slots[i].text = $"{tester2.controller.numberslots[i].number}";
                        P2Slots[i].color = Color.green;
                        break;
                    case Element.Solar:
                        if(BattleManager.Instance.state != battleState.Execution)
                        {
                            int RealNumber = tester2.controller.numberslots[i].number;
                            int NumbertoUse = RealNumber;

                            int rng = Random.Range(0, 10);

                            for (int u = 0; u < 10; u++)
                            {
                                if (RealNumber != rng)
                                {
                                    NumbertoUse = rng;
                                }
                                rng = Random.Range(0, 10);
                            }

                            P2Slots[i].text = $"{NumbertoUse}";
                            P2Slots[i].color = Color.grey;
                        }
                        else
                        {
                            P2Slots[i].text = $"{tester2.controller.numberslots[i].number}";
                            P2Slots[i].color = tester2.controller.GetMatchedNumber(i) ? Color.white : Color.black;
                        }
                        break;
                    case Element.Hydro:
                        break;
                    case Element.Sound:
                        break;
                }

            }
            else
            {
                P2Slots[i].text = $"";
            }
        }
    }

    private void ResetSlots()
    {
        for(int i=0;i<P1Slots.Count;i++)
        {
            P1Slots[i].text = "";
        }
        for (int i = 0; i < P2Slots.Count; i++)
        {
            P2Slots[i].text = "";
        }
    }

    public void UpdateCells()
    {
        for(int i=0;i<P1Cells.Count;i++)
        {
            P1Cells[i].gameObject.SetActive(false);
        }
        for(int i=0;i<P1CellsH.Count;i++)
        {
            P1CellsH[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < P2Cells.Count; i++)
        {
            P2Cells[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < P2CellsH.Count; i++)
        {
            P2CellsH[i].gameObject.SetActive(false);
        }

        Monster M = tester1.glove.equippedmonster;
        P1Cells[0].text = $"Main: {M.name}\nLvl {M.currentlevel}\nElement: {M.element}\nSkillPower: {M.skillpower}";
        P1Cells[0].gameObject.SetActive(true);
        if (BattleManager.Instance.GetControllerByIndex(1).chargedmonster.Contains(M))
        {
            P1CellsH[0].gameObject.SetActive(true);
        }

        for (int i = 1; i < 5; i++)
        {
            if (tester1.glove.cellmonsters[i] != null && tester1.glove.cellmonsters[i].id != 0)
            {
                M = tester1.glove.cellmonsters[i];
                P1Cells[i].text = $"Cell{i}: {M.name}\nLvl {M.currentlevel}\nElement{M.element}";
                P1Cells[i].gameObject.SetActive(true);
                if (BattleManager.Instance.GetControllerByIndex(1).chargedmonster.Contains(M))
                {
                    P1CellsH[i].gameObject.SetActive(true);
                }
            }
        }

        M = tester2.glove.equippedmonster;
        P2Cells[0].text = $"Main: {M.name}\nLvl {M.currentlevel}\nElement: {M.element}\nSkillPower: {M.skillpower}";
        P2Cells[0].gameObject.SetActive(true);
        if (BattleManager.Instance.GetControllerByIndex(2).chargedmonster.Contains(M))
        {
            P2CellsH[0].gameObject.SetActive(true);
        }

        for (int i = 1; i < 5; i++)
        {
            if (tester2.glove.cellmonsters[i] != null && tester2.glove.cellmonsters[i].id != 0)
            {
                M = tester2.glove.cellmonsters[i];
                P2Cells[i].text = $"Cell{i}: {M.name}\nLvl {M.currentlevel}\nElement{M.element}";
                P2Cells[i].gameObject.SetActive(true);
                if (BattleManager.Instance.GetControllerByIndex(2).chargedmonster.Contains(M))
                {
                    P2CellsH[i].gameObject.SetActive(true);
                }
            }
        }

        string defend = tester1.controller.defendcooldown > 0 ? "P1 Defend On Cooldown!" : "";
        P1DefendCooldown.text = $"{defend}";
        defend = tester2.controller.defendcooldown > 0 ? "P2 Defend On Cooldown!" : "";
        P2DefendCooldown.text = $"{defend}";

        P1MoveFirst.text = "";
        P2MoveFirst.text = "";
        if (BattleManager.Instance.playertomovefirst != 0 && (BattleManager.Instance.state == battleState.Select || BattleManager.Instance.state == battleState.Execution))
        {
            switch (BattleManager.Instance.playertomovefirst)
            {
                case 1:
                    P1MoveFirst.text = "(Move First)";
                    break;
                case 2:
                    P2MoveFirst.text = "(Move First)";
                    break;
            }
        }
    }

    private void UpdateSelectAction()
    {
        bool activate = BattleManager.Instance.state == battleState.Select || BattleManager.Instance.state == battleState.Execution;


        string text = tester1.controller.selectaction == selectAction.None ? "SELECT ACTION: None" : $"SELECT ACTION: {tester1.controller.selectaction}";
        P1SelectDecision.text = $"{text}";
        P1SelectDecision.gameObject.SetActive(activate);
        text = tester2.controller.selectaction == selectAction.None ? "SELECT ACTION: None" : $"SELECT ACTION: {tester2.controller.selectaction}";
        P2SelectDecision.text = $"{text}";
        P2SelectDecision.gameObject.SetActive(activate);
    }

    public void TestBattle()
    {
        if(!BattleManager.Instance.StartNewBattleCondition(tester1.glove, tester2.glove))
        {
            Debug.Log("ONE OR TWO PLAYER HAS NO MONSTER EQUIPPED");
            return;
        }
        BattleManager.Instance.StartBattle(tester1.glove, tester2.glove, battleType.NPC,tester2.NPCData);

        ExitBtn.SetActive(true);
        SelectUI.SetActive(false);

        if (!UsingUI) return;
        BattleUI.SetActive(true);
    }

    public void EndCurrentBattle()
    {
        if(BattleManager.Instance.state != battleState.None)
        {
            BattleManager.Instance.EndBattle();
        }


        SelectUI.SetActive(true);

        if (!UsingUI) return;
        BattleUI.SetActive(false);
    }
}
