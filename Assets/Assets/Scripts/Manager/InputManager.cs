using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public event Action OnInteract;
    public event Action OnNextInteract;
    public event Action<Vector2> OnMove;
    public event Action OnMoveStop;
    public event Action<int> OnClimb;
    public event Action<float> OnSwim;
    public event Action<bool> OnRun;
    public event Action OnCastAnchor;
    public event Action OnCastConfirm;
    public event Action<Vector2> OnMoveBoat;
    public event Action OnBoatDrift;
    public event Action OnDriftRelease;
    public event Action<bool> OnThrottle;
    public event Action<bool> OnSwitchFrontCam;
    public event Action OnPetLaunch;

    private InputSystem_Actions playerInput;

    private InputAction interactAction;
    private InputAction nextInteractAction;
    private InputAction moveAction;
    public InputAction moveaction => moveAction;
    private InputAction climbAction;
    private InputAction swimAction;
    private InputAction runButtonAction;
    public InputAction runbuttonaction => runButtonAction;
    private InputAction CastAction;
    private InputAction CastConfirmAction;

    private InputAction LaunchAction;

    private InputAction BoatMoveAction;
    private InputAction BoatDriftAction;
    private InputAction BoatThrottleAction;
    private InputAction BoatCamSwitchAction;


    private void Start()
    {
        UIManager.Instance.onDialogueOn += () => { ActivateInputs(false); };
        UIManager.Instance.onDialogueOff += () => { ActivateInputs(true); };

        UIManager.Instance.onScreenTransitionOn += () => { ActivateInputs(false); };
        UIManager.Instance.onScreenTransitionOff += () => { ActivateInputs(true); };

        ActivateInputs(true);
        playerInput.Minigame_Fishing.Enable();
        playerInput.Minigame_BoatRace.Enable();
    }

    public void ActivateInputs(bool activation)
    {
        if(activation)
        {
            playerInput.Player.Enable();
        }
        else
        {
            playerInput.Player.Disable();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerInput = new InputSystem_Actions();

            interactAction = playerInput.Player.Interact;
            moveAction = playerInput.Player.Move;
            climbAction = playerInput.Player.Climb;
            swimAction = playerInput.Player.Swim;
            nextInteractAction = playerInput.Player.NextInteract;
            runButtonAction = playerInput.Player.RunButton;
            CastAction = playerInput.Minigame_Fishing.Cast;
            CastConfirmAction = playerInput.Minigame_Fishing.Confirm;
            BoatMoveAction = playerInput.Minigame_BoatRace.Move;
            BoatDriftAction = playerInput.Minigame_BoatRace.Drift;
            BoatThrottleAction = playerInput.Minigame_BoatRace.Throttle;
            BoatCamSwitchAction = playerInput.Minigame_BoatRace.SwitchCam;
            LaunchAction = playerInput.Player.LaunchButton;

            interactAction.performed += ctx => OnInteract?.Invoke();
            nextInteractAction.performed += ctx => OnNextInteract?.Invoke();
            moveAction.performed += ctx => OnMove?.Invoke(moveAction.ReadValue<Vector2>());
            moveAction.canceled += ctx => OnMoveStop?.Invoke();
            climbAction.performed += ctx => OnClimb?.Invoke((int)ctx.ReadValue<float>());
            climbAction.canceled += ctx => OnClimb?.Invoke(0);
            swimAction.performed += ctx => OnSwim?.Invoke(ctx.ReadValue<float>());
            swimAction.canceled += ctx => OnSwim?.Invoke(0f);
            runButtonAction.performed += ctx => OnRun?.Invoke(true);
            runButtonAction.canceled += ctx => OnRun?.Invoke(false);
            CastAction.performed += ctx => OnCastAnchor?.Invoke();
            CastConfirmAction.performed += ctx => OnCastConfirm?.Invoke();
            BoatMoveAction.performed += ctx => OnMoveBoat?.Invoke(BoatMoveAction.ReadValue<Vector2>());
            BoatMoveAction.canceled += ctx => OnMoveBoat?.Invoke(new Vector2(0f,0f));
            BoatDriftAction.performed += ctx => OnBoatDrift?.Invoke();
            BoatDriftAction.canceled += ctx => OnDriftRelease?.Invoke();
            BoatThrottleAction.performed += ctx => OnThrottle?.Invoke(true);
            BoatThrottleAction.canceled += ctx => OnThrottle?.Invoke(false);
            BoatCamSwitchAction.performed += ctx => OnSwitchFrontCam?.Invoke(true);
            BoatCamSwitchAction.canceled += ctx => OnSwitchFrontCam?.Invoke(false);
            LaunchAction.performed += ctx => OnPetLaunch?.Invoke();

            //Boat inputs will need to be moved out to another PlayerInputManager for multiplayer handling, Singleton Instances will clash in multiplayer case.
        }
        else
        {
            Destroy(gameObject);
        }
    }
}