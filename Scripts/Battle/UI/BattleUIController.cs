using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    [SerializeField] private BattleController Controller;
    [SerializeField] private GloveController GloveController;
    public GloveController glovecontroller => GloveController;
    [SerializeField] private SelectActionUI SelectactionUI;
    public SelectActionUI selectactionui => SelectactionUI;

    [SerializeField] private GameObject PlayerStatsUI;
    [SerializeField] private GameObject SelectParent;

    [SerializeField] private Slider HealthSlider1; // 50% of health
    [SerializeField] private Slider HealthSlider2; // another 50% of health
    [SerializeField] private TextMeshProUGUI HealthText;
    [SerializeField] private float sliderTimer;
    private Coroutine HealhSliderCoroutine;

    [SerializeField] private List<CellIconUI> Cells;

    [SerializeField] private List<Image> EffectsIcon;
    [SerializeField] private List<TextMeshProUGUI> EffectsText;

    public void OnStartPhase()
    {
        SelectParent.SetActive(true);
        SelectactionUI.OnSelectPhaseEnd();
    }

    public void OnRollPhase()
    {
        SelectParent.SetActive(false);
        bool isplayer = Controller is BattleController_Player ? true : false;
        UpdateCells(isplayer);
    }

    public void OnSelectPhase()
    {
        SelectParent.SetActive(true);
        bool isplayer = Controller is BattleController_Player ? true : false;
        UpdateCells(isplayer);
        SelectactionUI.OnSelectPhaseStart();


        for(int i=0;i<5;i++)
        {
            Cells[i].ResetTopPosition();
        }
    }

    public void OnExecutionPhase()
    {
        SelectParent.SetActive(false);
        SelectactionUI.OnSelectPhaseEnd();
    }

    public void HideAllIdentity() //Must be called after icons are initialized
    {
        for (int i = 0; i < 5; i++)
        {
            Cells[i].HideIdentity();
        }

        SelectactionUI.HideIdentity();

    }

    public void InitializeUI(BattleController C)
    {
        Controller = C;
        PlayerStatsUI.SetActive(true);
        GloveController.InitializeController(Controller);

        if(Controller is BattleController_Player player)
        {
            SelectactionUI.InitializeController(Controller as BattleController_Player);
        }

        for (int i = 0; i < Cells.Count; i++)
        {
            Cells[i].gameObject.SetActive(false);
        }

        for (int i=0;i<5;i++)
        {
            if (Controller.glove.cellmonsters[i]!= null && Controller.glove.cellmonsters[i].id != 0)
            {
                Cells[i].InitializeIcon(Controller.glove.cellmonsters[i],i);
                Cells[i].gameObject.SetActive(true);
            }
        }

        UpdateEffectsIcon();
    }


    public void UpdateEffectsIcon()
    {
        for (int i = 0; i < 6; i++)
        {
            EffectsIcon[i].color = BattleUIManager.Instance.deselectedcolor;
            EffectsText[i].color = BattleUIManager.Instance.deselectedcolor;
        }

        Dictionary<ChargeEffects, int> effects = Controller.effects;

        var keys = effects.Keys.ToList();

        foreach (var key in keys)
        {
            if (effects[key] <= 0) continue;

            switch (key.ElementID)
            {
                case Element.None:
                    break;
                case Element.Heat:
                    EffectsIcon[3].color = BattleUIManager.Instance.selectedcolor;
                    EffectsText[3].color = BattleUIManager.Instance.selectedcolor;
                    break;
                case Element.Electric:
                    EffectsIcon[4].color = BattleUIManager.Instance.selectedcolor;
                    EffectsText[4].color = BattleUIManager.Instance.selectedcolor;
                    break;
                case Element.Wind:
                    EffectsIcon[2].color = BattleUIManager.Instance.selectedcolor;
                    EffectsText[2].color = BattleUIManager.Instance.selectedcolor;
                    break;
                case Element.Solar:
                    EffectsIcon[0].color = BattleUIManager.Instance.selectedcolor;
                    EffectsText[0].color = BattleUIManager.Instance.selectedcolor;
                    break;
                case Element.Hydro:
                    EffectsIcon[5].color = BattleUIManager.Instance.selectedcolor;
                    EffectsText[5].color = BattleUIManager.Instance.selectedcolor;
                    break;
                case Element.Sound:
                    EffectsIcon[1].color = BattleUIManager.Instance.selectedcolor;
                    EffectsText[1].color = BattleUIManager.Instance.selectedcolor;
                    break;
            }
        }
    }

    public void UpdateCells(bool playanimation)
    {
        for (int i = 0; i < 5; i++)
        {
            if (Controller.slotsactivated[i])
            {
                Cells[i].OnCellActivated();
            }
            else if (Controller.glove.cellmonsters[i] != null && Controller.glove.cellmonsters[i].id != 0)
            {
                Cells[i].OnCellDeactivated();
            }

            if (playanimation)
            {
                if (Controller.chargedmonster.Contains(Cells[i].monster))
                {
                    Cells[i].OnCellCharge(true);
                }
                else
                {
                    Cells[i].OnCellCharge(false);
                }
            }
        }
    }


    public void UpdateHealthUI()
    {
        if(HealhSliderCoroutine != null)
        {
            StopCoroutine(HealhSliderCoroutine);
        }
        HealhSliderCoroutine = StartCoroutine(HealthSlider());
    }

    public void ResetHealthUI()
    {
        HealthSlider1.value = 0;
        HealthSlider2.value = 0;
        HealthText.text = "000";
    }

    private IEnumerator HealthSlider()
    {
        yield return new WaitForSecondsRealtime(1f);

        if (Controller == null)
        {
            yield break;
        }

        int slidercurrent = (int)HealthSlider1.value + (int)HealthSlider2.value;
        int currentvalue = Controller.hp;
        int textvalue = Mathf.Abs(slidercurrent);

        bool slider1 = false;
        bool slider2 = false;

        // Calculate speed multiplier based on difference
        float speedMultiplier = (Mathf.Abs(slidercurrent - currentvalue) > 50) ? 2f : 1f;
        float timer = sliderTimer / speedMultiplier; // Apply speed boost to timer

        int slider1Value = Mathf.Clamp(currentvalue, 0, 50);
        int slider2Value = Mathf.Clamp(currentvalue - 50, 0, 50);

        while (true)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                int increment = (int)(1 * speedMultiplier); // Adjust increment based on speed

                if (slidercurrent < currentvalue)
                {
                    if (HealthSlider1.value < slider1Value)
                    {
                        HealthSlider1.value = Mathf.Min(HealthSlider1.value + increment, slider1Value);
                    }
                    else if (HealthSlider1.value > slider1Value)
                    {
                        HealthSlider1.value = Mathf.Max(HealthSlider1.value - increment, slider1Value);
                    }
                    else
                    {
                        slider1 = true;
                    }

                    if (slider1)
                    {
                        if (HealthSlider2.value < slider2Value)
                        {
                            HealthSlider2.value = Mathf.Min(HealthSlider2.value + increment, slider2Value);
                        }
                        else if (HealthSlider2.value > slider2Value)
                        {
                            HealthSlider2.value = Mathf.Max(HealthSlider2.value - increment, slider2Value);
                        }
                        else
                        {
                            slider2 = true;
                        }
                    }

                    if (textvalue < currentvalue)
                    {
                        textvalue = Mathf.Min(textvalue + increment, currentvalue);
                        HealthText.text = textvalue.ToString("D3");
                    }
                }
                else // Decreasing health
                {
                    if (HealthSlider2.value < slider2Value)
                    {
                        HealthSlider2.value = Mathf.Min(HealthSlider2.value + increment, slider2Value);
                    }
                    else if (HealthSlider2.value > slider2Value)
                    {
                        HealthSlider2.value = Mathf.Max(HealthSlider2.value - increment, slider2Value);
                    }
                    else
                    {
                        slider2 = true;
                    }

                    if (slider2)
                    {
                        if (HealthSlider1.value < slider1Value)
                        {
                            HealthSlider1.value = Mathf.Min(HealthSlider1.value + increment, slider1Value);
                        }
                        else if (HealthSlider1.value > slider1Value)
                        {
                            HealthSlider1.value = Mathf.Max(HealthSlider1.value - increment, slider1Value);
                        }
                        else
                        {
                            slider1 = true;
                        }
                    }

                    if (textvalue > currentvalue)
                    {
                        textvalue = Mathf.Max(textvalue - increment, currentvalue);
                        HealthText.text = textvalue.ToString("D3");
                    }
                }

                if (slider1 && slider2)
                {
                    HealthSlider1.value = slider1Value;
                    HealthSlider2.value = slider2Value;
                    HealthText.text = currentvalue.ToString("D3");
                    yield break;
                }

                timer = sliderTimer / speedMultiplier; // Reset timer with speed boost
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void OnBattleEnd()
    {
        glovecontroller.OnBattleEnd();
    }

    public void OnBattleExit()
    {
        ResetHealthUI();
        PlayerStatsUI.SetActive(false);
        GloveController.UninitializeController();
        Controller = null;
    }

}
