using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

public class MonsterInventoryUIController : MonoBehaviour
{
    private static MonsterInventoryUIController instance;
    public static MonsterInventoryUIController Instance => instance;

    private void Awake()
    {
        instance = this;
    }
    [SerializeField] private GameObject MainUI;
    [SerializeField] private GameObject IconPrefab;
    [SerializeField] private GameObject IconSpawnParent;
    [SerializeField] private List<RectTransform> GloveRects;
    [SerializeField] private List<MonsterInventoryIcon> AllIcons;
    [SerializeField] private List<SpriteRenderer> AllGloveRenderers;
    private Monster SelectedMonster;
    [SerializeField] private GameObject SelectedMonsterParent;
    [SerializeField] private Image SelectedMonsterImage;


    public Monster selectedmonster => SelectedMonster;
    [SerializeField] private Vector2 OpenListVector;
    [SerializeField] private Vector2 ClosedListVector;

    [SerializeField] private MonsterInventoryIcon HoveredIcon;
    [SerializeField] private RectTransform GloveHoverArrow;
    [SerializeField] private int GloveIndex;
    [SerializeField] private RectTransform InventoryHoverArrow;
    [SerializeField] private int InventoryIndex;
    [SerializeField] private InventorySection CurrentSection;
    [SerializeField] private GameObject InventoryPanelBG;

    [SerializeField] private GameObject TextBoxParent;
    [SerializeField] private TextMeshProUGUI NameTextBox;
    [SerializeField] private TextMeshProUGUI LevelTextBox;
    [SerializeField] private TextMeshProUGUI EXPTextBox;
    [SerializeField] private TextMeshProUGUI ElementTextBox;
    [SerializeField] private TextMeshProUGUI IDTextBox;
    [SerializeField] private TextMeshProUGUI MoveTextBox;

    public InventorySection currentsection => CurrentSection;
    [SerializeField] private float rowHeight = 135f;

    private int selectedAnimationIndex;
    private Tweener SelectedMonsterIdle;

    private int[] GloveMonstersIndex = new int[5];
    private Dictionary<int,List<Sprite>> GloveMonsterAnimations;
    private Tweener[] GloveMonstersIdle = new Tweener[5];

    private bool Initialized;
    public bool initialized => Initialized;

    [SerializeField] private ScrollRect inventoryScrollRect;
    public enum InventorySection
    {
        Glove,Inventory
    }

    private MonsterInventoryIcon GetFirstAvailableIcon()
    {
        if (AllIcons == null || AllIcons.Count < 1) return null;

        for(int i=0;i<AllIcons.Count;i++)
        {
            if (!AllIcons[i].initialized)
            {
                return AllIcons[i];
            }
        }
        return null;
    }

    private int GetActivatedIcons()
    {
        int count = 0;
        for (int i = 0; i < AllIcons.Count; i++)
        {
            if (AllIcons[i].initialized)
            {
                count++;
            }
        }
        return count - 1;
    }

    private int GetCurrentGloveSlot()
    {
        int slot = 0;

        for (int i = 1; i < 5; i++)
        {
            if (PlayerInventory.Instance.playerglove.cellmonsters[i] != null && PlayerInventory.Instance.playerglove.cellmonsters[i].id != 0)
            {
                slot = i;          
            }
        }

        slot = Mathf.Clamp(slot,0, 4);
        return slot;
    }

    public void InitializeInventoryUI()
    {
        SubscribetoInputs();
        MainUI.SetActive(true);
        Initialized = true;
        RefreshUI();
        GloveMonsterAnimations = new Dictionary<int, List<Sprite>>();
        if(SettingsManager.Instance.data.Language != GameLanguage.English)
        {
            NameTextBox.font = SettingsManager.Instance.GetLocalizedFont();
            ElementTextBox.font = SettingsManager.Instance.GetLocalizedFont();
            MoveTextBox.font = SettingsManager.Instance.GetLocalizedFont();
        }
        else
        {
            NameTextBox.font = SettingsManager.Instance.enfontline;
            ElementTextBox.font = SettingsManager.Instance.enfontline;
            MoveTextBox.font = SettingsManager.Instance.enfontline;
        }


        GloveHoverArrow.DOAnchorPos(new Vector2(GloveRects[GloveIndex].anchoredPosition.x, GloveRects[GloveIndex].anchoredPosition.y),0f);
        RefreshGloveIdleAnimations();
    }

    private void SubscribetoInputs()
    {
        UIInputManager.Instance.OnNavigate += OnNavigate;
        UIInputManager.Instance.OnInteract += OnConfirm;
        UIInputManager.Instance.OnCancel += OnCancel;
    }

    private void UnSubscribetoInputs()
    {
        UIInputManager.Instance.OnNavigate -= OnNavigate;
        UIInputManager.Instance.OnInteract -= OnConfirm;
        UIInputManager.Instance.OnCancel -= OnCancel;
    }

    private void DisplayCurrentMonsterStats()
    {
        Monster M = null;
        if (SelectedMonster == null)
        {
            switch (CurrentSection)
            {
                case InventorySection.Glove:
                    M = PlayerInventory.Instance.playerglove.cellmonsters[GloveIndex];
                    break;
                case InventorySection.Inventory:
                    M = HoveredIcon.monstertohold;
                    break;
            }
        }
        else
        {
            M = SelectedMonster;
        }

        MonsterAsset asset = null;

        if(M != null && M.id != 0)
        {
            asset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(M.id);
        }

        if (M == null || asset == null)
        {
            TextBoxParent.SetActive(false);
        }
        else if (asset != null)
        {
            switch (SettingsManager.Instance.data.Language)
            {
                case GameLanguage.English:
                    NameTextBox.text = M.name;      
                    ElementTextBox.text = M.element.ToString().ToUpper();
                    MoveTextBox.text = asset.Move1Name;
                    break;
                case GameLanguage.Japanese:
                    NameTextBox.text = asset.NameJP;
                    ElementTextBox.text = MonsterManager.Instance.GetLocalizedElementString(M.element);
                    MoveTextBox.text = asset.Move1NameJP;
                    break;
                case GameLanguage.Mandarin:
                    NameTextBox.text = asset.NameCN;
                    ElementTextBox.text = MonsterManager.Instance.GetLocalizedElementString(M.element);   
                    MoveTextBox.text = asset.Move1NameCN;
                    break;
            }
            IDTextBox.text = M.id.ToString("D3");
            LevelTextBox.text = $"Lvl {M.currentlevel.ToString("D3")}";
            EXPTextBox.text = $"{M.maxexp - M.currentexp}";
            TextBoxParent.SetActive(true);
        }
    }

    private void OnNavigate(Vector2 Input)
    {
        switch (CurrentSection)
        {
            case InventorySection.Glove:

                if(Input.x > 0.1f)
                {
                    GloveIndex++;
                    GloveIndex = Mathf.Clamp(GloveIndex, 0, 4);
                    GloveHoverArrow.DOAnchorPos(new Vector2(GloveRects[GloveIndex].anchoredPosition.x, GloveRects[GloveIndex].anchoredPosition.y), 0.1f);
                }
                else if(Input.x < -0.1f)
                {
                    GloveIndex--;
                    GloveIndex = Mathf.Clamp(GloveIndex, 0, 4);
                    GloveHoverArrow.DOAnchorPos(new Vector2(GloveRects[GloveIndex].anchoredPosition.x, GloveRects[GloveIndex].anchoredPosition.y), 0.1f);
                }
                else if(Input.y < -0.1f)
                {
                    EnterSection(1);
                }
                break;
            case InventorySection.Inventory:
                if (Input.x > 0.1f)
                {
                    InventoryIndex++;
                    InventoryIndex = Mathf.Clamp(InventoryIndex, 0, GetActivatedIcons());
                    HoverOnInventoryIcon(InventoryIndex);
                }
                else if (Input.x < -0.1f)
                {
                    InventoryIndex--;
                    InventoryIndex = Mathf.Clamp(InventoryIndex, 0, GetActivatedIcons());
                    HoverOnInventoryIcon(InventoryIndex);
                }
                else if (Input.y > 0.1f)
                {
                    if((InventoryIndex - 5) <= -1)
                    {
                        EnterSection(0);
                        return;
                    }
                    InventoryIndex -= 5;
                    InventoryIndex = Mathf.Clamp(InventoryIndex, 0, GetActivatedIcons());
                    HoverOnInventoryIcon(InventoryIndex);
                }
                else if (Input.y < -0.1f)
                {
                    InventoryIndex += 5;
                    InventoryIndex = Mathf.Clamp(InventoryIndex, 0, GetActivatedIcons());
                    HoverOnInventoryIcon(InventoryIndex);
                }
                break;
        }
        DisplayCurrentMonsterStats();
    }

    private void OnConfirm()
    {
        switch (CurrentSection)
        {
            case InventorySection.Glove:
                if(SelectedMonster != null && SelectedMonster.id != 0)
                {
                    if(GloveIndex > 0 && PlayerInventory.Instance.playerglove.equippedmonster != null && PlayerInventory.Instance.playerglove.equippedmonster.id != 0)
                    {
                        PlayerInventory.Instance.playerglove.SetCellMonster(GloveIndex, SelectedMonster);
                        RefreshGloveIdleAnimations();
                    }
                    else
                    {
                        PlayerInventory.Instance.playerglove.SetEquippedMonster(SelectedMonster);
                        RefreshGloveIdleAnimations();
                    }
                    SelectedMonster = null;
                    SelectedMonsterParent.SetActive(false);
                }
                else
                {
                    PlayerInventory.Instance.playerglove.RemoveCellMonster(GloveIndex);
                    RefreshGloveIdleAnimations();
                }

                GloveIndex = GetCurrentGloveSlot();
                GloveHoverArrow.DOAnchorPos(new Vector2(GloveRects[GloveIndex].anchoredPosition.x, GloveRects[GloveIndex].anchoredPosition.y), 0.1f);
                RefreshUI();
                break;
            case InventorySection.Inventory:
                SelectedMonster = AllIcons[InventoryIndex].monstertohold;
                SelectedMonsterParent.SetActive(true);
                SelectedMonsterImage.sprite = AllIcons[InventoryIndex].monsterimage.sprite;
                SelectedMonsterImage.SetNativeSize();
                StartSelectedIdleAnimation();
                EnterSection(0);
                RefreshUI();
                break;
        }
    }

    private void RefreshGloveIdleAnimations()
    {
        float delay = 0f;
        for (int i = 0; i < GloveMonstersIdle.Length; i++)
        {
            delay = UnityEngine.Random.Range(0f, 0.3f);
            StartGloveIdleAnimation(i,delay);
        }
    }

    private void StartGloveIdleAnimation(int index,float delay)
    {
        if (GloveMonstersIdle[index] != null)
        {
            GloveMonstersIdle[index].Complete();
            GloveMonstersIdle[index].Kill();
            GloveMonstersIdle[index] = null;
        }
        GloveMonstersIndex[index] = 0;

        if (PlayerInventory.Instance == null || PlayerInventory.Instance.playerglove.cellmonsters[index] == null || PlayerInventory.Instance.playerglove.cellmonsters[index].id == 0)
        {
            AllGloveRenderers[index].sprite = null;
            return;
        }
            MonsterAsset cachedAsset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(PlayerInventory.Instance.playerglove.cellmonsters[index].id);

        if (cachedAsset == null) return;
        GloveMonsterAnimations[index] = cachedAsset.PixelArt;

        GloveMonstersIdle[index] = DOTween.To(() => 0, x => { }, 0, 0.2f)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() =>
                        {
                            if (GloveMonsterAnimations[index] != null && GloveMonstersIndex[index] < GloveMonsterAnimations[index].Count)
                            {
                                AllGloveRenderers[index].sprite = GloveMonsterAnimations[index][GloveMonstersIndex[index]] ?? null;
                                GloveMonstersIndex[index]++;
                            }
                            else
                            {
                                GloveMonstersIndex[index] = 0;
                            }
                        }).SetDelay(delay);
    }

    private void StartSelectedIdleAnimation()
    {
        if (SelectedMonsterIdle != null)
        {
            SelectedMonsterIdle.Complete();
            SelectedMonsterIdle.Kill();
            SelectedMonsterIdle = null;
        }

        selectedAnimationIndex = 0;

        if (SelectedMonster == null) return;

        MonsterAsset cachedAsset = MonsterManager.Instance.monsterDatabase.GetAssetsByID(SelectedMonster.id);

        SelectedMonsterIdle = DOTween.To(() => 0, x => { }, 0, 0.3f)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() =>
                        {
                            if (cachedAsset != null && selectedAnimationIndex < cachedAsset.PixelArt2.Count)
                            {
                                SelectedMonsterImage.sprite = cachedAsset.PixelArt2[selectedAnimationIndex] ?? null;
                                selectedAnimationIndex++;
                            }
                            else
                            {
                                selectedAnimationIndex = 0;
                            }
                        });
    }

    private void OnCancel()
    {
        switch (CurrentSection)
        {
            case InventorySection.Glove:
                SelectedMonster = null;
                SelectedMonsterParent.SetActive(false);
                break;
            case InventorySection.Inventory:
                SelectedMonster = null;
                EnterSection(0);
                break;
        }

        DisplayCurrentMonsterStats();
    }

    private void RefreshUI()
    {
        for(int i=0;i<AllGloveRenderers.Count;i++)
        {
            if (PlayerInventory.Instance.playerglove.cellmonsters[i] != null && PlayerInventory.Instance.playerglove.cellmonsters[i].id != 0)
            {
                AllGloveRenderers[i].sprite = MonsterManager.Instance.monsterDatabase.GetAssetsByID(PlayerInventory.Instance.playerglove.cellmonsters[i].id).PixelArt[0];
            }
            else
            {
                AllGloveRenderers[i].sprite = null;
            }
        }

        for(int i=0;i<AllIcons.Count;i++)
        {
            AllIcons[i].UninitializeUI();
        }

        for (int i = 0; i < PlayerInventory.Instance.monsterinventory.Count; i++)
        {
            if (!PlayerInventory.Instance.monsterinventory[i].Equipped)
            {
                MonsterInventoryIcon Icontouse = GetFirstAvailableIcon();

                if (Icontouse == null)
                {
                    GameObject obj = Instantiate(IconPrefab, IconSpawnParent.transform);
                    MonsterInventoryIcon newicon = obj.GetComponent<MonsterInventoryIcon>();
                    newicon.InitializeUI(PlayerInventory.Instance.monsterinventory[i]);
                    AllIcons.Add(newicon);
                }
                else
                {
                    Icontouse.InitializeUI(PlayerInventory.Instance.monsterinventory[i]);
                }
            }
        }

        DisplayCurrentMonsterStats();
    }

    private void HoverOnInventoryIcon(int index)
    {
        if (AllIcons == null || index < 0 || index >= AllIcons.Count || AllIcons[index] == null)
            return;
        HoveredIcon = AllIcons[index];
        RectTransform rect = AllIcons[index].gameObject.transform as RectTransform;

        ScrollToRow(InventoryIndex / 5, () => {
   
            // Convert icon position from content space to viewport space
            Vector2 iconViewportPos = RectTransformUtility.WorldToScreenPoint(
                null, // If no camera is rendering UI, pass null
                rect.position
            );

            // Convert back to anchored position relative to the hover arrow's parent
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                InventoryHoverArrow.parent as RectTransform,
                iconViewportPos,
                null, // No camera needed for Canvas Overlay
                out Vector2 anchoredPos
            );

            // Apply offset (+130f in your case)
            anchoredPos.y += 130f;
            InventoryHoverArrow.DOAnchorPos(anchoredPos, 0.1f);
        });

    }

    private void ScrollToRow(int rowIndex,Action A)
    {
        float targetY = rowIndex * rowHeight;

        // Clamp to prevent overscrolling
        float maxScroll = inventoryScrollRect.content.rect.height - inventoryScrollRect.viewport.rect.height;
        targetY = Mathf.Clamp(targetY, 0, maxScroll);

        // Smooth scroll
        inventoryScrollRect.content.DOAnchorPosY(targetY, 0.1f).OnComplete(() => { A(); });
    }

    private void EnterSection(int section)
    {
        section = Mathf.Clamp(section, 0, 1);

        if (section == 1 && PlayerInventory.Instance.monsterinventory.Count < 1) return;

        CurrentSection = (InventorySection)section;

        switch (CurrentSection)
        {
            case InventorySection.Glove:
                GloveHoverArrow.gameObject.SetActive(true);
                InventoryHoverArrow.gameObject.SetActive(false);
                DOTween.Complete(InventoryHoverArrow);
                DOTween.Kill(InventoryHoverArrow);
                RectTransform inventoryrect = inventoryScrollRect.gameObject.transform as RectTransform;
                inventoryrect.DOAnchorPos(ClosedListVector, 0.2f).OnComplete(() => { inventoryScrollRect.verticalNormalizedPosition = 1f; });
                InventoryPanelBG.gameObject.SetActive(false);
                break;
            case InventorySection.Inventory:
                GloveHoverArrow.gameObject.SetActive(false);
                InventoryHoverArrow.gameObject.SetActive(true);
                InventoryIndex = Mathf.Clamp(InventoryIndex, 0, GetActivatedIcons());
                HoverOnInventoryIcon(InventoryIndex);
                DOTween.Complete(GloveHoverArrow);
                DOTween.Kill(GloveHoverArrow);
                inventoryrect = inventoryScrollRect.gameObject.transform as RectTransform;
                inventoryrect.DOAnchorPos(OpenListVector, 0.2f);
                InventoryPanelBG.gameObject.SetActive(true);
                break;
        }

        DisplayCurrentMonsterStats();
    }



    public void UnInitializeInventoryUI(Action A)
    {
        UnSubscribetoInputs();

        if (SelectedMonsterIdle != null)
        {
            SelectedMonsterIdle.Complete();
            SelectedMonsterIdle.Kill();
            SelectedMonsterIdle = null;
        }

        for (int i = 0; i < GloveMonstersIdle.Length; i++)
        {
            if (GloveMonstersIdle[i] != null)
            {
                GloveMonstersIdle[i].Complete();
                GloveMonstersIdle[i].Kill();
                GloveMonstersIdle[i] = null;
            }
        }

        for (int i=0;i<AllIcons.Count;i++)
        {
            AllIcons[i].UninitializeUI();
        }

        for (int i = AllIcons.Count - 1; i >= 50; i--)
        {
            Destroy(AllIcons[i].gameObject);
            AllIcons.RemoveAt(i);
        }
        MainUI.SetActive(false); 
        Initialized = false;
        SelectedMonster = null;
        A();
    }
}
