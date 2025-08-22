using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GloveController : MonoBehaviour
{
    [SerializeField] private BattleController Controller;
    [SerializeField] private List<SpriteRenderer> NumberRenderers;
    [SerializeField] private List<SpriteRenderer> NumberNeededRenderers;
    [SerializeField] private List<SpriteRenderer> MonsterRenderers;
    [SerializeField] private List<SpriteRenderer> MonsterPixelRenderers;
    [SerializeField] private List<SpriteRenderer> BlackoutRenderers;
    [SerializeField] private List<SpriteRenderer> MirageRenderers;

    [SerializeField] private Material UnlitMat;
    [SerializeField] private Material LitMat;

    [SerializeField] private List<GameObject> SlotObjects;
    [SerializeField] private List<int> slotstoshuffle;
    [SerializeField] private float SlotAnimationSpeed;
    private List<Coroutine> SlotsCoroutine;
    private List<Element> EffectsQueue;
    private bool firstrollphase = false;
    private bool blackout;
    private bool revealingSlots;

    private int[] GloveMonstersIndex = new int[5];
    private Dictionary<int, List<Sprite>> GloveMonsterAnimations;
    private Tweener[] GloveMonstersIdle = new Tweener[5];

    public void InitializeController(BattleController C)
    {
        Controller = C;
        Controller.OnSlotsChanged += OnSlotsRoll;
        GloveMonsterAnimations = new Dictionary<int, List<Sprite>>();

        for (int i = 0; i < 5; i++)
        {
            NumberRenderers[i].gameObject.SetActive(false);
            MonsterRenderers[i].gameObject.SetActive(false);
            NumberNeededRenderers[i].gameObject.SetActive(false);

            if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                NumberRenderers[i].gameObject.SetActive(true);
                MonsterRenderers[i].gameObject.SetActive(true);
                NumberNeededRenderers[i].gameObject.SetActive(true);
                NumberNeededRenderers[i].sprite = BattleUIManager.Instance.GetBottomSlotSpritebyNumber(CompareHelper.GetNumberfromElement(Controller.glove.cellmonsters[i].element));
                NumberRenderers[i].material = UnlitMat;
                NumberNeededRenderers[i].material = UnlitMat;
                MonsterRenderers[i].material = UnlitMat;
            }

        }
        firstrollphase = true;

        SlotsCoroutine = new List<Coroutine>();
        EffectsQueue = new List<Element>();

        RefreshGloveIdleAnimations();
    }

    private void OnSlotsRoll()
    {
        UpdateSlots(true,false);
    }

    public void UpdateSlots(bool checkmirage, bool checkreroll)
    {
        bool willneedtorerollthisslot = false;

        for (int i = 0; i < 5; i++)
        {
            willneedtorerollthisslot = false;

            if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                if (slotstoshuffle != null && slotstoshuffle.Count > 0)
                {
                    for (int y = 0; y < slotstoshuffle.Count; y++)
                    {
                        if (slotstoshuffle[y] == i && checkreroll)
                        {
                            willneedtorerollthisslot = true;
                            break;
                        }
                    }
                }

                if (willneedtorerollthisslot)
                {
                    NumberRenderers[i].material = LitMat;
                    NumberNeededRenderers[i].material = LitMat;
                    MonsterRenderers[i].material = LitMat;
                    continue;
                }

                NumberRenderers[i].sprite = BattleUIManager.Instance.GetSlotSpritebyNumber(Controller.numberslots[i].number);

                if (CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.cellmonsters[i].element) || CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.equippedmonster.element))
                {
                    NumberRenderers[i].material = LitMat;

                    if (!blackout)
                    {
                        NumberNeededRenderers[i].material = LitMat;
                        MonsterRenderers[i].material = LitMat;
                    }
                }
                else
                {
                    NumberRenderers[i].material = UnlitMat;
                    MonsterRenderers[i].material = UnlitMat;
                    NumberNeededRenderers[i].material = UnlitMat;
                }
            }
        }
        if (checkmirage)
        {
            CheckforMirage();
        }

    }


    public void ReshuffleSlot(int slot)
    {
        if (slot > 5 || slot < 0) return;
        if(slotstoshuffle == null)
        {
            slotstoshuffle = new List<int>();
        }

        slotstoshuffle.Add(slot);
        QueueEffects(Element.Hydro);
    }


    private void CheckForBlackOut()
    {
        Dictionary<ChargeEffects, int> effects = Controller.effects;

        var keys = effects.Keys.ToList();

        bool electricEffect = false;

        foreach (var key in keys)
        {
            if(key.ElementID == Element.Electric)
            {
                electricEffect = true;
                break;
            }
        }

        if (!electricEffect)
        {
            blackout = false;
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            if (!Controller.slotsactivated[i])
            {
                BlackoutRenderers[i].gameObject.SetActive(true);
            }
        }
        blackout = true;
        QueueEffects(Element.Electric);
    }


    private void ClearBlackOut()
    {
        if (!blackout) return;
        blackout = false;
        for (int i = 0; i < 5; i++)
        {
            if (!Controller.slotsactivated[i] && !slotstoshuffle.Contains(i))
            {
                StartCoroutine(BlackOutClearEffect(BlackoutRenderers[i].gameObject,1f));
            }
            else
            {
                BlackoutRenderers[i].gameObject.SetActive(false); // just in case
            }
        }
    }

    private void CheckforMirage()
    {
        for (int i = 0; i < 5; i++)
        {
            MirageRenderers[i].gameObject.SetActive(false);
            if (Controller.numberslots[i].fake)
            {
                MirageRenderers[i].sprite = BattleUIManager.Instance.GetSlotSpritebyNumber(CompareHelper.GetNumberfromElement(Controller.glove.cellmonsters[i].element));
                MirageRenderers[i].gameObject.SetActive(true);

                if(!blackout)
                {
                    MonsterRenderers[i].material = LitMat;
                    NumberNeededRenderers[i].material = LitMat;
                }
            }
        }
    }

    private void ClearMirage()
    {
        for(int i=0;i<MirageRenderers.Count;i++)
        {
            if (MirageRenderers[i].gameObject.activeSelf)
            {
                StartCoroutine(MirageEffect(i, 1f));
                NumberRenderers[i].sprite = BattleUIManager.Instance.GetSlotSpritebyNumber(0);
            }
        }
    }

    public void OnRollPhase()
    {
        revealingSlots = false;

        if (SlotsCoroutine != null && SlotsCoroutine.Count > 0)
        {
            for(int i=0;i<SlotsCoroutine.Count;i++)
            {
                StopCoroutine(SlotsCoroutine[i]);
            }
            SlotsCoroutine.Clear();
        }

        CheckForBlackOut();

        if(firstrollphase)
        {
            float delay = 0;

            for (int i = 0; i < 5; i++)
            {
                SlotObjects[i].transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            for (int i = 0; i < 5; i++)
            {
                NumberRenderers[i].material = UnlitMat;
                MonsterRenderers[i].material = UnlitMat;
                NumberNeededRenderers[i].material = UnlitMat;
                SlotRollAnimation(i, delay, true);
                delay += 0.06f;
            }
            firstrollphase = false;
        }
        else
        {
            float delay = 0;

            for (int i = 0; i < 5; i++)
            {
                delay = Random.Range(0, 0.25f);

                if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
                {
                    if (!Controller.slotsactivated[i] && (CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.cellmonsters[i].element) || CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.equippedmonster.element)))
                    {
                        NumberRenderers[i].material = UnlitMat;
                        MonsterRenderers[i].material = UnlitMat;
                        NumberNeededRenderers[i].material = UnlitMat;
                        SlotRollAnimation(i, delay, true);
                    }
                }
            }
        }

        RefreshGloveIdleAnimations();
    }

    public void OnRollConfirm()
    {
        if (blackout || revealingSlots) return;

        float delay = 0;

        for(int i=0;i<5;i++)
        {
            if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                if ((CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.cellmonsters[i].element) || CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.equippedmonster.element))|| Controller.numberslots[i].fake)
                {
                    SlotRollAnimation(i, delay, false);
                }
            }
        }
    }


    private IEnumerator ReshuffleEffect(int slot, float duration)
    {
        SpriteRenderer SR = NumberRenderers[slot];
        SR.material = UnlitMat;
        MonsterRenderers[slot].material = UnlitMat;
        NumberNeededRenderers[slot].material = UnlitMat;

        float timer = duration;
        float effectInterval = duration / 15f; // Distribute 15 changes over the duration

        while (timer > 0)
        {
            SR.sprite = BattleUIManager.Instance.GetSlotSpritebyNumber(Random.Range(0, 10));
            timer -= effectInterval;
            yield return new WaitForSeconds(effectInterval);
        }

        SR.sprite = BattleUIManager.Instance.GetSlotSpritebyNumber(Controller.numberslots[slot].number);

        if (CompareHelper.ElementMatchesInt(Controller.numberslots[slot].number, Controller.glove.cellmonsters[slot].element) || CompareHelper.ElementMatchesInt(Controller.numberslots[slot].number, Controller.glove.equippedmonster.element))
        {
            SR.material = LitMat;
            MonsterRenderers[slot].material = LitMat;
            NumberNeededRenderers[slot].material = LitMat;
        }
    }

    private IEnumerator BlackOutClearEffect(GameObject renderer, float duration)
    {
        int cycles = 4; // 4 off/on cycles (8 state changes total)
        float remainingTime = duration;

        for (int i = 0; i < cycles; i++)
        {
            // Calculate random interval (weighted by remaining time)
            float minInterval = 0.05f; // Minimum time for any interval
            float maxInterval = remainingTime / (2 * (cycles - i)) * 2f; // Dynamic max

            // Off state
            renderer.SetActive(false);
            float offInterval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(offInterval);
            remainingTime -= offInterval;

            // On state (only if not last cycle)
            if (i < cycles - 1)
            {
                renderer.SetActive(true);
                float onInterval = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(onInterval);
                remainingTime -= onInterval;
            }
        }

        // Ensure final state is off
        renderer.SetActive(false);
    }

    private IEnumerator MirageEffect(int slot,float duration)
    {
        MonsterRenderers[slot].material = LitMat;
        NumberNeededRenderers[slot].material = LitMat;
        MirageRenderers[slot].gameObject.SetActive(true);
        float interval = duration / 6f; // 6 state changes

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(interval);
            MonsterRenderers[slot].material = UnlitMat;
            NumberNeededRenderers[slot].material = UnlitMat;
            MirageRenderers[slot].gameObject.SetActive(false);
            yield return new WaitForSeconds(interval);
            MonsterRenderers[slot].material = LitMat;
            NumberNeededRenderers[slot].material = LitMat;
            MirageRenderers[slot].gameObject.SetActive(true);
        }

        MirageRenderers[slot].gameObject.SetActive(false);
        MonsterRenderers[slot].material = UnlitMat;
        NumberNeededRenderers[slot].material = UnlitMat;
    }

    public void QueueEffects(Element element)
    {
        if(EffectsQueue == null)
        {
            EffectsQueue = new List<Element>();
        }
        switch (element)
        {
            case Element.None:
                break;
            case Element.Heat:
                break;
            case Element.Electric:
                EffectsQueue.Add(element);
                break;
            case Element.Wind:
                break;
            case Element.Solar:
                EffectsQueue.Add(element);
                break;
            case Element.Hydro:
                EffectsQueue.Add(element);
                break;
            case Element.Sound:
                break;
        }

        BattleManager.Instance.DelayRollEnd();
    }

    public void PlayEffects()
    {
        StartCoroutine(EffectsSequence());
    }

    private IEnumerator EffectsSequence()
    {

        if (EffectsQueue.Contains(Element.Electric))
        {
            ClearBlackOut();
            UpdateSlots(true, true);
            yield return new WaitForSeconds(1.5f);
        }
        if (EffectsQueue.Contains(Element.Solar))
        {
            ClearMirage();
            yield return new WaitForSeconds(1.5f);
        }
        if(EffectsQueue.Contains(Element.Hydro))
        {
            if(slotstoshuffle.Count > 0)
            {
                for (int i = 0; i < slotstoshuffle.Count; i++)
                {
                    StartCoroutine(ReshuffleEffect(slotstoshuffle[i], 1.5f));
                }
            }
            slotstoshuffle.Clear();
            yield return new WaitForSeconds(1.5f);
        }

        UpdateSlots(false,false);

        EffectsQueue.Clear();
    }

    public void OnRollEnd()
    {
        revealingSlots = true;
        int delaymultiplier = EffectsQueue.Count;
        PlayEffects();

        float totaldelay = delaymultiplier > 0 ? 1.8f * delaymultiplier : 0.2f;
        StartCoroutine(SlotRevealDelay(totaldelay)); // The more the effect, the longer the delay before reveal 
    }

    private IEnumerator SlotRevealDelay(float startdelay)
    {
        float timer = 2f;

        if(startdelay > 0)
        {
            timer = startdelay;
        }

        yield return new WaitForSeconds(timer);

        float delay = 0; 
        for (int i = 0; i < 5; i++)
        {
            if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                if (CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.cellmonsters[i].element) || CompareHelper.ElementMatchesInt(Controller.numberslots[i].number, Controller.glove.equippedmonster.element))
                {
                    SlotRollAnimation(i, delay, false);
                }
                else
                {
                    SlotRollAnimation(i, delay, true);
                }
            }
        }
        revealingSlots = false;

        yield return new WaitForSeconds(2f);
        StopIdleAnimations();

    }


    private void SlotRollAnimation(int slot,float delay,bool open)
    {
        if (slot > 5 || slot < 0) return;
        Quaternion OpenRotation = Quaternion.Euler(-90f, 0, 0); // Open position
        Quaternion ShutRotation = Quaternion.Euler(0, 0, 0);    // Closed position

        Quaternion currentRotation = open ? OpenRotation : ShutRotation;

        SlotObjects[slot].transform.DOLocalRotateQuaternion(currentRotation, 0.35f).SetDelay(delay).SetEase(Ease.InOutElastic);
    }

    public void OnBattleEnd()
    {
        if (Controller == null) return;

        for (int i = 0; i < Controller.glove.cellmonsters.Length; i++)
        {
            float delay = Random.Range(0.1f, 0.3f);

            if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                UpdateMonsterLevels(i, Controller.glove.cellmonsters[i].currentlevel);
                NumberNeededRenderers[i].sprite = BattleUIManager.Instance.GetBottomSlotSpritebyNumber(10);
            }
        }
    }

    public void OnRewardScreen()
    {
        if (SlotsCoroutine != null && SlotsCoroutine.Count > 0)
        {
            for (int i = 0; i < SlotsCoroutine.Count; i++)
            {
                StopCoroutine(SlotsCoroutine[i]);
            }
            SlotsCoroutine.Clear();
        }

        for (int i = 0; i < Controller.glove.cellmonsters.Length; i++)
        {
            float delay = Random.Range(0.1f, 0.3f);

            if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                SlotRollAnimation(i, delay, true);
                NumberRenderers[i].material = UnlitMat;
                MonsterRenderers[i].material = UnlitMat;
                NumberNeededRenderers[i].material = UnlitMat;
            }
        }
    }

    private void RefreshGloveIdleAnimations()
    {
        float delay = 0f;
        for (int i = 0; i < GloveMonstersIdle.Length; i++)
        {
            delay = Random.Range(0f, 0.3f);
            StartGloveIdleAnimation(i, delay);
        }
    }

    private void StartGloveIdleAnimation(int index, float delay)
    {
        if (GloveMonstersIdle[index] != null)
        {
            GloveMonstersIdle[index].Complete();
            GloveMonstersIdle[index].Kill();
            GloveMonstersIdle[index] = null;
        }
        GloveMonstersIndex[index] = 0;

        MonsterAsset cachedAsset = null;

        if (BattleManager.Instance.GetGlovebyIndex(1).cellmonsters[index] != null && BattleManager.Instance.GetGlovebyIndex(1).cellmonsters[index].id != 0)
        {
             cachedAsset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(BattleManager.Instance.GetGlovebyIndex(1).cellmonsters[index].id);
        }
        else
        {
            MonsterPixelRenderers[index].sprite = null;
            return;
        }

        GloveMonsterAnimations[index] = cachedAsset.PixelArt;

        GloveMonstersIdle[index] = DOTween.To(() => 0, x => { }, 0, 0.2f)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() =>
                        {
                            if (GloveMonsterAnimations[index] != null && GloveMonstersIndex[index] < GloveMonsterAnimations[index].Count)
                            {
                                MonsterPixelRenderers[index].sprite = GloveMonsterAnimations[index][GloveMonstersIndex[index]] ?? null;
                                GloveMonstersIndex[index]++;
                            }
                            else
                            {
                                GloveMonstersIndex[index] = 0;
                            }
                        }).SetDelay(delay);
    }

    private void StopIdleAnimations()
    {
        for (int i = 0; i < GloveMonstersIdle.Length; i++)
        {
            if (GloveMonstersIdle[i] != null)
            {
                GloveMonstersIdle[i].Complete();
                GloveMonstersIdle[i].Kill();
                GloveMonstersIdle[i] = null;
            }
        }
    }

    public void UpdateMonsterLevels(int slot, int newlevel)
    {
        if (Controller.glove.cellmonsters[slot] != null && Controller.glove.cellmonsters[slot].id != 0)
        {
            NumberRenderers[slot].sprite = BattleUIManager.Instance.GetSlotSpritebyNumber(newlevel);
        }
    }

    public void UninitializeController()
    {
        StopIdleAnimations();
        if(Controller != null)
        {
            Controller.OnSlotsChanged -= OnSlotsRoll;
        }
        Controller = null;
    }
}
