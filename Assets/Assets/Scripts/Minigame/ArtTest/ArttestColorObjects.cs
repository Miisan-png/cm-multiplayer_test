using System.Collections.Generic;
using UnityEngine;

public class ArttestColorObjects : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> SR;
    [SerializeField] private List<GameObject> Border;
    [SerializeField] private List<Color> ColorsRequired;
    [SerializeField] private bool Correct;
    public bool correct => Correct;


    public void ResetObject()
    {
        Correct = false;

        for (int i = 0; i < SR.Count; i++)
        {
            SR[i].color = Color.white;
        }

        if (Border != null && Border.Count >0)
        {
            for(int i=0;i<Border.Count;i++)
            {
                Border[i].SetActive(false);
            }
        }

        if (ColorsRequired != null && ColorsRequired.Count > 0)
        {
            for (int i = 0; i < ColorsRequired.Count; i++)
            {
                if (SR[0].color == ColorsRequired[i])
                {
                    Correct = true;
                    return;
                }
            }
        }
    }
    
    
    public void ColorObject(Color Color)
    {
        Color colortouse = Color;
        List<ColorCombination> colorcombination = Minigame_ArtTest.Instance.allcolorcombinations;

        if(Color != SR[0].color && SR[0].color != Color.black)
        {
            for (int i = 0; i < colorcombination.Count; i++)
            {
                // Check both possible orderings
                if ((Color == colorcombination[i].color2 && SR[0].color == colorcombination[i].color1) ||
                    (Color == colorcombination[i].color1 && SR[0].color == colorcombination[i].color2))
                {
                    colortouse = colorcombination[i].color3;
                    break;
                }
            }

            for (int i = 0; i < SR.Count; i++)
            {
                SR[i].color = colortouse;
            }
        }

        if (ColorsRequired != null && ColorsRequired.Count >0)
        {
            Correct = false;    
            for (int i = 0; i < ColorsRequired.Count; i++)
            {
                if (colortouse == ColorsRequired[i])
                {
                    Correct = true;
                    return;
                }
            }
        }
        else if(colortouse != Color.white)
        {
            Correct = true;
        }


        if (Border != null && Border.Count > 0)
        {
            for (int i = 0; i < Border.Count; i++)
            {
                Border[i].SetActive(true);
            }
        }
    }

}
