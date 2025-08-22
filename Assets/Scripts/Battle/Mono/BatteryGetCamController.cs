using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class BatteryGetCamController : MonoBehaviour
{
    private void Start()
    {
        BattleInputManager.Instance.OnNavigateSelect += (Vector2 _input) => input = _input;
    }
    [SerializeField] private CinemachinePanTilt Pantilt;
    [SerializeField] private Vector2 input;
    [SerializeField] private GameObject ScrollBar;
    public void OnValueChanged()
    {
        Pantilt.PanAxis.Value += 1.5f;
    }
    private void FixedUpdate()
    {
        if (input.y != 0 && EventSystem.current.currentSelectedGameObject == ScrollBar)
        {
            OnValueChanged();
        }
    }

}
