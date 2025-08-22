using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIBase : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] protected float controllerMoveThreshold = 0.5f;
    [SerializeField] protected float controllerRepeatDelay = 0.5f;
    [SerializeField] protected float controllerRepeatRate = 0.1f;

    [Header("Initial Selection")]
    [SerializeField] private GameObject firstObjectOnEnable; // Made this serialized field

    protected bool usingController = false;
    protected float lastNavTime;
    protected Vector2 lastInput;

    protected virtual void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        CheckCurrentInputDevice();

        // Always try to select the first object when UI is enabled
        TrySelectFirstObject();
    }

    protected virtual void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void CheckCurrentInputDevice()
    {
        // Check if any controller is connected
        bool controllerConnected = Gamepad.current != null;

        // If controller is connected, we'll assume we're using controller input
        // You might want to make this more sophisticated based on your game's needs
        if (controllerConnected != usingController)
        {
            usingController = controllerConnected;
            OnControlSchemeChanged(usingController);

            // If we just switched to controller, select the first object
            if (usingController)
            {
                TrySelectFirstObject();
            }
        }
    }

    private void TrySelectFirstObject()
    {
        if (!usingController) return;

        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        GameObject selectObject = firstObjectOnEnable;

        // If no specific object is set, try to find the first selectable child
        if (selectObject == null)
        {
            var selectable = GetComponentInChildren<UnityEngine.UI.Selectable>(true);
            if (selectable != null)
            {
                selectObject = selectable.gameObject;
            }
        }

        if (selectObject != null)
        {
            eventSystem.SetSelectedGameObject(selectObject);
            OnUISelectionChanged(selectObject);
        }
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            var inputAction = (InputAction)obj;
            var device = inputAction.activeControl.device;

            bool isControllerOrKeyboard = device is Gamepad || device is Keyboard;
            if (isControllerOrKeyboard != usingController)
            {
                usingController = isControllerOrKeyboard;
                OnControlSchemeChanged(usingController);

                // If we just switched to controller, select the first object
                if (usingController)
                {
                    TrySelectFirstObject();
                }
            }
        }
    }

    protected virtual void Update()
    {
        if (usingController)
        {
            HandleControllerNavigation();
        }
       else if (!usingController)
        {
            HandleMouseNavigation();
        }

    }

    protected virtual void HandleControllerNavigation()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        Vector2 input = gamepad.leftStick.ReadValue();

        // Check if input exceeds threshold
        if (input.magnitude < controllerMoveThreshold)
        {
            lastInput = Vector2.zero;
            return;
        }

        // Check if input direction changed significantly
        bool directionChanged = Vector2.Dot(input.normalized, lastInput.normalized) < 0.8f;
        bool shouldNavigate = directionChanged || (Time.unscaledTime - lastNavTime >
                                  (directionChanged ? controllerRepeatDelay : controllerRepeatRate));

        if (shouldNavigate)
        {
            lastInput = input;
            lastNavTime = Time.unscaledTime;

            NavigateUI(input);
        }
    }
    protected virtual void HandleMouseNavigation()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        Vector2 mousePosition = mouse.position.ReadValue();
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, raycastResults);

        GameObject hoveredObject = null;
        foreach (var result in raycastResults)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Selectable>() != null)
            {
                hoveredObject = result.gameObject;
                break;
            }
        }

        if (hoveredObject != null)
        {
            if (eventSystem.currentSelectedGameObject != hoveredObject)
            {
                eventSystem.SetSelectedGameObject(hoveredObject);
                OnUISelectionChanged(hoveredObject);
            }
        }
        else
        {
            // No selectable under mouse deselect
            if (eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }



    protected void NavigateUI(Vector2 direction)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null || eventSystem.currentSelectedGameObject == null) return;

        var current = eventSystem.currentSelectedGameObject.GetComponent<UnityEngine.UI.Selectable>();
        if (current == null) return;

        UnityEngine.UI.Selectable next = null;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            next = direction.x > 0 ? current.FindSelectableOnRight() : current.FindSelectableOnLeft();
        }
        else
        {
            next = direction.y > 0 ? current.FindSelectableOnUp() : current.FindSelectableOnDown();
        }

        if (next != null)
        {
            eventSystem.SetSelectedGameObject(next.gameObject);
            OnUISelectionChanged(next.gameObject);
        }
    }

    // Virtual methods for derived classes
    protected virtual void OnControlSchemeChanged(bool usingController)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        if (usingController)
        {
            if (eventSystem.currentSelectedGameObject == null)
            {
                var selectable = GetComponentInChildren<UnityEngine.UI.Selectable>();
                if (selectable != null)
                {
                    eventSystem.SetSelectedGameObject(selectable.gameObject);
                }
            }
        }
        else
        {
            var selectable = GetComponentInChildren<UnityEngine.UI.Selectable>();
            if (selectable != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }


    protected virtual void OnUISelectionChanged(GameObject newSelection)
    {
        // Optional: Play sound, animate selection, etc.
    }
}