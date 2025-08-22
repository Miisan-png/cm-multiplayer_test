using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private static BattleManager instance;
    public static BattleManager Instance => instance;

    public void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        setState(battleState.None);
    }

    public Action OnBattleStart;
    public Action OnBattleStartCutscene;
    public Action OnBattleStartCutsceneEnd;
    public Action OnBattleStartCutsceneSkipped;
    public Action OnBattleEnd;
    public Action OnBattleExit;
    public Action OnRollConfirm;
    public Action<battleState> OnPhaseChange;
    public Action OnRollTimerStart;
    public Action OnRollEnd;
    public Action OnSelecEnd;
    public Action OnExecuteEnd;
    public Action<int, selectAction> PlayerSelectAction; 
    public Action<int, int> MonsterDamaged;
    public Action<int> MonsterDied;
    public Action OnCapturePrompt;
    public Action<int[]> OnRewardScreen;

    [SerializeField] private battleState State;
    public battleState state => State;

    [SerializeField] private battleType BattleType;
    public battleType battleType => BattleType;
    [SerializeField] private NPC NPCinBattle;
    public NPC npcinbattle => NPCinBattle;

    [SerializeField] private BattleGlove P1Glove;
    [SerializeField] private BattleController P1Controller;
    public BattleController p1controller => P1Controller;

    [SerializeField] private BattleGlove P2Glove;
    [SerializeField] private BattleController P2Controller;
    public BattleController p2controller => P2Controller;
    [SerializeField] private int PlayerSelectConfirm;
    [SerializeField] private float CurrentSelectTimer;
    public float selecttimer => CurrentSelectTimer;
    [SerializeField] private float SelectDuration;
    private Coroutine selectcoroutine;

    [SerializeField] private int PlayertoMoveFirst;
    public int playertomovefirst => PlayertoMoveFirst;
    [SerializeField] private List<BattleController> PlayersMoved;
    private Coroutine executionMoveCoroutine;

    [SerializeField] private int PlayerWon;

    public int playerwon => PlayerWon;

    [SerializeField] private float CurrentRollTimer;
    public float rolltimer => CurrentRollTimer;
    [SerializeField] private float RollDuration;
    [SerializeField] private float RollEndDuration = 2f;
    [SerializeField] private float RollEndTimer;
    private bool RollEndDelayed;
    public bool rollenddelay => RollEndDelayed;
    private bool CanRoll;
    public bool canroll => CanRoll;
    private bool StartingCutsceneSkipped;
    public bool startingcutsceneskipped => StartingCutsceneSkipped;

    private Coroutine StartDelayCoroutine;

    private Coroutine rollcoroutine; //track select timer

    private Coroutine P1RerollCoroutine;
    private Coroutine P2RerollCoroutine;

    private Coroutine RollEndCoroutine;
    private Coroutine SelectEndCoroutine;

    private void setState(battleState S)
    {
        if (State == S) return; // Prevent duplicate state changes
        State = S;
        OnPhaseChange?.Invoke(State);
    }

    public void SkipNextState()
    {
        switch (State)
        {
            case battleState.None: break;
            case battleState.End: break;
            case battleState.Start:
                setState(battleState.Roll);
                StartRollPhase();
                break;
            case battleState.Roll:
                setState(battleState.Select);
                StartSelectPhase();
                break;
            case battleState.Select:
                setState(battleState.Execution);
                StartExecutionPhase();
                break;
            case battleState.Execution:
                setState(battleState.Roll);
                StartRollPhase();
                break;
            case battleState.Dialogue:
                setState(battleState.Roll);
                StartRollPhase();
                break;

        }
    }

    public bool StartNewBattleCondition(BattleGlove p1glove, BattleGlove p2glove)
    {
        return State == battleState.None && p1glove.equippedmonster.id != 0 && p2glove.equippedmonster.id != 0;
    }

    public void StartBattle(BattleGlove p1glove, BattleGlove p2glove,battleType battletype)
    {
        if (!StartNewBattleCondition(p1glove,p2glove)) return;

        P1Glove = p1glove;
        P2Glove = p2glove;

        SelectEndCoroutine = null;
        RollEndCoroutine = null;

        BattleType = battletype;

        switch (BattleType)
        {
            case battleType.Wild:
                P1Controller = BattleControllerManager.Instance.playercontroller;
                P2Controller = BattleControllerManager.Instance.npccontroller1; //AssignControllers based on type of battle
                break;
            case battleType.NPC:
                P1Controller = BattleControllerManager.Instance.playercontroller;
                P2Controller = BattleControllerManager.Instance.npccontroller1;
                break;
        }

        P1Controller.InitializeController(P1Glove);
        P2Controller.InitializeController(P2Glove);

        setState(battleState.Start);

        OnBattleStart?.Invoke();
        StartingCutsceneSkipped = false;

        if (StartDelayCoroutine != null)
        {
            StopCoroutine(StartDelayCoroutine);
        }

        StartDelayCoroutine = StartCoroutine(StartDelay());

        PlayerSelectConfirm = 0;
        PlayerWon = 0;
        RollEndTimer = RollEndDuration;
    }
    public void StartBattle(BattleGlove p1glove, BattleGlove p2glove, battleType battletype,NPC npc)
    {
        if (!StartNewBattleCondition(p1glove, p2glove)) return;

        P1Glove = p1glove;
        P2Glove = p2glove;

        SelectEndCoroutine = null;
        RollEndCoroutine = null;

        NPCinBattle = npc;
        BattleType = battletype;

        switch (BattleType)
        {
            case battleType.Wild:
                P1Controller = BattleControllerManager.Instance.playercontroller;
                P2Controller = BattleControllerManager.Instance.npccontroller1; //AssignControllers based on type of battle
                break;
            case battleType.NPC:
                P1Controller = BattleControllerManager.Instance.playercontroller;
                P2Controller = BattleControllerManager.Instance.npccontroller1;
                break;
        }

        P1Controller.InitializeController(P1Glove);
        P2Controller.InitializeController(P2Glove);

        setState(battleState.Start);

        OnBattleStart?.Invoke();
        StartingCutsceneSkipped = false;

        if (StartDelayCoroutine != null)
        {
            StopCoroutine(StartDelayCoroutine);
        }

        StartDelayCoroutine = StartCoroutine(StartDelay());

        PlayerSelectConfirm = 0;
        PlayerWon = 0;
        RollEndTimer = RollEndDuration;
    }

    public void SkipStartingCutscene()
    {
        if(state == battleState.Start && !startingcutsceneskipped)
        {
            StartingCutsceneSkipped = true;

            if (StartDelayCoroutine != null)
            {
                StopCoroutine(StartDelayCoroutine);
            }

            OnBattleStartCutsceneSkipped?.Invoke();
            OnBattleStartCutsceneEnd?.Invoke();

            StartDelayCoroutine = StartCoroutine(SkippedDelay());
        }
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(3.5f);
        OnBattleStartCutscene?.Invoke();
        yield return new WaitForSeconds(7f);
        OnBattleStartCutsceneEnd?.Invoke();
        StartingCutsceneSkipped = true;
        yield return new WaitForSeconds(4f);
        SkipNextState();
    }

    private IEnumerator SkippedDelay()
    {
        yield return new WaitForSeconds(4f);
        SkipNextState();
    }

    #region RollPhase
    private void StartRollPhase()
    {
        CanRoll = false;
        PlayertoMoveFirst = 0;
        PlayersMoved.Clear();
        if (rollcoroutine != null) StopCoroutine(rollcoroutine);
        rollcoroutine = StartCoroutine(RollTimer());
    }

    private IEnumerator RollTimer()
    {
        yield return new WaitForSeconds(1f);

        OnRollTimerStart?.Invoke();

        CanRoll = true;
        P1RerollCoroutine = StartCoroutine(RerollSequence(1));
        P2RerollCoroutine = StartCoroutine(RerollSequence(2));

        CurrentRollTimer = RollDuration;
        while (CurrentRollTimer > 0)
        {
            CurrentRollTimer -= Time.fixedDeltaTime;

            if(CurrentRollTimer < 0)
            {
                CurrentRollTimer = 0;
            }

            yield return new WaitForFixedUpdate();
        }
        EndRollPhase();
    }

    public void ConfirmMove(int player)
    {
        SkipRollTimer(GetControllerByIndex(player).skiptotime); //Tries to skip, if timer already lower than 5 then it wont skip again

        if(player ==1)
        {
            if(P1RerollCoroutine != null)
            {
                StopCoroutine(P1RerollCoroutine);
            }
        }
        else if(player ==2)
        {
            if (P2RerollCoroutine != null)
            {
                StopCoroutine(P2RerollCoroutine);
            }
        }

        if (PlayertoMoveFirst == 0)
        {
            PlayertoMoveFirst = player;
            OnRollConfirm?.Invoke();
        }
        else
        {
            EndRollPhase();
        }
    }

    public void SkipRollTimer(float timetoskipto)
    {
        if (CurrentRollTimer > timetoskipto) CurrentRollTimer = timetoskipto;
    }

    private IEnumerator RerollSequence(int player)
    {
        BattleController controller = player == 1 ? P1Controller : P2Controller;
        while (State == battleState.Roll)
        {
            controller.RerollNumberSlots();
            yield return new WaitForSeconds(controller.rerolltimer);
        }
    }

    private void EndRollPhase()
    {
        if (rollcoroutine != null)
        {
            StopCoroutine(rollcoroutine);
        }
        StopCoroutine(P1RerollCoroutine);
        StopCoroutine(P2RerollCoroutine);

        if(RollEndCoroutine == null)
        {
            RollEndCoroutine = StartCoroutine(rollEndDelay());
        }
    }

    public void DelayRollEnd()
    {
        if (RollEndDelayed || state != battleState.Roll) return;

        RollEndTimer = 6f;
        RollEndDelayed = true;

        Debug.Log("RollEndDelayed");
    }

    private IEnumerator rollEndDelay()
    {
        OnRollEnd?.Invoke();

        RollEndTimer = RollEndDelayed ? 6f : RollEndDuration;

        while(RollEndTimer > 0)
        {
            RollEndTimer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        SkipNextState();

        RollEndTimer = RollEndDuration;
        RollEndDelayed = false;
        RollEndCoroutine = null;
    }

    #endregion

    #region SelectPhase
    private void StartSelectPhase()
    {
        if(selectcoroutine != null)
        {
            StopCoroutine(selectcoroutine);
        }
        selectcoroutine = StartCoroutine(SelectTimer());
    }

    private IEnumerator SelectTimer()
    {
        CurrentSelectTimer = SelectDuration;

        if (CurrentSelectTimer <= 0) //To disable selecttimer, set select duration to -1
        {
            yield break;
        }

        while (CurrentSelectTimer > 0)
        {
            CurrentSelectTimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        EndSelectPhase();
    }

    public void ConfirmSelect()
    {
        if (State != battleState.Select) return;
        PlayerSelectConfirm++;
        if(PlayerSelectConfirm >= 2)
        {
            EndSelectPhase();
            PlayerSelectConfirm = 0;
        }
    }

    private void EndSelectPhase()
    {
        if (State != battleState.Select) return;
        if (selectcoroutine != null)
        {
            StopCoroutine(selectcoroutine);
        }
        if(SelectEndCoroutine == null)
        {
            SelectEndCoroutine = StartCoroutine(selectEndDelay());
        }
    }
    private IEnumerator selectEndDelay()
    {
        yield return new WaitForSeconds(0.7f);
        OnSelecEnd?.Invoke();
        SkipNextState();
        SelectEndCoroutine = null;
    }

    #endregion

    #region ExecutionPhase
    private void StartExecutionPhase()
    {
        if (PlayertoMoveFirst == 0)
        {
            PlayertoMoveFirst = UnityEngine.Random.Range(1, 3);
            Debug.Log($"No player confirmed,Player{PlayertoMoveFirst} Move First");
        }

        BattleController self = PlayertoMoveFirst == 1 ? P1Controller : P2Controller;

        HandlePlayerExecution(PlayertoMoveFirst, self.selectaction);
    }

    private void HandlePlayerExecution(int player,selectAction move)
    {
        BattleController self = player == 1 ? P1Controller : P2Controller;

        switch (move)
        {
            case selectAction.Skip:
                executionMoveCoroutine = StartCoroutine(SkipAnimation(self));
                break;
            case selectAction.Attack:
                executionMoveCoroutine = StartCoroutine(AttackAnimation(player, 1.5f));
                break;
            case selectAction.Defend:
                executionMoveCoroutine = StartCoroutine(DefendAnimation(self));
                break;
        }

        PlayerSelectAction?.Invoke(player, move);
    }



    private IEnumerator SkipAnimation(BattleController self)
    {
        yield return new WaitForSeconds(1f);
        PlayersMoved.Add(self);
        StartCoroutine(WaittoSkipPhase());
    }

    private IEnumerator DefendAnimation(BattleController self)
    {
        yield return new WaitForSeconds(1f);
        PlayersMoved.Add(self);
        StartCoroutine(WaittoSkipPhase());
    }

    private IEnumerator AttackAnimation(int attacker, float _timer)
    {
        Animator animator = BattleObjectsManager.Instance.GetAnimatorbyIndex(attacker);

        if (animator != null)
        {
            yield return new WaitForSeconds(1f); // Initial delay (optional)

            animator.SetTrigger("Attack01");

            // Wait until "Attack01" is the current state (with timeout)
            float timeout = Time.time + 0.5f; // Max 0.5s wait (adjust as needed)
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01"))
            {
                if (Time.time > timeout)
                {
                    Debug.LogWarning("Timeout waiting for Attack01 state!");
                    break;
                }
                yield return null; // Wait another frame
            }

            // Now check if we're in the correct state
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01"))
            {
                Debug.Log("Attack01 is now playing!");

                // Wait until the animation finishes
                yield return new WaitUntil(() =>
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f
                );
            }
            else
            {
                Debug.LogWarning("Attack01 failed to start, falling back to timer.");
                yield return new WaitForSeconds(_timer);
            }
        }
        else
        {
            yield return new WaitForSeconds(_timer);
        }

        int target = attacker == 1 ? 2 : 1;
        StartAttackDelay(target);
    }

    private void StartAttackDelay(int target)
    {
        int totaldamage = target == 1 ? P2Controller.damagetodeal : P1Controller.damagetodeal;
        StartCoroutine(DelaybeforeRealAttack(target, totaldamage));
    }

    private IEnumerator DelaybeforeRealAttack(int target, int damage)
    {
        BattleController self = target == 1 ? P2Controller : P1Controller;

        if (GetControllerByIndex(target).selectaction != selectAction.Defend)
        {
            GetControllerByIndex(target).TakeDamage(damage);    
            MonsterDamaged?.Invoke(target, damage);
        }
        else //ignore damage if defended
        {
            MonsterDamaged?.Invoke(target, 0);
        }

        PlayersMoved.Add(self);

        yield return new WaitForSeconds(1f);

        if (GetControllerByIndex(target).hp <= 0)
        {
            StartCoroutine(MonsterDeathDelay(() => {
                Debug.Log($"PLayer{self.playerindex} Wins, ending battle");
                OnMonsterDeath(target);
                EndBattle();
            }));
        }
        else
        {
            StartCoroutine(WaittoSkipPhase());
        }
    }

    private IEnumerator MonsterDeathDelay(Action A)
    {
        yield return new WaitForSeconds(2f);
        A();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            OnMonsterDeath(2);
            EndBattle();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            OnMonsterDeath(1);
            EndBattle();
        }
    }
    private IEnumerator WaittoSkipPhase()
    {
        yield return new WaitForSeconds(1f);

        // Only proceed if we're still in Execution phase
        if (state != battleState.Execution) yield break;

        if (PlayersMoved.Count > 1)
        {
            OnExecuteEnd?.Invoke();
            yield return new WaitForSeconds(1f);
            SkipNextState();
        }
        else
        {
            int nexttomove = PlayersMoved[0].playerindex == 1 ? 2 : 1;
            if (executionMoveCoroutine != null) StopCoroutine(executionMoveCoroutine); //Stops all attacking if applicable
            HandlePlayerExecution(nexttomove, GetControllerByIndex(nexttomove).selectaction);
        }
    }

    private int[] GiveRewardstoWinner()
    {
        BattleController controller = GetControllerByIndex(PlayerWon);
        int[] exprewards = new int[5];
        BattleController target = GetControllerByIndex(PlayerWon == 1? 2:1);

        if (controller is not BattleController_Player) return null;

        int expmultiplier = 1;
        int totalexp = 0;

        for (int i=0;i< target.glove.cellmonsters.Length; i++)
        {
            if (target.glove.cellmonsters[i]!= null && target.glove.cellmonsters[i].id != 0)
            {
                switch (target.glove.cellmonsters[i].rarity)
                {
                    case MonsterRarity.common:
                        totalexp += 10;
                        totalexp += 1 * target.glove.cellmonsters[i].currentlevel;
                        break;
                    case MonsterRarity.uncommon:
                        totalexp += 20;
                        totalexp += 2 * target.glove.cellmonsters[i].currentlevel;
                        break;
                    case MonsterRarity.rare:
                        totalexp += 40;
                        totalexp += 4 * target.glove.cellmonsters[i].currentlevel;
                        break;
                    case MonsterRarity.super_rare:
                        totalexp += 80;
                        totalexp += 8 * target.glove.cellmonsters[i].currentlevel;
                        break;
                    case MonsterRarity.mystic:
                        totalexp += 160;
                        totalexp += 16 * target.glove.cellmonsters[i].currentlevel;
                        break;
                }
            }
        }

        for (int i=0;i<controller.glove.cellmonsters.Length;i++)
        {
            if (controller.glove.cellmonsters[i] != null && controller.glove.cellmonsters[i].id !=0)
            {
                controller.glove.cellmonsters[i].GainEXP(totalexp * expmultiplier);
                exprewards[i] = totalexp * expmultiplier;
            }
        }

        return exprewards;
    }

    public Monster GetEquippedMonster(int player)
    {
        return player == 1 ? P1Glove.equippedmonster : P2Glove.equippedmonster;
    }

    public BattleGlove GetGlove(BattleGlove glove)
    {
        return glove == P1Glove ? P1Glove : P2Glove;
    }

    public BattleGlove GetGlovebyIndex(int player)
    {
        return player == 1 ? P1Glove : P2Glove;
    }

    public BattleController GetController(BattleController _controller)
    {
        return P1Controller == _controller ? p1controller : p2controller;
    }

    public BattleController GetControllerByIndex(int player)
    {
        return player == 1 ? p1controller : p2controller;
    }

    #endregion

    #region BattleEnd
    public void OnMonsterDeath(int player)
    {
        setState(battleState.End);
        MonsterDied?.Invoke(player);
        PlayerWon = player == 1 ? 2 : 1;
    }

    public void EndBattle()
    {
        OnBattleEnd?.Invoke();
        StopAllCoroutines();

        StartCoroutine(BattleEndDelay());
        Debug.Log("BattleEnd");
    }

    private IEnumerator BattleEndDelay()
    {
        yield return new WaitForSeconds(5f);

        //if wild monster battle, skip to capture screen then reward screen
        //if NPC battle, skip to reward screen

        if (PlayerWon != 1) yield break;

        switch (BattleType)
        {
            case battleType.Wild:
                OnCapturePrompt?.Invoke();
                break;
            case battleType.NPC:
                EnterRewardScreen(0);
                break;
        }
    }

    public void EnterRewardScreen(float delay)
    {
        StartCoroutine(RewardScreenDelay(delay));
    }

    private IEnumerator RewardScreenDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnRewardScreen?.Invoke(GiveRewardstoWinner());
        Debug.Log("EnterRewardScreen!");
    }

    public void ExitBattle(bool UnloadScene)
    {
        OnBattleExit?.Invoke();

        setState(battleState.None);

        if (UnloadScene)
        {
            GameManager.Instance.UnLoadBattleScene(playerwon == 1);
        }
    }
    #endregion
}

public enum selectAction
{
   None, Skip, Attack, Defend
}

public enum battleState
{
    Start, Select, Roll, Execution, End, Dialogue, None
}

public enum battleType
{
    Wild,NPC
}