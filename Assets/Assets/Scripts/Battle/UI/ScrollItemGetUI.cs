using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;

public class ScrollItemGetUI : MonoBehaviour
{
    private void Start()
    {
        BattleInputManager.Instance.OnNavigateSelect += (Vector2 _input) => { 
            input = _input; 
            if(input.y == 0)
            {
                OnValueReset();
            }
        }; 
    }

    [SerializeField] private Image UpRenderer;
    [SerializeField] private Image DownRenderer;
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color DeSelectedColor;
    [SerializeField] private Vector2 input;
    [SerializeField] private GameObject SelectedObject;
    [SerializeField] private GameObject ScrollBar;
    public void OnValueChanged(Vector2 input)
    {
        // Handle visual feedback
        if (input.y > 0.1f)
        {
            UpRenderer.color = SelectedColor;
            DownRenderer.color = DeSelectedColor;
        }
        else if (input.y < -0.1f)
        {
            UpRenderer.color = DeSelectedColor;
            DownRenderer.color = SelectedColor;
        }
    }

    public void OnValueReset()
    {
        UpRenderer.color = SelectedColor;
        DownRenderer.color = SelectedColor;
    }

    private void Update()
    {
        if (input.y != 0 && EventSystem.current.currentSelectedGameObject == ScrollBar)
        {
            OnValueChanged(input);
        }
    }
}