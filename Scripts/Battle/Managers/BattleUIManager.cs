using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    private static BattleUIManager instance;
    public static BattleUIManager Instance => instance;

    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _slotSprites = new Dictionary<int, Sprite>()
        {
        {0, slot0sprite},
        {1, slot1sprite},
        {2, slot2sprite},
        {3, slot3sprite},
        {4, slot4sprite},
        {5, slot5sprite},
        {6, slot6sprite},
        {7, slot7sprite},
        {8, slot8sprite},
        {9, slot9sprite},
        {10,slot10sprite}
        };

        _bottomSlotSprites = new Dictionary<int, Sprite>()
        {
        {0, btmSlot0Sprite},
        {1, btmSlot1Sprite},
        {2, btmSlot2Sprite},
        {3, btmSlot3Sprite},
        {4, btmSlot4Sprite},
        {5, btmSlot5Sprite},
        {6, btmSlot6Sprite},
        {7, btmSlot7Sprite},
        {8, btmSlot8Sprite},
        {9, btmSlot9Sprite},
        {10, btmSlot10Sprite}
        };
        _effectSprites = new Dictionary<Element, Sprite>()
        {
            {Element.Heat,fireeffectsprite},
            {Element.Hydro,watereffectsprite},
            {Element.Electric,electriceffectsprite},
            {Element.Solar,lighteffectsprite},
            {Element.Sound,soundeffectsprite},
            {Element.Wind,windeffectsprite},
        };


        BattleManager.Instance.OnBattleStart += OnBattleStart;
        BattleManager.Instance.OnBattleStartCutsceneEnd += OnStartCutsceneEnd;
        BattleManager.Instance.OnPhaseChange += OnPhaseChange;
        BattleManager.Instance.OnRollConfirm += OnRollConfirm;
        BattleManager.Instance.OnBattleEnd += OnBattleEnd;
        BattleManager.Instance.OnBattleExit += OnBattleExit;
        BattleManager.Instance.OnRollTimerStart += OnRollTimerStart;
        BattleManager.Instance.OnRewardScreen += OnRewardsScreen;
        BattleInputManager.Instance.OnPause += SkipEXPAnimation;
    }
    [SerializeField] private Sprite BtmSlot0Sprite;
    public Sprite btmSlot0Sprite => BtmSlot0Sprite;

    [SerializeField] private Sprite BtmSlot1Sprite;
    public Sprite btmSlot1Sprite => BtmSlot1Sprite;

    [SerializeField] private Sprite BtmSlot2Sprite;
    public Sprite btmSlot2Sprite => BtmSlot2Sprite;

    [SerializeField] private Sprite BtmSlot3Sprite;
    public Sprite btmSlot3Sprite => BtmSlot3Sprite;

    [SerializeField] private Sprite BtmSlot4Sprite;
    public Sprite btmSlot4Sprite => BtmSlot4Sprite;

    [SerializeField] private Sprite BtmSlot5Sprite;
    public Sprite btmSlot5Sprite => BtmSlot5Sprite;

    [SerializeField] private Sprite BtmSlot6Sprite;
    public Sprite btmSlot6Sprite => BtmSlot6Sprite;

    [SerializeField] private Sprite BtmSlot7Sprite;
    public Sprite btmSlot7Sprite => BtmSlot7Sprite;

    [SerializeField] private Sprite BtmSlot8Sprite;
    public Sprite btmSlot8Sprite => BtmSlot8Sprite;

    [SerializeField] private Sprite BtmSlot9Sprite;
    public Sprite btmSlot9Sprite => BtmSlot9Sprite;

    [SerializeField] private Sprite BtmSlot10Sprite; //emptyslot
    public Sprite btmSlot10Sprite => BtmSlot10Sprite;

    private Dictionary<int, Sprite> _bottomSlotSprites;
    public Sprite GetBottomSlotSpritebyNumber(int ID)
    {
        return _bottomSlotSprites.TryGetValue(ID, out var sprite) ? sprite : BtmSlot0Sprite;
    }

    [SerializeField] private Sprite Slot0Sprite;
    public Sprite slot0sprite => Slot0Sprite;

    [SerializeField] private Sprite Slot1Sprite;
    public Sprite slot1sprite => Slot1Sprite;

    [SerializeField] private Sprite Slot2Sprite;
    public Sprite slot2sprite => Slot2Sprite;

    [SerializeField] private Sprite Slot3Sprite;
    public Sprite slot3sprite => Slot3Sprite;

    [SerializeField] private Sprite Slot4Sprite;
    public Sprite slot4sprite => Slot4Sprite;

    [SerializeField] private Sprite Slot5Sprite;
    public Sprite slot5sprite => Slot5Sprite;

    [SerializeField] private Sprite Slot6Sprite;
    public Sprite slot6sprite => Slot6Sprite;

    [SerializeField] private Sprite Slot7Sprite;
    public Sprite slot7sprite => Slot7Sprite;

    [SerializeField] private Sprite Slot8Sprite;
    public Sprite slot8sprite => Slot8Sprite;

    [SerializeField] private Sprite Slot9Sprite;
    public Sprite slot9sprite => Slot9Sprite;

    [SerializeField] private Sprite Slot10Sprite; //emptyslot
    public Sprite slot10sprite => Slot10Sprite;

    private Dictionary<int, Sprite> _slotSprites;
    public Sprite GetSlotSpritebyNumber(int ID)
    {
        return _slotSprites.TryGetValue(ID, out var sprite) ? sprite : Slot0Sprite;
    }

    [SerializeField] private Sprite FireEffectSprite;
    public Sprite fireeffectsprite => FireEffectSprite;

    [SerializeField] private Sprite WaterEffectSprite;
    public Sprite watereffectsprite => WaterEffectSprite;

    [SerializeField] private Sprite WindEffectSprite;
    public Sprite windeffectsprite => WindEffectSprite;

    [SerializeField] private Sprite LightEffectSprite;
    public Sprite lighteffectsprite => LightEffectSprite;

    [SerializeField] private Sprite SoundEffectSprite;
    public Sprite soundeffectsprite => SoundEffectSprite;

    [SerializeField] private Sprite ElectricEffectSprite;
    public Sprite electriceffectsprite => ElectricEffectSprite;

    private Dictionary<Element, Sprite> _effectSprites;

    public Sprite GetEffectSpriteByElement(Element E)
    {
        return _effectSprites.TryGetValue(E, out var sprite) ? sprite : fireeffectsprite;
    }


    [SerializeField] private Color SelectedColor;
    public Color selectedcolor => SelectedColor;
    [SerializeField] private Color DeselectedColor;
    public Color deselectedcolor => DeselectedColor;

    [SerializeField] private GameObject BattleMainUI;
    [SerializeField] private GameObject BattleEndUI;
    [SerializeField] private GameObject CaptureUI;
    [SerializeField] private GameObject RewardsScreenUI;

    [SerializeField] private BattleUIController P1Controller;
    public BattleUIController p1controller => P1Controller;
    [SerializeField] private BattleUIController P2Controller;
    public BattleUIController p2controller => P2Controller;

    public BattleUIController GetControllerbyID(int index)
    {
        return index == 1 ? p1controller : p2controller;
    }

    [SerializeField] private GameObject MoveOrder;
    [SerializeField] private TextMeshProUGUI RollCountdown;

    [SerializeField] private TextMeshProUGUI P1FirstText;
    [SerializeField] private TextMeshProUGUI P1SecondText;
    [SerializeField] private TextMeshProUGUI P2FirstText;
    [SerializeField] private TextMeshProUGUI P2SecondText;
    [SerializeField] private TextMeshProUGUI MonsterAttackText;

    [SerializeField] private TextMeshProUGUI WinnerText;
    [SerializeField] private List<TextMeshProUGUI> EXPText;
    [SerializeField] private List<TextMeshProUGUI> EXPGetText;
    [SerializeField] private List<int> CachedEXP;
    [SerializeField] private List<int> CachedLevel;
    [SerializeField] private int[] _EXPGet;
    private Coroutine EXPCoroutine;

    [SerializeField] private Button CaptureYesBtn;
    [SerializeField] private Button CaptureNoBtn;
    [SerializeField] private Button ExitBattleBtn;
    [SerializeField] private Button ExitBattleBtnLose;
    [SerializeField] private Animator CaptureAnimator;

    private void OnBattleStart()
    {
        RewardsScreenUI.SetActive(false);
        BattleEndUI.SetActive(false);
        MoveOrder.SetActive(false);
        P1Controller.InitializeUI(BattleManager.Instance.GetControllerByIndex(1));
        P2Controller.InitializeUI(BattleManager.Instance.GetControllerByIndex(2));
    }

    private void OnStartCutsceneEnd()
    {
        BattleMainUI.SetActive(true);
        P1Controller.UpdateHealthUI();
        P2Controller.UpdateHealthUI();
    }

    private void OnPhaseChange(battleState S)
    {
        switch (S)
        {
            case battleState.Start:
                RollCountdown.gameObject.SetActive(false);
                break;
            case battleState.Select:
                RollCountdown.gameObject.SetActive(false);
                break;
            case battleState.Roll:
                MoveOrder.SetActive(true);
                ResetMoveOrder();
                break;
            case battleState.Execution:
                MoveOrder.SetActive(false);
                DeactivateAttackName();
                break;
            case battleState.End:

                break;
            case battleState.Dialogue:
                break;
            case battleState.None:
                break;
        }
    }

    private void OnRollTimerStart()
    {
        RollCountdown.gameObject.SetActive(true);
    }

    private void OnRollConfirm()
    {
        if (BattleManager.Instance.playertomovefirst == 1)
        {
            P1FirstText.color = selectedcolor;
            P1SecondText.color = deselectedcolor;

            P2FirstText.color = deselectedcolor;
            P2SecondText.color = selectedcolor;
        }
        else
        {
            P2FirstText.color = selectedcolor;
            P2SecondText.color = deselectedcolor;

            P1FirstText.color = deselectedcolor;
            P1SecondText.color = selectedcolor;
        }   
    }

    private void ResetMoveOrder()
    {
        P1FirstText.color = deselectedcolor;
        P1SecondText.color = deselectedcolor;

        P2FirstText.color = deselectedcolor;
        P2SecondText.color = deselectedcolor;
    }

    public void AnnounceAttackName(int monster,int moveindex)
    {
        MonsterAsset asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(monster);
        if (asset == null) return;
        string movename = "";

        switch (moveindex)
        {
            default:
                switch (SettingsManager.Instance.data.Language)
                {
                    case GameLanguage.English:
                        movename = asset.Move1Name;
                        break;
                    case GameLanguage.Japanese:
                        movename = asset.Move1NameJP;           
                        break;
                    case GameLanguage.Mandarin:
                        movename = asset.Move1NameCN;
                        break;
                }         
                break;
        }
        MonsterAttackText.font = SettingsManager.Instance.GetLocalizedOutlineFont();

        MonsterAttackText.text = movename;
        MonsterAttackText.gameObject.SetActive(true);

        Debug.Log("Attackcalled");
    }

    public void DeactivateAttackName()
    {
        MonsterAttackText.gameObject.SetActive(false);
    }


    private void HandleOnMonsterDeath(int winner)
    {
        switch(winner)
        {
            case 1:
                WinnerText.text = "Player 1 Wins!";
                break;
            case 2:
                WinnerText.text = "Player 2 Wins! (You Lose)";
                ExitBattleBtnLose.onClick.RemoveAllListeners();
                ExitBattleBtnLose.onClick.AddListener(() => {
                    BattleManager.Instance.ExitBattle(true);
                    ExitBattleBtnLose.gameObject.SetActive(false);
                });
                ExitBattleBtnLose.gameObject.SetActive(true);
                break;
        }
        BattleEndUI.SetActive(true);
    }
    public void DeactivateWinUI()
    {
        BattleEndUI.SetActive(false);
    }
    public void OnCaptureUI()
    {
        BattleEndUI.SetActive(false);
        CaptureUI.SetActive(true);

        CaptureYesBtn.onClick.RemoveAllListeners();
        CaptureYesBtn.onClick.AddListener(() => {
            CaptureAnimator.SetTrigger("CaptureMonster");
            CaptureUI.SetActive(false);
        });

        CaptureNoBtn.onClick.RemoveAllListeners();
        CaptureNoBtn.onClick.AddListener(() => {
            CaptureAnimator.SetTrigger("ReleaseMonster");
            CaptureUI.SetActive(false);
        });
    }

    private void OnRewardsScreen(int[] EXPGet)
    {
        ExitBattleBtn.onClick.RemoveAllListeners();
        ExitBattleBtn.onClick.AddListener(() => {
            BattleManager.Instance.ExitBattle(true);
            ExitBattleBtn.gameObject.SetActive(false);
        });
        ExitBattleBtn.gameObject.SetActive(true);

        CaptureUI.SetActive(false);
        P1Controller.glovecontroller.OnRewardScreen();
        P2Controller.glovecontroller.OnRewardScreen();

        _EXPGet = EXPGet;
        BattleEndUI.SetActive(false);
        RewardsScreenUI.gameObject.SetActive(true);

        if(EXPCoroutine != null)
        {
            StopCoroutine(EXPCoroutine);
        }


        EXPCoroutine = StartCoroutine(EXPGainAnimation(_EXPGet));
    }

    private void OnBattleEnd()
    {
        BattleMainUI.SetActive(false);

        P1Controller.OnBattleEnd();
        P2Controller.OnBattleEnd();

        HandleOnMonsterDeath(BattleManager.Instance.playerwon);

        if (BattleManager.Instance.playerwon != 1) return;

        BattleController controller = BattleManager.Instance.GetControllerByIndex(1);
        CachedEXP = new List<int>();
        CachedLevel = new List<int>(); // New list to track levels

        for (int i = 0; i < 5; i++)
        {
            if (controller.glove.cellmonsters[i] != null && controller.glove.cellmonsters[i].id != 0)
            {
                EXPText[i].text = $"{controller.glove.cellmonsters[i].currentexp:D3}"; // Show current EXP
                CachedEXP.Add(controller.glove.cellmonsters[i].currentexp);
                CachedLevel.Add(controller.glove.cellmonsters[i].currentlevel);
            }
            else
            {
                EXPText[i].text = "";
                CachedEXP.Add(0);
                CachedLevel.Add(1);
            }
            EXPGetText[i].text = $"";
        }
    }

    private void SkipEXPAnimation()
    {
        if (EXPCoroutine == null)
        {
            return;
        }
        else
        {
            StopCoroutine(EXPCoroutine);
        }

        BattleController controller = BattleManager.Instance.GetControllerByIndex(1);

        for (int i = 0; i < 5; i++)
        {
            EXPGetText[i].text = $"";
            if (controller.glove.cellmonsters[i] != null && controller.glove.cellmonsters[i].id != 0)
            {
                EXPText[i].text = $"{controller.glove.cellmonsters[i].currentexp:D3}";
                EXPGetText[i].text = $"+{_EXPGet[i]}";
                P1Controller.glovecontroller.UpdateMonsterLevels(i, controller.glove.cellmonsters[i].currentlevel);
            }
        }

        EXPCoroutine = null;
    }

    private IEnumerator EXPGainAnimation(int[] EXPGet)
    {
        yield return new WaitForSeconds(1f);
        BattleController controller = BattleManager.Instance.GetControllerByIndex(1);
        bool[] isAnimating = new bool[5]; // Track which monsters are still animating
        int[] remainingEXP = new int[5]; // Track remaining EXP to add for each monster

        // Initialize animation state
        for (int i = 0; i < 5; i++)
        {
            EXPGetText[i].text = $"";
            if (controller.glove.cellmonsters[i] != null && controller.glove.cellmonsters[i].id != 0)
            {
                EXPText[i].text = $"{CachedEXP[i]:D3}";
                isAnimating[i] = true;
                remainingEXP[i] = EXPGet[i]; // Initialize remaining EXP
                EXPGetText[i].text = $"+{EXPGet[i]}";
            }
        }

        // Keep animating until all monsters finish
        while (isAnimating.Any(x => x))
        {
            for (int i = 0; i < 5; i++)
            {
                if (!isAnimating[i]) continue;

                var monster = controller.glove.cellmonsters[i];
                if (monster == null || monster.id == 0)
                {
                    isAnimating[i] = false;
                    continue;
                }

                // If we still have EXP to add
                if (remainingEXP[i] > 0)
                {
                    // Add 1 EXP
                    CachedEXP[i]++;
                    remainingEXP[i]--;

                    // If EXP reached 1000, reset to 0 and increase level
                    if (CachedEXP[i] >= 1000)
                    {
                        CachedEXP[i] = 0;
                        CachedLevel[i]++;
                        P1Controller.glovecontroller.UpdateMonsterLevels(i, CachedLevel[i]);
                    }

                    EXPText[i].text = $"{CachedEXP[i]:D3}";
                }
                // If no more EXP to add, stop animating
                else
                {
                    isAnimating[i] = false;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        // Final update to match exact values
        for (int i = 0; i < 5; i++)
        {
            EXPGetText[i].text = $"";
            if (controller.glove.cellmonsters[i] != null && controller.glove.cellmonsters[i].id != 0)
            {
                EXPText[i].text = $"{controller.glove.cellmonsters[i].currentexp:D3}";
                EXPGetText[i].text = $"+{_EXPGet[i]}";
                P1Controller.glovecontroller.UpdateMonsterLevels(i, controller.glove.cellmonsters[i].currentlevel);
            }
        }

        EXPCoroutine = null;
    }

    private void OnBattleExit()
    {
        P1Controller.OnBattleExit();
        P2Controller.OnBattleExit();
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        UnsubscribeAllEvents();

        // Clear singleton instance if this is it
        if (instance == this)
        {
            instance = null;
        }
    }

    private void UnsubscribeAllEvents()
    {
        // BattleManager events
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStart -= OnBattleStart;
            BattleManager.Instance.OnBattleStartCutsceneEnd -= OnStartCutsceneEnd;
            BattleManager.Instance.OnPhaseChange -= OnPhaseChange;
            BattleManager.Instance.OnRollConfirm -= OnRollConfirm;
            BattleManager.Instance.OnBattleEnd -= OnBattleEnd;
            BattleManager.Instance.OnBattleExit -= OnBattleExit;
            BattleManager.Instance.OnRollTimerStart -= OnRollTimerStart;
            BattleManager.Instance.OnRewardScreen -= OnRewardsScreen;
        }

        // Input events
        if (BattleInputManager.Instance != null)
        {
            BattleInputManager.Instance.OnPause -= SkipEXPAnimation;
        }

        // UI button events
        CaptureYesBtn.onClick.RemoveAllListeners();
        CaptureNoBtn.onClick.RemoveAllListeners();
        ExitBattleBtn.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (BattleManager.Instance.state == battleState.Roll)
        {
            if (BattleManager.Instance.rolltimer > 1f)
            {
                RollCountdown.text = $"{(int)BattleManager.Instance.rolltimer}";
            }
            else
            {
                RollCountdown.text = $"{BattleManager.Instance.rolltimer:F2}";
            }
        }
    }
}
