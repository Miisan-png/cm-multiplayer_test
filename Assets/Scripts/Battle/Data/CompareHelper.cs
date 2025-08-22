public static class CompareHelper
{
    public static bool ElementBeats(Element attacker, Element defender)
    {
        // Sound only beats itself
        if (attacker == Element.Sound && defender == Element.Sound)
            return true;

        // Fire > Electric > Air > Light > Water > Fire
        switch (attacker)
        {
            case Element.Heat:
                return defender == Element.Electric;
            case Element.Electric:
                return defender == Element.Wind;
            case Element.Wind:
                return defender == Element.Solar;
            case Element.Solar:
                return defender == Element.Hydro;
            case Element.Hydro:
                return defender == Element.Heat;
            default:
                return false;
        }
    }


    public static int Compare(Element a, Element b)
    {
        if (a == b) return 0;              // Draw
        if (ElementBeats(a, b)) return 1;         // a wins
        return -1;                         // b wins
    }

    public static bool ElementMatchesInt(int number,Element element)
    {
        bool condition = false;

        switch (element)
        {
            case Element.None:
                break;
            case Element.Heat:

                if(number == 5)
                {
                    condition = true;
                }

                break;
            case Element.Electric:

                if (number == 2)
                {
                    condition = true;
                }

                break;
            case Element.Wind:

                if (number == 6)
                {
                    condition = true;
                }

                break;
            case Element.Solar:

                if (number == 7)
                {
                    condition = true;
                }

                break;
            case Element.Hydro:

                if (number == 3)
                {
                    condition = true;
                }

                break;
            case Element.Sound:

                if (number == 8)
                {
                    condition = true;
                }

                break;
        }
        return condition;
    }

    public static Element GetElementFromNumber(int number)
    {
        Element element = Element.None;

        switch (number)
        {
            case 2:
                element = Element.Electric;
                break;
            case 3:
                element = Element.Hydro;
                break;
            case 5:
                element = Element.Heat;
                break;
            case 6:
                element = Element.Wind;
                break;
            case 7:
                element = Element.Solar;
                break;
            case 8:
                element = Element.Sound;
                break;
        }
        return element;
    }
    public static int GetNumberfromElement(Element E)
    {
        int number = 1;

        float randomNumber = UnityEngine.Random.Range(0f, 1f);

        if (randomNumber < 0.3f)
        {
            number = 4;
        }
        else if (randomNumber < 0.6f)
        {
            number = 9;
        }

        switch (E)
        {
            case Element.Heat:
                number = 5;
                break;
            case Element.Electric:
                number = 2;
                break;
            case Element.Wind:
                number = 6;
                break;
            case Element.Solar:
                number = 7;
                break;
            case Element.Hydro:
                number = 3;
                break;
            case Element.Sound:
                number = 8;
                break;
        }
        return number;
    }
}
