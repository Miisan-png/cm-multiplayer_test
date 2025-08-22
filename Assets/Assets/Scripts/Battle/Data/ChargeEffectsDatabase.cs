using System;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEffectsDatabase : MonoBehaviour
{
    private static ChargeEffectsDatabase instance;
    public static ChargeEffectsDatabase Instance => instance;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private Dictionary<Element, Dictionary<int, ChargeEffects>> AllEffects;
    public Dictionary<Element, Dictionary<int, ChargeEffects>> alleffects => AllEffects;

    private Dictionary<Element, Dictionary<int, string>> AllEffectDescription;
    public Dictionary<Element, Dictionary<int, string>> alleffectdescription => AllEffectDescription;
    private Dictionary<Element, Dictionary<int, string>> AllEffectDescriptionJP;
    public Dictionary<Element, Dictionary<int, string>> alleffectdescriptionjp => AllEffectDescriptionJP;
    private Dictionary<Element, Dictionary<int, string>> AllEffectDescriptionCN;
    public Dictionary<Element, Dictionary<int, string>> alleffectdescriptioncn => AllEffectDescriptionCN;
    private void Start()
    {
        InitializeDictionaries();
        RegisterAllEffects();
    }

    private void InitializeDictionaries()
    {
        AllEffects = new Dictionary<Element, Dictionary<int, ChargeEffects>>();
        AllEffectDescription = new Dictionary<Element, Dictionary<int, string>>();
        AllEffectDescriptionJP = new Dictionary<Element, Dictionary<int, string>>();
        AllEffectDescriptionCN = new Dictionary<Element, Dictionary<int, string>>();

        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            AllEffects[element] = new Dictionary<int, ChargeEffects>();
            AllEffectDescription[element] = new Dictionary<int, string>();
            AllEffectDescriptionJP[element] = new Dictionary<int, string>();
            AllEffectDescriptionCN[element] = new Dictionary<int, string>();
        }
    }

    private void RegisterAllEffects()
    {
        RegisterEffect(Element.Electric, 1, new ChargeEffects(
            ctx => {

            },
            ctx => {

            },
            ctx => {

            },
            EffectTiming.AfterRoll,
            Element.Electric,
            1,
            1
        ));
        RegisterDescription(Element.Electric, 1, "Inflict BLKT (Blackout) on the opponent.\nOpponent's numbers will not be displayed\nduring the next Charge Phase.");
        RegisterDescriptionJP(Element.Electric, 1, "敵に BLKT（盲）効果を付与する.\n敵の次のチャージフェーズでは、\nグローブボックスに番号が表示されない");
        RegisterDescriptionCN(Element.Electric, 1, "对敌人施加BLKT（盲）效果.\n敌人在下一回充力阶段时,手套箱上\n不会显示出号码.");

        RegisterEffect(Element.Hydro, 1, new ChargeEffects(
            ctx => {

            },
            ctx => {
                int slotrerolled = ctx.TargetController.RerollRandomSlot();

                if (ctx.TargetController is BattleController_Player)
                {
                    BattleController_Player player = ctx.TargetController as BattleController_Player;

                    player.uicontroller.glovecontroller.ReshuffleSlot(slotrerolled);
                }

                Debug.Log($"Water effect activated!");
            },
            ctx => {
            },
            EffectTiming.AfterRoll,
            Element.Hydro,
            1,
            1
        ));
        RegisterDescription(Element.Hydro, 1, "Inflict STCT (Short Circuit) on the opponent.\nOpponent randomly recharges all Charged Patoris\r\nafter Charge Phase.");
        RegisterDescriptionJP(Element.Hydro, 1, "敵に STCT（乱）効果を付与する.\n敵の次のチャージフェーズ終了時に、\r\n既にチャージしたバトリをランダムに再チャージする.");
        RegisterDescriptionCN(Element.Hydro, 1, "对敌人施加STCT（乱）效果.\n敌人在下一回充力阶段结束时,\r\n随机重新充过每个已充了的叭斗力.");

        RegisterEffect(Element.Heat, 1, new ChargeEffects(
            ctx => {
                ctx.TargetController.AddtoNumberPool(Element.None, false);
                ctx.TargetController.AddtoNumberPool(Element.None, false);
                Debug.Log($"Fire effect activated from player{ctx.UserController.playerindex} to player {ctx.TargetController.playerindex}");
            },
            ctx => {

            },
            ctx => {
                ctx.TargetController.RemoveNumberfromPool(Element.None, false);
                ctx.TargetController.RemoveNumberfromPool(Element.None, false);
                Debug.Log($"Fire effect dispel from player{ctx.TargetController.playerindex}");
            },
            EffectTiming.BeforeRoll,
            Element.Heat,
            1,
            1
        ));
        RegisterDescription(Element.Heat, 1, "Inflict BRNT (Burnout) on the opponent.\nOpponent have a chance to roll <u>zer0s</u> next turn,\nSKIP if rolled one or more <u>zer0s</u>.");
        RegisterDescriptionJP(Element.Heat, 1, "敵に BRNT（疲）効果を付与する.\n敵は次のチャージフェーズで、</u>「ゼロ0」</u>,をチャージする可能性がある\n敵が</u>「ゼロ0」</u>を1つ以上チャージしたとき、1ターン何もしない");
        RegisterDescriptionCN(Element.Heat, 1, "对敌人施加BRNT（累）效果.\n敌人在下一回充力阶段时，有机率充</u>“零０”</u>,\n敌人充一个或以上</u>“零０”</u>时，发呆一回.");

        RegisterEffect(Element.Wind, 1, new ChargeEffects(
            ctx => {
                ctx.TargetController.AddtoNumberPool(Element.None, false);
                ctx.TargetController.AddtoNumberPool(Element.None, false);
                Debug.Log($"Wind effect activated from player{ctx.UserController.playerindex} to player {ctx.TargetController.playerindex}");
            },
            ctx => {
                for (int i = 0; i < ctx.TargetController.numberslots.Length; i++)
                {
                    if (ctx.TargetController.numberslots[i].number == 0 && ctx.TargetController.glove.cellmonsters[i] != null && ctx.TargetController.glove.cellmonsters[i].id != 0)
                    {
                        if (ctx.TargetController.hp >= 3)
                        {
                            ctx.TargetController.TakeDamage(2);
                            BattleManager.Instance.MonsterDamaged?.Invoke(ctx.TargetController.playerindex, 2);
                            Debug.Log($"Wind debuff activated from player {ctx.TargetController.playerindex},take 2 damage");
                            break;
                        }
                    }
                }
            },
            ctx => {
                ctx.TargetController.RemoveNumberfromPool(Element.None, false);
                ctx.TargetController.RemoveNumberfromPool(Element.None, false);
                Debug.Log($"Wind effect dispel from player{ctx.TargetController.playerindex}");
            },
            EffectTiming.AfterRoll,
            Element.Wind,
            1,
            1
        ));
        RegisterDescription(Element.Wind, 1, "Inflict SHRD (Shred) on the opponent.\nOpponent have a chance to roll <u>zer0s</u> next turn,\nself inflicts 2 DMG if rolled one or more <u>zer0s</u>.");
        RegisterDescriptionJP(Element.Wind, 1, "敵に SHRD（破）効果を付与する.\n敵は次のチャージフェーズで、</u>「ゼロ0」</u>をチャージする可能性がある\n敵が</u>「ゼロ0」</u>を1つ以上チャージしたとき、自身に2ダメージを受ける");
        RegisterDescriptionCN(Element.Wind, 1, "对敌人施加SHRD（破）效果.\n敌人在下一回充力阶段时，有机率充</u>“零０”</u>\n敌人充一个或以上</u>“零０”</u>时，自身受到２伤害");

        RegisterEffect(Element.Solar, 1, new ChargeEffects(
            ctx => {
                ctx.TargetController.AddtoNumberPool(Element.None, true);
                ctx.TargetController.AddtoNumberPool(Element.None, true);
                Debug.Log($"Light effect activated from player{ctx.UserController.playerindex} to player {ctx.TargetController.playerindex}");
            },
            ctx => {
                if (ctx.TargetController is BattleController_Player)
                {
                    BattleController_Player player = ctx.TargetController as BattleController_Player;
                    player.uicontroller.glovecontroller.QueueEffects(Element.Solar);
                }
            },
            ctx => {
                ctx.TargetController.RemoveNumberfromPool(Element.None, true);
                ctx.TargetController.RemoveNumberfromPool(Element.None, true);
                Debug.Log($"Light effect dispel from player{ctx.TargetController.playerindex}");
            },
            EffectTiming.BeforeRoll,
            Element.Solar,
            1,
            1
        ));
        RegisterDescription(Element.Solar, 1, "Inflict MIRD (Mirage) on the opponent.\nOpponent have a change to roll <u>hidden zer0s</u> \nnext turn.");
        RegisterDescriptionJP(Element.Solar, 1, "敵に MIRG（幻）効果を付与する.\n敵は次のチャージフェーズで、隠された</u>「ゼロ0」</u>をチャージする可能性がある");
        RegisterDescriptionCN(Element.Solar, 1, "对敌人施加MIRG（幻）效果.\n敌人在下一回充力阶段时，有机率充隐藏的</u>“零０”</u>");

        RegisterEffect(Element.Sound, 1, new ChargeEffects(
            ctx => {
                ctx.TargetController.AddtoNumberPool(Element.None, false);
                ctx.TargetController.AddtoNumberPool(Element.None, false);
                Debug.Log($"Sound effect activated from player{ctx.UserController.playerindex} to player {ctx.TargetController.playerindex}");
            },
            ctx => {
                int dmg = 0;
                for (int i = 0; i < ctx.TargetController.numberslots.Length; i++)
                {
                    if (ctx.TargetController.numberslots[i].number == 0 && ctx.TargetController.glove.cellmonsters[i] != null && ctx.TargetController.glove.cellmonsters[i].id != 0)
                    {
                        if (ctx.TargetController.hp >= 2)
                        {
                            ctx.TargetController.TakeDamage(1);
                            dmg++;
                            continue;
                        }
                    }
                }
                if (dmg >= 1)
                {
                    Debug.Log($"Sound debuff activated from player {ctx.TargetController.playerindex},take total {dmg} damage");
                    BattleManager.Instance.MonsterDamaged?.Invoke(ctx.TargetController.playerindex, dmg);
                }
            },
            ctx => {
                ctx.TargetController.RemoveNumberfromPool(Element.None, false);
                ctx.TargetController.RemoveNumberfromPool(Element.None, false);
                Debug.Log($"Sound effect dispel from player{ctx.TargetController.playerindex}");
            },
            EffectTiming.AfterRoll,
            Element.Sound,
            1,
            1
        ));
        RegisterDescription(Element.Sound, 1, "Inflict CNFS (Confuse) on the opponent.\nOpponent have a chance to roll <u>zer0s</u> next turn,\nself inflicts 1 DMG for each rolled <u>zer0s</u>. ");
        RegisterDescriptionJP(Element.Sound, 1, "敵に CNFS（惑）効果を付与する.\n敵は次のチャージフェーズで、</u>「ゼロ0」</u>をチャージする可能性がある\n敵はチャージした</u>「ゼロ0」</u>1つごとに、自身に1ダメージを受ける");
        RegisterDescriptionCN(Element.Sound, 1, "对敌人施加CNFS（惑）效果.\n敌人在下一回充力阶段时，有机率充</u>“零０”</u>\n敌人每充一个</u>“零０”</u>，自身受到１伤害");
    }

    private void RegisterEffect(Element element, int effectId, ChargeEffects effect)
    {
        AllEffects[element][effectId] = effect;
    }

    private void RegisterDescription(Element element, int effectId, string desc)
    {
        AllEffectDescription[element][effectId] = desc;
    }
    private void RegisterDescriptionJP(Element element, int effectId, string desc)
    {
        AllEffectDescriptionJP[element][effectId] = desc;
    }
    private void RegisterDescriptionCN(Element element, int effectId, string desc)
    {
        AllEffectDescriptionCN[element][effectId] = desc;
    }

}