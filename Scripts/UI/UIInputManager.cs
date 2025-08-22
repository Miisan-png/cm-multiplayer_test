using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

public class UIInputManager : MonoBehaviour
{
    public static UIInputManager Instance;

    // UI Navigation Events
    public event Action<Vector2> OnNavigate;
    public event Action OnSubmit;
    public event Action OnCancel;
    public event Action OnPause;

    // Dialogue Events (from your original)
    public event Action OnInteract;
    public event Action OnNextInteract;

    private InputSystem_Actions uiInput;
    public InputSystem_Actions uiinput => uiInput;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;
    public InputAction cancelaction => cancelAction;
private InputAction pauseAction;

    // Dialogue actions (from your original)
    private InputAction interactAction;
    private InputAction nextInteractAction;

    [SerializeField] private float navigationRepeatRate = 0.2f;
    private float lastNavigationTime;
    private Vector2 lastNavigationInput;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeInput();
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeInput()
    {
        uiInput = new InputSystem_Actions();

        // UI Navigation Setup
        navigateAction = uiInput.UI.Navigate;
        submitAction = uiInput.UI.Submit;
        cancelAction = uiInput.UI.Cancel;
        pauseAction = uiInput.UI.Pause;

        // Dialogue Setup (from your original)
        interactAction = uiInput.Player.Interact;
        nextInteractAction = uiInput.Player.NextInteract;
 

        // Enable all actions
        navigateAction.Enable();
        submitAction.Enable();
        cancelAction.Enable();
        interactAction.Enable();
        nextInteractAction.Enable();
        pauseAction.Enable();

        navigateAction.canceled += ctx => OnNavigate?.Invoke(Vector2.zero);
        submitAction.performed += ctx => OnSubmit?.Invoke();
        cancelAction.performed += ctx => OnCancel?.Invoke();
        interactAction.performed += ctx => OnInteract?.Invoke();
        nextInteractAction.performed += ctx => OnNextInteract?.Invoke();
        pauseAction.performed += ctx => OnPause?.Invoke();
    }

    private void Update()
    {
        if (navigateAction.IsPressed())
        {
            Vector2 currentInput = navigateAction.ReadValue<Vector2>();

            if (currentInput.magnitude < 0.2f)
            {
                lastNavigationInput = Vector2.zero;
                return;
            }

            if (Time.time - lastNavigationTime >= navigationRepeatRate ||
                currentInput != lastNavigationInput)
            {
                lastNavigationTime = Time.time;
                lastNavigationInput = currentInput;
                OnNavigate?.Invoke(currentInput);
            }
        }
    }

    public void ClearInputs()
    {
        lastNavigationInput = Vector2.zero;
        lastNavigationTime = 0f;
        OnNavigate?.Invoke(Vector2.zero); 
        uiInput.UI.Disable();
        StartCoroutine(InputClearDelay());
    }

    private IEnumerator InputClearDelay()
    {
        yield return null;
        uiInput.UI.Enable();
    }

    public void EnableUIInput()
    {
        uiInput.UI.Enable();
    }

    public void DisableUIInput()
    {
        uiInput.UI.Disable();
    }

}