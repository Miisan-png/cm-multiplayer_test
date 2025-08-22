using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MonsterInventoryIcon : MonoBehaviour
{
    [SerializeField] private Monster MonstertoHold;
    public Monster monstertohold => MonstertoHold;
    [SerializeField] private Image MonsterImage;
    public Image monsterimage => MonsterImage;
    private bool Initialized;
    public bool initialized => Initialized;

    private MonsterAsset cachedAsset;

    private int animationIndex;
    private Tweener IdleAnimation;

    public void InitializeUI(Monster M)
    {
        if (M == null || M.id < 1) return;
        MonstertoHold = M;

        MonsterAsset Asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(MonstertoHold.id);
        cachedAsset = Asset;

        MonsterImage.sprite = Asset.PixelArt[0] ?? null;
        MonsterImage.SetNativeSize();
        MonsterImage.gameObject.SetActive(true);
        Initialized = true;
        gameObject.SetActive(true);

        StartIdleAnimation();
    }

    private void StartIdleAnimation()
    {
        if(IdleAnimation != null)
        {
            IdleAnimation.Complete();
            IdleAnimation.Kill();
            IdleAnimation = null;
        }
        animationIndex = 0;

        float delay = Random.Range(0f, 0.4f);

        IdleAnimation = DOTween.To(() => 0, x => { }, 0, 0.2f)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() =>
                        {
                            if (cachedAsset != null && animationIndex < cachedAsset.PixelArt.Count)
                            {
                                MonsterImage.sprite = cachedAsset.PixelArt[animationIndex] ?? null;
                                animationIndex++;
                            }
                            else
                            {
                                animationIndex = 0;
                            }
                        }).SetDelay(delay);                      
    }

    public void UninitializeUI()
    {
        if (IdleAnimation != null)
        {
            IdleAnimation.Complete();
            IdleAnimation.Kill(); 
            IdleAnimation = null;
        }
        MonstertoHold = null;
        MonsterImage.gameObject.SetActive(false);
        Initialized = false;
        gameObject.SetActive(false);
    }
}
