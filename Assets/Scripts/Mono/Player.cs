using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Player : MonoBehaviour
{
    private void Awake()
    {
        setstate(PlayerState.Airborne);
    }
    private void Start()
    {
        currentMoveSpeed = WalkSpeed;

        InputManager.Instance.OnInteract += HandleInteract;
        InputManager.Instance.OnMove += HandleMovement;
        InputManager.Instance.OnMoveStop += ResetStick;
        InputManager.Instance.OnMoveStop += stopMovement;
        InputManager.Instance.OnClimb += HandleClimb;
        InputManager.Instance.OnRun += HandleRun;
        CameraManager.Instance.onCameraChange += OnCameraChanged;

        GDetector.onLeaveGround += SetOffGround;
        GDetector.onStayGround += StayGrounded;
        GDetector.onEnterGround += TouchGround;
    }

    [SerializeField] private float RunSpeed;
    [SerializeField] private float WalkSpeed;
    [SerializeField] private float currentMoveSpeed;
    [SerializeField] private float DownhillSpeedReduction;
    [SerializeField] private float fallSpeed;

    [SerializeField] private float moveDirectionX;
    [SerializeField] private float moveDirectionZ;
    [SerializeField] private Vector2 cacheDirection;

    [SerializeField] private float turnSpeed;
    [SerializeField] private float sinkSpeed;

    [SerializeField] private Rigidbody rb;
    private RigidbodyConstraints rbConstraints = RigidbodyConstraints.FreezeRotation;
    private RigidbodyConstraints rbConstraintsClimbing = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private Collider playerTallCollider;
    [SerializeField] private float cachedRadius;
    [SerializeField] private float walkColliderRadius;
    [SerializeField] private float runColliderRadius;
    [SerializeField] private float rotationDirection;
    private Tween PosLerpingTween;

    [SerializeField] private int climbingDirection;
    [SerializeField] private float climbingSpeed;

    [SerializeField] private float swimSpeed;
    [SerializeField] private float buoyancy;
    [SerializeField] private float swimY;
    [SerializeField] private float swimDirectionX;
    [SerializeField] private float swimDirectionZ;

    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isRunning;
    public bool isrunning => isRunning;

    [SerializeField] private PlayerGroundDetector GDetector;
    [SerializeField] private PlayerInteractableDetector IDetector;
    public PlayerInteractableDetector idetector => IDetector;
    [SerializeField] private PlayerWaterDetector WDetector;
    [SerializeField] private PlayerInteractionBubble IBubble;
    public PlayerInteractionBubble bubble => IBubble;

    [SerializeField] private Vector3 _currentCameraForward;
    [SerializeField] private Vector3 _currentCameraRight;
    [SerializeField] private bool updatedCamera;

    [SerializeField] private float directionThreshold = 0.05f;

    public PlayerState state;
    private Action StateAction;

    public delegate void whileMoving();
    public event whileMoving WhileMoving;

    public Action OnTeleport;
    public Action<PlayerState> OnStateChanged;
    public Action<bool> OnRun;
    public Action OnInteract;
    public Action OnClimbLadder;
    public Action OnOffLadder;

    [Header("Slope Settings")]
    [SerializeField] private float baseGravity = 2f;
    [SerializeField] private float uphillGravityMultiplier = 0.8f; // Reduce gravity when going uphill
    [SerializeField] private float downhillGravityMultiplier = 1.3f; // Increase gravity when going downhill
    [SerializeField] private float maxSlopeAngle = 45f;

    private void OnDestroy()
    {
        InputManager.Instance.OnInteract -= HandleInteract;
        InputManager.Instance.OnMove -= HandleMovement;
        InputManager.Instance.OnMoveStop -= ResetStick;
        InputManager.Instance.OnMoveStop -= stopMovement;
        InputManager.Instance.OnClimb -= HandleClimb;
        InputManager.Instance.OnRun -= HandleRun;
        CameraManager.Instance.onCameraChange -= OnCameraChanged;

        GDetector.onLeaveGround -= SetOffGround;
        GDetector.onStayGround -= StayGrounded;
        GDetector.onEnterGround -= TouchGround;
    }

    public void setstate(PlayerState _state)
    {
        if (state == _state)
        {
            return;
        }

        state = _state;

        switch (state)
        {
            case PlayerState.None:
                rb.constraints = rbConstraints;
                rb.linearVelocity = new Vector3(0, 0, 0);
                rb.angularVelocity = new Vector3(0, 0, 0);
                isGrounded = false;
                StateAction = () => { rb.linearVelocity = new Vector3(0, 0, 0); };
                break;
            case PlayerState.Idle:
                rb.constraints = rbConstraints;
                rb.linearVelocity = new Vector3(0, 0, 0);
                StateAction = ()=> { rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); };
                break;
            case PlayerState.Walk:
                if(InputManager.Instance.runbuttonaction.IsPressed())
                {
                    HandleRun(true);
                }
                rb.constraints = rbConstraints;
                rb.isKinematic = false;
                StateAction = Movement;
                break;
            case PlayerState.Climbing:
                rb.isKinematic = false;
                rb.constraints = rbConstraintsClimbing;
                rb.linearVelocity = new Vector3(0, 0, 0);
                rb.angularVelocity = new Vector3(0, 0, 0);
                isGrounded = false;
                StateAction = Climb;
                break;
            case PlayerState.Airborne:
                rb.constraints = rbConstraints;
                rb.isKinematic = false;
                StateAction = Fall;
                break;
            case PlayerState.Swimming:

                break;

        }

        OnStateChanged?.Invoke(state);
    }

    private bool ShouldIgnoreCollision()
    {
        return
        state == PlayerState.None || state == PlayerState.Climbing || state == PlayerState.Swimming;
    }


    private void StayGrounded()
    {
        if (ShouldIgnoreCollision())
        {
            return;
        }

        isGrounded = true;
    }

    private void TouchGround()
    {
        if(moveDirectionZ != 0 || moveDirectionX != 0)
        {
            setstate(PlayerState.Walk);
        }
        else
        {
            setstate(PlayerState.Idle);
        }

    }

    private void SetOffGround()
    {
        if (ShouldIgnoreCollision())
        {
            return;
        }

        isGrounded = false;

        stopMovement();
    }
    private float GetSlopeAdjustedGravity(Vector3 movementDirection)
    {
        float RunningGravity = isRunning ? 2f : 0f;

        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f))
            return baseGravity + RunningGravity;

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle > maxSlopeAngle) return baseGravity;

        // Determine if we're moving uphill or downhill
        float slopeDirection = Vector3.Dot(movementDirection.normalized, hit.normal);

        if (slopeDirection < -0.1f) // Moving uphill
            return baseGravity * uphillGravityMultiplier + RunningGravity;
        else if (slopeDirection > 0.1f) // Moving downhill
            return baseGravity * downhillGravityMultiplier + RunningGravity;

        return baseGravity + RunningGravity;
    }

    private void Fall()
    {
        Vector3 fallspeed = new Vector3(0, rb.linearVelocity.y - baseGravity, 0);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, fallspeed, fallSpeed);
    }

    public void OnPetPickUp()
    {
        playerTallCollider.enabled = true;
        playerCollider.enabled = false;
    }

    public void OnDropPet()
    {
        playerCollider.enabled = true;
        playerTallCollider.enabled = false;
    }

    public void OnThrowPet()
    {
        StartCoroutine(ThrowPetAnimation());
    }

    private IEnumerator ThrowPetAnimation()
    {
        setstate(PlayerState.None);
        OnDropPet();

        float timer = 0.5f;

        while(timer >0)
        {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (moveDirectionZ != 0 || moveDirectionX != 0)
        {
            setstate(PlayerState.Walk);
        }
        else
        {
            setstate(PlayerState.Idle);
        }
    }

    private bool InteractCondition()
    {
        return IDetector.HoverInteractable != null && !IDetector.HoverInteractable.interacted && IDetector.HoverInteractable._Interactable && state != PlayerState.None && state != PlayerState.Climbing;
    }
    private void HandleInteract()
    {
        if (!InteractCondition()) return;
        OnInteract?.Invoke();
        IDetector.StartHoverInteraction();
        Debug.Log("Interacted");
    }

    private bool CheckforStickChange(Vector2 d)
    {
        return
        Vector2.Distance(cacheDirection, d) < directionThreshold;
    }

    private void HandleMovement(Vector2 direction)
    {
        if(!CheckforStickChange(direction))
        {
            cacheDirection = new Vector2(0, 0);
            UpdateCameraVectors();
        }

        setMovement(direction.x, direction.y);
    }

    private void HandleClimb(int direction)
    {
        setClimb(direction);
    }


    private void HandleRun(bool run)
    {
        StartColliderScale(run);

        if (state != PlayerState.Walk)
        {
            currentMoveSpeed = WalkSpeed;
            isRunning = false;
            return;
        }
        switch (run)
        {
            case true:
                currentMoveSpeed = RunSpeed;
                isRunning = true;
                break;
            case false:
                currentMoveSpeed = WalkSpeed;
                isRunning = false;
    
                break;
        }
        OnRun?.Invoke(run);
    }

    private void StartColliderScale(bool run)
    {
        if(state != PlayerState.Walk)
        {
            CapsuleCollider collider1 = playerCollider as CapsuleCollider;
            cachedRadius = collider1.radius;
            DOTween.To(() => cachedRadius, x => cachedRadius = x, walkColliderRadius, 0.4f).OnUpdate(UpdateRadius);
            return;
        }

        float newradius = run ? runColliderRadius : walkColliderRadius;
        cachedRadius = run ? walkColliderRadius : runColliderRadius;

        DOTween.To(() => cachedRadius, x => cachedRadius = x, newradius, 0.4f).OnUpdate(UpdateRadius);
    }

    private void UpdateRadius()
    {
        CapsuleCollider collider1 = playerCollider as CapsuleCollider;
        CapsuleCollider collider2 = playerTallCollider as CapsuleCollider;

        collider1.radius = cachedRadius;
        collider2.radius = cachedRadius;
    }

    private void setMovement(float moveX,float moveZ)
    {
        moveDirectionX = moveX;
        moveDirectionZ = moveZ;

        if(MovementCondition())
        {
            setstate(PlayerState.Walk);
        }

    }

    private bool StopMovementCondition()
    {
        return (state != PlayerState.None && state != PlayerState.Climbing && state != PlayerState.Swimming);
    }

    private void ResetStick()
    {
        moveDirectionX = 0;
        moveDirectionZ = 0;
    }

    private void stopMovement()
    {
        UpdateCameraVectors();

        if (isGrounded && StopMovementCondition())
        {
            setstate(PlayerState.Idle);
        }
        else if(StopMovementCondition())
        {
            setstate(PlayerState.Airborne);
        }
    }

    private bool UpdateCameraCondition()
    {
        return
        state != PlayerState.None && state != PlayerState.Climbing && state != PlayerState.Swimming;
    }


    private void UpdateCameraVectors()
    {
        if(UpdateCameraCondition())
        {
            Transform T = Camera.main.transform;
            if (CameraManager.instance.controlforward != null)
            {
                T = CameraManager.instance.controlforward.transform;
            }
            // Get camera forward and right vectors, ignoring Y axis
            _currentCameraForward = T.forward;
            _currentCameraForward.y = 0;
            _currentCameraForward.Normalize();

            _currentCameraRight = T.right;
            _currentCameraRight.y = 0;
            _currentCameraRight.Normalize();

            updatedCamera = true;
        }
    }

    private bool MovementCondition()
    {
        return isGrounded &&
             state != PlayerState.None &&
             state != PlayerState.Climbing &&
             state != PlayerState.Swimming;
    }
    private Vector3 CalculateNormalMovement()
    {
        return (_currentCameraForward * moveDirectionZ) + (_currentCameraRight * moveDirectionX);
    }

    private Vector3 CalculateCameraForwardMovement()
    {
        return (_currentCameraForward * 1); // Forward only
    }

    private void Movement()
    {
        if (MovementCondition())
        {
            Vector3 moveDirection = updatedCamera
                ? CalculateNormalMovement()
                : CalculateCameraForwardMovement();

            // Get adaptive gravity based on slope direction
            float currentGravity = GetSlopeAdjustedGravity(moveDirection);

            moveDirection = moveDirection.normalized * currentMoveSpeed;

            rb.linearVelocity = new Vector3(
                    moveDirection.x,
                    rb.linearVelocity.y - currentGravity,
                    moveDirection.z
                );

            HandleRotation(moveDirection);
            WhileMoving?.Invoke();
        }
    }

    //Rotate Player
    private void HandleRotation(Vector3 MoveDirection)
    {
        if (moveDirectionX != 0 || moveDirectionZ != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );
        }
    }

    //Look at a target
    public void HandleRotationTowards(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0; // Optional: Keep rotation horizontal (ignore Y-axis)

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );
        }
    }
    private void OnCameraChanged()
    {
        cacheDirection = new Vector2(moveDirectionX, moveDirectionZ);
        _currentCameraForward = CameraManager.instance.currentdetector.spawnpoint.forward ;
        _currentCameraRight = CameraManager.instance.currentdetector.spawnpoint.right ;
        updatedCamera = false;
    }

    private void setClimb(int climbD)
    {
        climbingDirection = climbD;
    }

    private void Climb()
    {
        rb.linearVelocity = new Vector3(0, climbingSpeed * climbingDirection, 0);
    }

    public void startLerp(Transform lerppos, PlayerState state,float duration)
    {
        if(PosLerpingTween != null)
        {
            PosLerpingTween.Kill();
        }

        PosLerpingTween = rb.DOMove(lerppos.position, duration).OnComplete(()=> { setstate(state); });
    }
    public void startLerp(Transform lerppos, PlayerState state, float duration,Action actiononend)
    {
        if (PosLerpingTween != null)
        {
            PosLerpingTween.Kill();
        }

        PosLerpingTween = rb.DOMove(lerppos.position, duration).OnComplete(() => { setstate(state); actiononend(); });
    }
    public void Teleport(Transform T)
    {
        setstate(PlayerState.None);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = new Vector3(T.position.x, T.position.y, T.position.z);
        transform.position = new Vector3(transform.position.x, GDetector.GetGroundPosition(transform.position.y), transform.position.z);
        transform.rotation = T.rotation;

        if (moveDirectionZ != 0 || moveDirectionX != 0)
        {
            setstate(PlayerState.Walk);
        }
        else
        {
            setstate(PlayerState.Airborne);
        }

        OnTeleport?.Invoke();
    }
    private void FixedUpdate()
    {
        if (StateAction != null)
        {
            StateAction();
        }
    }
}
public enum PlayerState
{
    Idle, Walk, Climbing, Airborne, Swimming, None
}
