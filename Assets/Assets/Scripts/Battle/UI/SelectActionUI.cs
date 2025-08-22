using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectActionUI : MonoBehaviour
{
    [SerializeField] private BattleController_Player Player;
    [SerializeField] private GameObject IdentityParent;
    [SerializeField] private GameObject UIParent;
    [SerializeField] private GameObject BatteryInput;
    [SerializeField] private GameObject SelectAction;
    [SerializeField] private List<TextMeshProUGUI> SelectActionText;
    [SerializeField] private List<GameObject> SelectActionArrows;

    [SerializeField] private List<TextMeshProUGUI> AttackCellText;
    [SerializeField] private List<GameObject> AttackArrows;

    [SerializeField] private TextMeshProUGUI CellDamageBox;
    [SerializeField] private Image CellEffectIcon;
    [SerializeField] private TextMeshProUGUI EffectTextBox;
    [SerializeField] private TextMeshProUGUI CalculatedDamageBox;
    [SerializeField] private Slider CalculatedDamageSlider;
    [SerializeField] private List<Image> CalculatedEffectsIcon;
    private Coroutine DamageCalculateCoroutine;

    public void InitializeController(BattleController_Player player)
    {
        IdentityParent.SetActive(true);
        Player = player;
    }

    public void OnSelectPhaseStart()
    {
        SelectAction.SetActive(true);
        BatteryInput.SetActive(false);
        UIParent.SetActive(true);

        OnActionHover();
    }

    public void OnSelectPhaseEnd()
    {
        SelectAction.SetActive(false);
        BatteryInput.SetActive(false);
        UIParent.SetActive(false);
    }

    public void HideIdentity()
    {
        IdentityParent.SetActive(false);
    }

    private void DeSelectAllTexts()
    {
        if (Player == null) return;
        for(int i=0;i<SelectActionText.Count;i++)
        {
            switch(i)
            {
                case 0:
                    SelectActionText[i].color = Player.SelectActionCondition(selectAction.Attack) ? BattleUIManager.Instance.selectedcolor : BattleUIManager.Instance.deselectedcolor;
                    continue;
                case 1:
                    SelectActionText[i].color = Player.SelectActionCondition(selectAction.Defend) ? BattleUIManager.Instance.selectedcolor : BattleUIManager.Instance.deselectedcolor;
                    continue;
                case 2:
                    SelectActionText[i].color = Player.SelectActionCondition(selectAction.Skip) ? BattleUIManager.Instance.selectedcolor : BattleUIManager.Instance.deselectedcolor;
                    continue;
                default:
                    SelectActionText[i].color = BattleUIManager.Instance.selectedcolor;
                    break;
            }
        }
        for(int i=0;i<SelectActionArrows.Count;i++)
        {
            SelectActionArrows[i].SetActive(false);
        }

        for (int i = 0; i < AttackCellText.Count; i++)
        {
            AttackCellText[i].color = BattleUIManager.Instance.deselectedcolor;
            AttackCellText[i].text = "X";

            if (i < Player.slotsactivated.Length && Player.slotsactivated[i])
            {
                AttackCellText[i].color = BattleUIManager.Instance.selectedcolor;

                if(Player.chargedmonster[i] != null && Player.chargedmonster[i].id != 0)
                {
                    AttackCellText[i].text = "O";
                }
            }

            if (i == 5)
            {
                AttackCellText[i].text = "O";
                AttackCellText[i].color = Player.ConfirmCondition() ? BattleUIManager.Instance.selectedcolor : BattleUIManager.Instance.deselectedcolor;
            }

            if (i == 6)
            {
                AttackCellText[i].text = "X";
                AttackCellText[i].color = BattleUIManager.Instance.selectedcolor;
            }

        }
        for (int i = 0; i < AttackArrows.Count; i++)
        {
            AttackArrows[i].SetActive(false);
        }
    }

    public void OnActionHover()
    {
        if (Player == null) return;

        DeSelectAllTexts();
        switch(Player.hoveredaction)
        {
            case 1:
                SelectActionArrows[0].SetActive(true);
                break;
            case 2:
                SelectActionArrows[1].SetActive(true);
                break;
            case 3:
                SelectActionArrows[2].SetActive(true);
                break;
        }
    }

    public void OnActionSelected()
    {
        if (Player == null) return;

        DeSelectAllTexts();
        switch (Player.selectaction)
        {
            case selectAction.None:
                SelectAction.SetActive(true);
                BatteryInput.SetActive(false);
                break;
            case selectAction.Skip:
                OnSelectPhaseEnd();
                break;
            case selectAction.Attack:
                SelectAction.SetActive(false);
                BatteryInput.SetActive(true);
                UpdateCalculatedStats();
                OnCellHover();
                break;
            case selectAction.Defend:
                OnSelectPhaseEnd();
                break;
        }
    }

    public void OnCellHover()
    {
        if (Player == null) return;

        DeSelectAllTexts();
        switch (Player.hoveredcell)
        {
            case 1:
                AttackArrows[0].SetActive(true);
                break;
            case 2:
                AttackArrows[1].SetActive(true);
                break;
            case 3:
                AttackArrows[2].SetActive(true);
                break;
            case 4:
                AttackArrows[3].SetActive(true);
                break;
            case 5:
                AttackArrows[4].SetActive(true);
                break;
            case 6:
                AttackArrows[5].SetActive(true);
                break;
            case 7:
                AttackArrows[6].SetActive(true);
                break;
        }

        HandleCellDescription();
    }

    private void HandleCellDescription()
    {
        CellEffectIcon.gameObject.SetActive(false);

        if (Player.hoveredcell > 5)
        {
            switch (Player.hoveredcell)
            {
                case 6:
                    string attacktext = "Attack!";
                    switch (SettingsManager.Instance.data.Language)
                    {
                        case GameLanguage.Japanese:
                            attacktext = "攻撃!";
                            break;
                        case GameLanguage.Mandarin:
                            attacktext = "攻击!";
                            break;
                    }
                    CellDamageBox.text = attacktext;
                    EffectTextBox.text = "";
                    break;
                case 7:
                    string canceltext = "Cancel";
                    switch (SettingsManager.Instance.data.Language)
                    {
                        case GameLanguage.Japanese:
                            canceltext = "キャンセル";
                            break;
                        case GameLanguage.Mandarin:
                            canceltext = "取消";
                            break;
                    }
                    CellDamageBox.text = canceltext;
                    EffectTextBox.text = "";
                    break;
            }
            return;
        }

        string effectdesc = "No additional effects.";
        switch (SettingsManager.Instance.data.Language)
        {
            case GameLanguage.Japanese:
                effectdesc = "追加効果なし.";
                break;
            case GameLanguage.Mandarin:
                effectdesc = "没有其他效果.";
                break;
        }

        if (Player.hoveredcell != 1)
        {
            switch (SettingsManager.Instance.data.Language)
            {
                case GameLanguage.English:
                    if (ChargeEffectsDatabase.Instance.alleffectdescription.TryGetValue(Player.glove.cellmonsters[Player.hoveredcell - 1].element, out Dictionary<int, string> output))
                    {
                        effectdesc = output[Player.glove.cellmonsters[Player.hoveredcell - 1].skillindex];
                    }
                    break;
                case GameLanguage.Japanese:
                    if (ChargeEffectsDatabase.Instance.alleffectdescriptionjp.TryGetValue(Player.glove.cellmonsters[Player.hoveredcell - 1].element, out Dictionary<int, string> JPoutput))
                    {
                        effectdesc = JPoutput[Player.glove.cellmonsters[Player.hoveredcell - 1].skillindex];
                    }
                    break;
                case GameLanguage.Mandarin:
                    if (ChargeEffectsDatabase.Instance.alleffectdescriptioncn.TryGetValue(Player.glove.cellmonsters[Player.hoveredcell - 1].element, out Dictionary<int, string> CNoutput))
                    {
                        effectdesc = CNoutput[Player.glove.cellmonsters[Player.hoveredcell - 1].skillindex];
                    }
                    break;
            }

            CellEffectIcon.sprite = BattleUIManager.Instance.GetEffectSpriteByElement(Player.glove.cellmonsters[Player.hoveredcell - 1].element);
            CellEffectIcon.gameObject.SetActive(true);
        }
        EffectTextBox.font = SettingsManager.Instance.GetLocalizedFont();
        EffectTextBox.text = effectdesc;

        CellDamageBox.text = Player.hoveredcell == 1 ? "DMG +2" : "DMG +1";

    }


    public void OnCellSelected(int hovered)
    {
        if (Player == null) return;

        DeSelectAllTexts();
        AttackArrows[hovered - 1].SetActive(true);
        switch(hovered)
        {
            case 1:

                break;
            case 2:

                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;
            case 6:
                OnSelectPhaseEnd();
                break;
            case 7:
                SelectAction.SetActive(true);
                BatteryInput.SetActive(false);
                OnActionHover();
                break;
        }

        UpdateCalculatedStats();
    }

    private void UpdateCalculatedStats()
    {
        if(DamageCalculateCoroutine != null)
        {
            StopCoroutine(DamageCalculateCoroutine);
        }
        DamageCalculateCoroutine = StartCoroutine(DamageBoxAnimation());

        int numberofcells = 0;

        for(int i=0;i<5;i++)
        {
            if (Player.chargedmonster[i] != null && Player.chargedmonster[i].id != 0)
            {
                numberofcells++;
            }
        }

        CalculatedDamageSlider.value = numberofcells;


        for (int i = 0; i < CalculatedEffectsIcon.Count; i++)
        {
            CalculatedEffectsIcon[i].color = BattleUIManager.Instance.deselectedcolor;
        }

        List<Element> Effectstoadd = new List<Element>();

        for (int i = 1; i < 5; i++)
        {
            if (Player.chargedmonster[i] != null && Player.chargedmonster[i].id != 0)
            {
                Effectstoadd.Add(Player.chargedmonster[i].element);
            }
        }

        if(Effectstoadd.Count > 0)
        {
            for (int i = 0; i < Effectstoadd.Count; i++)
            {
                switch (Effectstoadd[i])
                {
                    case Element.None:
                        break;
                    case Element.Heat:
                        CalculatedEffectsIcon[3].color = BattleUIManager.Instance.selectedcolor;
                        break;
                    case Element.Electric:
                        CalculatedEffectsIcon[4].color = BattleUIManager.Instance.selectedcolor;
                        break;
                    case Element.Wind:
                        CalculatedEffectsIcon[2].color = BattleUIManager.Instance.selectedcolor;
                        break;
                    case Element.Solar:
                        CalculatedEffectsIcon[0].color = BattleUIManager.Instance.selectedcolor;
                        break;
                    case Element.Hydro:
                        CalculatedEffectsIcon[5].color = BattleUIManager.Instance.selectedcolor;
                        break;
                    case Element.Sound:
                        CalculatedEffectsIcon[1].color = BattleUIManager.Instance.selectedcolor;
                        break;
                }
            }
        }
    }

    private IEnumerator DamageBoxAnimation()
    {
        float animationDuration = 0.5f;
        int damage = Player.GetTotalDamage();
        string finalDamage = damage.ToString("00"); // Ensures 2-digit format

        char[] currentDisplay = new char[2];
        bool[] shouldAnimate = new bool[2]; // Track which digits should animate

        // Initialize display and determine which digits to animate
        for (int i = 0; i < 2; i++)
        {
            if (finalDamage[i] != '0')
            {
                currentDisplay[i] = Random.Range(0, 10).ToString()[0];
                shouldAnimate[i] = true; // Mark for animation
            }
            else
            {
                currentDisplay[i] = '0'; // Keep zero static
                shouldAnimate[i] = false;
            }
        }

        float elapsedTime = 0f;
        float interval = 0.05f;
        float nextChangeTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            if (Time.time >= nextChangeTime)
            {
                nextChangeTime = Time.time + interval;

                for (int i = 0; i < 2; i++)
                {
                    // Only animate digits marked for animation
                    if (shouldAnimate[i])
                    {
                        if (elapsedTime > animationDuration * (i + 1) / 3f)
                        {
                            currentDisplay[i] = finalDamage[i]; // Lock in final digit
                        }
                        else
                        {
                            currentDisplay[i] = Random.Range(0, 10).ToString()[0];
                        }
                    }
                }

                CalculatedDamageBox.text = new string(currentDisplay);
            }

            yield return null;
        }

        // Ensure final correct value
        CalculatedDamageBox.text = finalDamage;
    }
}
