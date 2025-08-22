using TMPro;
using UnityEngine;

public class UI_PopUp : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TextBox;

    public void setText(string t)
    {
        TextBox.text = t;
    }

    public void onAnimEnd()
    {
        Destroy(gameObject);
    }
}
