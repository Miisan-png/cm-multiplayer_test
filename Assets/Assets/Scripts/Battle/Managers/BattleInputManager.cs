using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputManager : MonoBehaviour
{
    private static BattleInputManager instance;
    public static BattleInputManager Instance => instance;

    private InputSystem_Actions playerBattleInput;

    private InputAction confirm;
    private InputAction selectnavigate;
    private InputAction pause;

    public event Action OnConfirm;
    public event Action<Vector2> OnNavigateSelect;
    public event Action OnPause;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        playerBattleInput = new InputSystem_Actions();

        confirm = playerBattleInput.Battle.Confirm;
        selectnavigate = playerBattleInput.Battle.SelectNavigate;
        pause = playerBattleInput.Battle.Pause;

        confirm.performed += ctx => OnConfirm?.Invoke();
        selectnavigate.performed += ctx => OnNavigateSelect?.Invoke(selectnavigate.ReadValue<Vector2>());
        selectnavigate.canceled += ctx => OnNavigateSelect?.Invoke(Vector2.zero);
        pause.performed += ctx => OnPause?.Invoke();
    }

    private void Start()
    {
        BattleManager.Instance.OnBattleStart += () => { playerBattleInput.Battle.Enable(); };
        BattleManager.Instance.OnBattleExit += () => { playerBattleInput.Battle.Disable(); };

        playerBattleInput.Battle.Enable();
    }
}
