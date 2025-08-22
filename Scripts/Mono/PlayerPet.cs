using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPet : MonoBehaviour
{
    [SerializeField] private Player PlayertoFollow;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject PlayerBack;
    [SerializeField] private GameObject PlayerPickUp;
    [SerializeField] private GameObject PlayerBag;

    private float AccelerationSpeed = 10f;
    [SerializeField] private float WalkSpeed => PlayertoFollow.isrunning ? 13f : 9f;
    [SerializeField] private float SlowDownSpeed;

    [SerializeField] public bool Grounded;
    [SerializeField] private bool Launched;
    [SerializeField] private bool Teleporting;
    public bool launched => Launched;
    [SerializeField] private Animator animator;
    [SerializeField] private PetState State;

    public PetState state => State;
    private Action StateAction;
    [SerializeField] private bool isRunning => PlayertoFollow.isrunning;

    [SerializeField] private Interactable interactable;
    [SerializeField] private Interactable pickupinteractable;
    [SerializeField] private Collider CollisionCollider;
    [SerializeField] private Collider TriggerCollider;
    private Coroutine TeleportExitCoroutine;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask trackLayer;
    private RaycastHit groundHit;

    [Header("Slope Settings")]
    [SerializeField] private float baseGravity = 1.5f;
    [SerializeField] private float uphillGravityMultiplier = 0.5f; // Reduce gravity when going uphill
    [SerializeField] private float downhillGravityMultiplier = 2.5f; // Increase gravity when going downhill
    [SerializeField] private float maxSlopeAngle = 45f;

    [Header("Launch Settings")]
    [SerializeField] private float LaunchForceHorizontal = 15f;
    [SerializeField] private float LaunchForceVertical = 15f;
    [SerializeField] private LayerMask pickupBlockingLayers;
    private Coroutine launchCoroutine;

    private bool ChangeStateCondition(PetState S)
    {
        return S != State;
    }

    public void SetState(PetState S)
    {
        if (!ChangeStateCondition(S)) return;
        State = S;

        switch (State)
        {
            case PetState.None:
                animator.SetBool("IsWalking", false);
                StateAction = null;
                break;
            case PetState.Idle:
                animator.SetBool("IsWalking", false);
                StateAction = HandleFall;
                break;
            case PetState.Walking:
                animator.SetBool("IsWalking", true);
                StateAction = () => { animator.SetBool("IsRunning", isRunning); Movement(); }; 
                break;
            case PetState.PickedUp:
                animator.SetBool("IsWalking", false);
                Grounded = false;
                StateAction = () => {
                    Transform T = PlayertoFollow.state == PlayerState.Climbing ? PlayerBag.transform : PlayerPickUp.transform;
                    transform.rotation = T.rotation;
                    transform.position = T.position;
                };
                break;
        }
    }


    private void Start()
    {
        gameObject.SetActive(true);

        PlayertoFollow.OnTeleport += HandleOnTeleport;
        PlayertoFollow.OnClimbLadder += () => { OnPickUp(false); }; 
        PlayertoFollow.OnOffLadder += () => { HandleOnDrop(false); }; 
        MiniGameManager.Instance.onMiniGameStart += () => { HandleOnDrop(false); };
        interactable.onInteraction += OnInteract;
        pickupinteractable.onInteraction += () => { OnPickUp(true); };
        InputManager.Instance.OnPetLaunch += () => {
            if (state == PetState.None) return;

            if (PlayerManager.Instance.player.idetector.HoverInteractable == null)
            {
                HandleOnDrop(true);
            }
            else
            {
                HandleOnLaunch(PlayerManager.Instance.player.idetector.HoverInteractable.gameObject);
            }
        };
        AreaManager.Instance.onSwitchArea += (Areas A) => {
            switch (A)
            {
                case Areas.None:
                    gameObject.SetActive(false);
                    SetState(PetState.None);
                    break;
                case Areas.Home:
                    gameObject.SetActive(false);
                    SetState(PetState.None);
                    break;
                case Areas.StartingTown:
                    gameObject.SetActive(true);
                    HandleOnDrop(false);
                    SetState(PetState.Walking);
                    break;
                case Areas.School:
                    gameObject.SetActive(true);
                    SetState(PetState.Walking);
                    break;
            }

            HandleOnTeleport();
        };

        if(AreaManager.Instance.currentarea != Areas.Home)
        {
            gameObject.SetActive(true);
            HandleOnTeleport();
            SetState(PetState.Walking);
        }
        else
        {
            gameObject.SetActive(false);
            SetState(PetState.None);
        }
    }

    private void FixedUpdate()
    {
        if(PlayertoFollow.state == PlayerState.Walk && State != PetState.PickedUp)
        {
            SetState(PetState.Walking);
        }
        StateAction?.Invoke();
    }

    private void OnInteract()
    {
        animator.SetTrigger("IsInteracting");
        interactable.resetInteraction();
    }

    private void CollidersActivation(bool activate)
    {
        CollisionCollider.enabled = activate;
    }
    private void InteractableActivation(bool activate)
    {
        interactable.enabled = activate;
        pickupinteractable.enabled = activate;
        TriggerCollider.enabled = activate;
    }

    private void OnPickUp(bool SetPlayerColliders)
    {
        // Perform raycast check before pickup
        Vector3 raycastStart = PlayertoFollow.transform.position + Vector3.up * 0.5f; // Start slightly above player's feet
        Vector3 raycastDirection = Vector3.up;
        float raycastDistance = 3.85f;

        if (Physics.Raycast(raycastStart, raycastDirection, out RaycastHit hit, raycastDistance, pickupBlockingLayers))
        {
            // If we hit something above the player, don't allow pickup
            Debug.Log("Cannot pick up pet - obstacle above player");
            pickupinteractable.resetInteraction();
            return;
        }

        SetState(PetState.PickedUp);
        animator.SetBool("IsWalking", false);

        CollidersActivation(false);
        InteractableActivation(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if(SetPlayerColliders)
        {
            PlayertoFollow.OnPetPickUp();
        }
    }

    private void HandleOnLaunch(GameObject target)
    {
        if (state != PetState.PickedUp || !gameObject.activeSelf) return;

        if (launchCoroutine != null)
        {
            StopCoroutine(launchCoroutine);
        }

        CollidersActivation(true);

        launchCoroutine = StartCoroutine(FlyToTarget(target.transform.position));
        pickupinteractable.resetInteraction();
    }

    private void HandleOnDrop(bool launch)
    {
        if (state != PetState.PickedUp && state != PetState.None) return;

        if (launchCoroutine != null)
        {
            StopCoroutine(launchCoroutine);
        }

        CollidersActivation(true);

        if (launch)
        {
            // Calculate a position in front of the player to launch towards
            Vector3 launchTarget = PlayertoFollow.transform.position + PlayertoFollow.transform.forward * 1f;
            launchCoroutine = StartCoroutine(FlyToTarget(launchTarget));
        }
        else
        {
            rb.isKinematic = false;
            SetState(PetState.Idle);
            HandleOnTeleport();
            InteractableActivation(true);
            PlayertoFollow.OnDropPet();
        }

        pickupinteractable.resetInteraction();

        PlayertoFollow.idetector.HandleOnLeaveInteractable(pickupinteractable);

        PlayertoFollow.idetector.HandleOnLeaveInteractable(interactable);
    }


    private IEnumerator FlyToTarget(Vector3 target)
    {
        PlayertoFollow.OnThrowPet();

        float timer = 0.5f;

        while(timer >0)
        {
            PlayertoFollow.HandleRotationTowards(target);
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.isKinematic = false; // Keep physics active
        Vector3 startPosition = transform.position;

        // Calculate horizontal direction (ignore height difference)
        Vector3 horizontalDirection = new Vector3(
            target.x - startPosition.x,
            0f,
            target.z - startPosition.z
        ).normalized;

        SetState(PetState.Idle);

        rb.AddForce(Vector3.up * LaunchForceVertical/2, ForceMode.Impulse);
        rb.AddForce((horizontalDirection * LaunchForceHorizontal) / 2, ForceMode.Impulse);
        yield return new WaitForFixedUpdate();
        rb.AddForce(Vector3.up * LaunchForceVertical / 2, ForceMode.Impulse);
        rb.AddForce((horizontalDirection * LaunchForceHorizontal) / 2, ForceMode.Impulse); 

        launchCoroutine = null;

        Launched = true;

        yield return new WaitForSeconds(1f);
        InteractableActivation(true);
    }

    private float GetSlopeAdjustedGravity(Vector3 movementDirection)
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f))
            return baseGravity;

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle > maxSlopeAngle) return baseGravity;

        // Determine if we're moving uphill or downhill
        float slopeDirection = Vector3.Dot(movementDirection.normalized, hit.normal);

        if (slopeDirection < -0.1f) // Moving uphill
            return baseGravity * uphillGravityMultiplier;
        else if (slopeDirection > 0.1f) // Moving downhill
            return baseGravity * downhillGravityMultiplier;

        return baseGravity;
    }

    private void Movement()
    {
        if (!Grounded)
        {
            HandleFall();
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = PlayertoFollow.transform.position;

        Vector3 horizontalDirection = new Vector3(
            targetPosition.x - currentPosition.x,
            0f,
            targetPosition.z - currentPosition.z
        );

        float horizontalDistance = horizontalDirection.magnitude;
        Vector3 targetMovement = Vector3.zero;

        if (horizontalDistance > 3f)
        {
            horizontalDirection.Normalize();
            Vector3 targetVelocity = horizontalDirection * WalkSpeed;

            // Get adaptive gravity
            float currentGravity = GetSlopeAdjustedGravity(targetVelocity);

            targetMovement = Vector3.Lerp(
                rb.linearVelocity,
                targetVelocity,
                AccelerationSpeed * Time.fixedDeltaTime
            );

            targetMovement.y = rb.linearVelocity.y - currentGravity;
        }
        else
        {
            targetMovement = Vector3.Slerp(rb.linearVelocity, Vector3.zero, SlowDownSpeed * Time.fixedDeltaTime);
        }

        rb.linearVelocity = targetMovement;
        HandleRotationTowards();

        if (rb.linearVelocity.magnitude < 0.5f && horizontalDistance <= 3f)
        {
            SetState(PetState.Idle);
        }
    }

    private void HandleFall()
    {
        Vector3 targetMovement = Vector3.zero;

        if (!Grounded)
        {
            targetMovement = Vector3.Slerp(rb.linearVelocity, new Vector3(0, -10f, 0), 10f * Time.deltaTime);
        }

        rb.linearVelocity = targetMovement;

    }

    public void HandleRotationTowards()
    {
        Vector3 directionToTarget = (PlayertoFollow.transform.position - transform.position).normalized;
        directionToTarget.y = 0; // Optional: Keep rotation horizontal (ignore Y-axis)

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                WalkSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void HandleOnTeleport()
    {
        if (State == PetState.None) return;

        Teleporting = true;

        rb.position = PlayerBack.transform.position;
        rb.rotation = PlayerBack.transform.rotation;

        Grounded = false;

        StartCoroutine(TeleportingDelay());
    }

    private IEnumerator TeleportingDelay()
    {
        yield return null;
        yield return null;
        Teleporting = false;
    }

    public Vector3 CheckGroundOnce(Vector3 CurrentPos)
    {
        Vector3 raycastoffset = Vector3.up * 3f;
        Vector3 sphereOrigin = CurrentPos + raycastoffset;
        float sphereRadius = 0.25f; // Adjust based on player width/scale
        bool hitGround = Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out groundHit, 15f, trackLayer);

        if (hitGround)
        {
            // Check if the surface is a slope
            float angle = Vector3.Angle(groundHit.normal, Vector3.up);
            bool isSlope = angle > 1f; // 1 degree threshold to treat as slope

            Vector3 contactPoint = groundHit.point;

            return contactPoint;
        }
        else
        {
            return CurrentPos;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Overworld_Ground"))
        {
            Grounded = true;
            Launched = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Teleporting) return;

        if (other.gameObject.CompareTag("Player"))
        {
            PlayertoFollow.idetector.HandleOnTouchedInteractable(interactable);
            PlayertoFollow.idetector.HandleOnTouchedInteractable(pickupinteractable);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Overworld_Ground"))
        {
            Grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Overworld_Ground"))
        {
            Grounded = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayertoFollow.idetector.HandleOnLeaveInteractable(interactable);
            PlayertoFollow.idetector.HandleOnLeaveInteractable(pickupinteractable);
        }
    }
}

public enum PetState
{
   None,Idle,Walking,PickedUp
}