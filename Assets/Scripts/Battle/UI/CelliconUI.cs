using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellIconUI : MonoBehaviour
{
    private int GloveIndex;

    [SerializeField] private Vector2 Origin;
    private bool originset;
    [SerializeField] private List<GameObject> DimObjs;
    [SerializeField] private GameObject ButtonTop;
    [SerializeField] private GameObject FakeTop;
    [SerializeField] private float ButtonAnimationSpeed = 1f;

    [SerializeField] private GameObject UnPressed;
    [SerializeField] private GameObject Pressed;
    [SerializeField] private GameObject ChargedBorder;
    [SerializeField] private Image EffectIcon;
    [SerializeField] private Image MonsterIcon;
    [SerializeField] private GameObject IdentityParent;
    [SerializeField] private GameObject FakeParent;
    [SerializeField] private Monster Mon;

    [SerializeField] private bool use = false;
    [SerializeField] private bool activated = false;
    public Monster monster => Mon;
    private Coroutine clickCoroutine;

    private int IdleAnimationIndex;
    private List<Sprite> IdleAnimation;
    private Tweener MonsterIdle;

    private void Awake()
    {
        if (!originset)
        {
            Origin = transform.position;
            originset = true;
        }
    }

    private void OnEnable()
    {
        StartPixelIdleAnimation(activated);
    }

    private void OnDisable()
    {
        StopIdleAnimation();
    }

    public void InitializeIcon(Monster mon,int _GloveIndex)
    {
        StopAllCoroutines();
        clickCoroutine = null;
        GloveIndex = _GloveIndex;
        ButtonTop.transform.position = UnPressed.transform.position;
        Mon = mon;
        EffectIcon.sprite = _GloveIndex != 0 ? BattleUIManager.Instance.GetEffectSpriteByElement(Mon.element) : null;
        EffectIcon.gameObject.SetActive(_GloveIndex != 0);
        FakeParent.SetActive(false);
        IdentityParent.SetActive(true);
        OnCellCharge(false);
        ResetTopPosition();
        OnCellActivated();
    }

    private void StartPixelIdleAnimation(bool Charged)
    {
        if (MonsterIdle != null)
        {
            MonsterIdle.Complete();
            MonsterIdle.Kill();
            MonsterIdle = null;
        }
        IdleAnimationIndex = 0;

        MonsterAsset cachedAsset = null;

        if(monster != null && monster.id != 0)
        {
            cachedAsset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(monster.id);
        }

        if(cachedAsset == null)
        {
            MonsterIcon.sprite = null;
            return;
        }

        if(Charged)
        {
            IdleAnimation = cachedAsset.PixelArt2;
        }
        else
        {
            IdleAnimation = cachedAsset.PixelArt;
        }
        
        MonsterIdle = DOTween.To(() => 0, x => { }, 0, 0.2f)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() =>
                        {
                            if (IdleAnimation != null && IdleAnimationIndex < IdleAnimation.Count)
                            {
                                MonsterIcon.sprite = IdleAnimation[IdleAnimationIndex] ?? null;
                                IdleAnimationIndex++;
                            }
                            else
                            {
                                IdleAnimationIndex = 0;
                            }
                        }).SetDelay(Random.Range(0,0.4f));
    }

    private void StopIdleAnimation()
    {
        if (MonsterIdle != null)
        {
            MonsterIdle.Complete();
            MonsterIdle.Kill();
            MonsterIdle = null;
        }
    }
    public void HideIdentity()
    {
        FakeParent.SetActive(true);
        IdentityParent.SetActive(false);
    }

    public void OnCellCharge(bool tocharge)
    {
        if (use == tocharge) return;

        use = tocharge;

        if (clickCoroutine != null)
        {
            StopCoroutine(clickCoroutine);
        }
        if(gameObject.activeInHierarchy)
        {
            clickCoroutine = StartCoroutine(ClickAnimation());
        }
        else
        {
            ChargedBorder.SetActive(use);
        }
    }

    public void ResetTopPosition()
    {
        ButtonTop.transform.position = UnPressed.transform.position;
    }

    public void OnCellActivated()
    {
        for(int i=0;i<DimObjs.Count;i++)
        {
            DimObjs[i].SetActive(false);
        }
        activated = true;
        StartPixelIdleAnimation(activated);
    }

    public void OnCellDeactivated()
    {
        for (int i = 0; i < DimObjs.Count; i++)
        {
            DimObjs[i].SetActive(true);
        }
        activated = false;
        StartPixelIdleAnimation(activated);
    }


    private IEnumerator ClickAnimation()
    {
        Vector2 newpos = use ? Pressed.gameObject.transform.position : UnPressed.gameObject.transform.position;

        if(!use)
        {
            ChargedBorder.SetActive(use);
        }
 
        while (Vector2.Distance(ButtonTop.transform.position, newpos) > 0.02f)
        {
            ButtonTop.transform.position = Vector2.MoveTowards(ButtonTop.transform.position, newpos, ButtonAnimationSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        ButtonTop.transform.position = newpos;

        float division = use ? 1f : 2f;

        yield return new WaitForSecondsRealtime(0.05f);

        transform.position = new Vector2(transform.position.x, transform.position.y - 6f);

        yield return new WaitForSecondsRealtime(0.1f);

        transform.position = new Vector2(transform.position.x, transform.position.y + (12f / division));
        ChargedBorder.SetActive(use);

        yield return new WaitForSecondsRealtime(0.2f);

        transform.position = Origin;
    }
}
