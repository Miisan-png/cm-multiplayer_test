using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokeDexUIController : MonoBehaviour
{
    private static PokeDexUIController instance;
    public static PokeDexUIController Instance => instance;

    public void Awake()
    {
        instance = this;
    }
    [SerializeField] private GameObject MainUI;
    [SerializeField] private PokeDexDatabase AllMonsterDex;
    [SerializeField] private Image PageImage;
    [SerializeField] private Image NameImage;

    [SerializeField] private RectTransform LeftArrow;
    [SerializeField] private RectTransform RightArrow;

    [SerializeField] private List<int> AllGetMonster;
    private int PageIndex = 0;

    private bool Initialized;
    public bool initialized => Initialized;

    public bool InitializeDexCondition()
    {
        if (PlayerInventory.Instance == null || PlayerInventory.Instance.monsterinventory == null || PlayerInventory.Instance.monsterinventory.Count < 1) return false;

        AllGetMonster = new List<int>();

        for (int i = 0; i < PlayerInventory.Instance.monsterinventory.Count; i++)
        {
            if (!AllGetMonster.Contains(PlayerInventory.Instance.monsterinventory[i].id))
            {
                AllGetMonster.Add(PlayerInventory.Instance.monsterinventory[i].id);
            }
        }

        return AllGetMonster != null && AllGetMonster.Count > 0;
    }

    public void InitializeDex()
    {
        if (!InitializeDexCondition()) return;

        UIInputManager.Instance.OnNavigate += OnNavigateText;
        Initialized = true;
        EnterNewPage(AllGetMonster[PageIndex]);

        MainUI.SetActive(true);
    }

    public void UnintializeDex(Action A)
    {
        UIInputManager.Instance.OnNavigate -= OnNavigateText;

        Initialized = false;

        MainUI.SetActive(false);

        A();
    }

    private void OnNavigateText(Vector2 Input)
    {
        if (AllGetMonster == null || AllGetMonster.Count < 1) return;

        if(Input.x > 0.1f)
        {
            PageIndex++;
        }
        else if(Input.x < -0.1f)
        {
            PageIndex--;
        }

        PageIndex = Mathf.Clamp(PageIndex, 0, AllGetMonster.Count-1);

        EnterNewPage(AllGetMonster[PageIndex]);
    }

    private void EnterNewPage(int ID)
    {
        PokeDexData data = AllMonsterDex.GetDataByID(ID);
        if (data == null) return;

        PageImage.sprite = data.PageSprite;

        Sprite namesprite = null;
        switch (SettingsManager.Instance.data.Language)
        {
            case GameLanguage.English:
                namesprite = data.EnName;
                break;
            case GameLanguage.Japanese:
                namesprite = data.JpName;
                break;
            case GameLanguage.Mandarin:
                namesprite = data.CnName;
                break;
        }
        NameImage.sprite = namesprite;

        NameImage.rectTransform.anchoredPosition = data.NameVector;
        NameImage.SetNativeSize();

        LeftArrow.gameObject.SetActive(PageIndex > 0);
        RightArrow.gameObject.SetActive(PageIndex < AllGetMonster.Count - 1);
    }

}
