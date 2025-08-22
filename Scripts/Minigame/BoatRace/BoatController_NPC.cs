using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoatController_NPC : BoatController
{
    [Header("Corner Detection")]
    [SerializeField] private float CurrentDriftTriggerDistance;
    [SerializeField] private float SafeDriftTriggerDistance;
    [SerializeField] private float AggressiveDriftTriggerDistance;
    [SerializeField] private float WideCornerThreshold;
    [SerializeField] private float TightCornerThreshold;

    [SerializeField] private float DistancetoReleaseDrift;
    private Coroutine CheckDistanceCoroutine;
    private Coroutine DriftingCoroutine;

    [Header("Steering Settings")]
    [SerializeField] private float steerRange = 5f;
    [SerializeField] private float steerDuration = 0.1f;

    [Header("Raycast Settings")]
    [SerializeField] private float FrtRayCastDistance;
    [SerializeField] private float SideRayCastDistance;
    [SerializeField] private LayerMask trackLayer;
    [SerializeField] private Vector3 raycastOffset = new Vector3(0, 0.1f, 0); // Small offset from center
    private RaycastHit FrtObstacleHit;
    private RaycastHit RightObstacleHit;
    private RaycastHit LeftObstacleHit;
    private Coroutine RaycastCoroutine;

    [SerializeField] private bool ObstacleFrt;
    [SerializeField] private bool ObstacleRight;
    [SerializeField] private bool ObstacleLeft;
    [SerializeField] private int ClearRight;
    [SerializeField] private int ClearLeft;

    private Coroutine steeringCoroutine;

    [SerializeField] private GameObject CornerTarget;

    protected override void Start()
    {
        base.Start();
        Minigame_BoatRace.Instance.OnRaceStart += OnRaceStart;
        Minigame_BoatRace.Instance.onMiniGameEnd += OnRaceEnd;
        Minigame_BoatRace.Instance.OnSwitchCourse += AssignHandlingData;
        base.onBump += OnBump;
        base.onDirectionChanged += OnDirectionChanged;
        base.OnEnterAlternateCorner += HandleAlternateCorners;
    }

    private void AssignHandlingData(TrackData datatouse)
    {
        SafeDriftTriggerDistance = datatouse.SafeDriftTriggerDistance;
        AggressiveDriftTriggerDistance = datatouse.AggressiveDriftTriggerDistance;
        WideCornerThreshold = datatouse.WideCornerThreshold;
        TightCornerThreshold = datatouse.TightCornerThreshold;
        steerRange = datatouse.steerRange;
        steerDuration = datatouse.steerDuration;
    }


    private void OnRaceStart()
    {
        ResetAllCoroutines();
        throttle(true);
        CornerTarget = NextCorner?.gameObject;

        StartSteering();
        CheckDistanceCoroutine = StartCoroutine(CheckDistancefromNextCorner());
        RaycastCoroutine = StartCoroutine(HandleRaycast());
    }

    private void OnRaceEnd()
    {
        throttle(false);
        ResetAllCoroutines();
    }

    private void OnBump()
    {
        Debug.Log("Restearing on bump");
        StartSteering();
    }

    private void OnDirectionChanged(TurnDirection direction)
    {
        Debug.Log("Restearing on turn");
        CornerTarget = NextCorner?.gameObject; // <-- Add this line

        StartSteering();

        if (CheckDistanceCoroutine != null)
            StopCoroutine(CheckDistanceCoroutine);

        CheckDistanceCoroutine = StartCoroutine(CheckDistancefromNextCorner());
    }

    private void ResetAllCoroutines()
    {
        if (CheckDistanceCoroutine != null)
        {
            StopCoroutine(CheckDistanceCoroutine);
            CheckDistanceCoroutine = null;
        }

        if (DriftingCoroutine != null)
        {
            StopCoroutine(DriftingCoroutine);
            DriftingCoroutine = null;
        }

        if (RaycastCoroutine != null)
        {
            StopCoroutine(RaycastCoroutine);
            RaycastCoroutine = null;
        }

        StopSteering();
    }

    private void StartSteering()
    {
        StopSteering(); // Ensures only one steering coroutine
        steeringCoroutine = StartCoroutine(SteerTowardNextCorner());
        Debug.Log("Started Steering");
    }

    private void StopSteering()
    {
        if (steeringCoroutine != null)
        {
            StopCoroutine(steeringCoroutine);
            steeringCoroutine = null;
            steering(0); // Reset steering on stop
            Debug.Log("Stopped Steering");
        }
    }

    private IEnumerator SteerTowardNextCorner()
    {
        while (true)
        {
            if (CornerTarget != null)
            {
                Vector3 toCorner = CornerTarget.transform.position - transform.position;

                // Flatten vectors to XZ plane
                Vector3 forwardFlat = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                Vector3 toCornerFlat = new Vector3(toCorner.x, 0f, toCorner.z).normalized;

                float angle = Vector3.SignedAngle(forwardFlat, toCornerFlat, Vector3.up);

                int steerDirection = 0;
                if (Mathf.Abs(angle) > steerRange)
                    steerDirection = angle > 0 ? 1 : -1;

                if (ObstacleFrt)
                {
                    if (ObstacleLeft && ObstacleRight)
                    {
                        if (ClearLeft > ClearRight)
                            steerDirection = -1;
                        else if (ClearRight > ClearLeft)
                            steerDirection = 1;
                        else
                            steerDirection = 1; // Arbitrary choice on tie
                    }
                    else if (ObstacleRight)
                    {
                        // No side cones hit, but still front obstacle? Use front ray info
                        if (ClearLeft > ClearRight)
                            steerDirection = -1;
                        else if (ClearRight > ClearLeft)
                            steerDirection = 1;
                        else
                            steerDirection = -1;
                    }
                    else if (ObstacleLeft)
                    {
                        // No side cones hit, but still front obstacle? Use front ray info
                        if (ClearLeft > ClearRight)
                            steerDirection = -1;
                        else if (ClearRight > ClearLeft)
                            steerDirection = 1;
                        else
                            steerDirection = 1;
                    }
                    else
                    {
                        // No side cones hit, but still front obstacle? Use front ray info
                        if (ClearLeft > ClearRight)
                            steerDirection = -1;
                        else if (ClearRight > ClearLeft)
                            steerDirection = 1;
                        else
                            steerDirection = 1;
                    }
                }
                else
                {
                    switch(steerDirection)
                    {
                        case 1:
                            if(ObstacleRight)
                            {
                                steerDirection = 0;
                            }
                            break;
                        case -1:
                            if (ObstacleLeft)
                            {
                                steerDirection = 0;
                            }
                            break;
                    }
                }

                if (steerDirection == 0)
                {
                    steering(0);
                }
                else
                {
                    steering(steerDirection);
                    float _steerduration = steerDuration;
                    while(_steerduration > 0)
                    {
                        _steerduration -= Time.fixedDeltaTime;
                        yield return new WaitForFixedUpdate();
                    }
                    steering(0);
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator HandleRaycast()
    {
        while (true)
        {
            Vector3 origin = transform.position + raycastOffset;
            float radius = 0.25f;
            float coneDistance = FrtRayCastDistance;

            int clearLeft, clearRight;

            ParallelFrontCasts(
                origin,
                transform.forward,
                width: 1.0f,
                count: 5,
                distance: coneDistance,
                radius: radius,
                trackLayer,
                out FrtObstacleHit,
                out clearLeft,
                out clearRight
            );

            ClearRight = clearRight;
            ClearLeft = clearLeft;

            coneDistance = SideRayCastDistance;

            // Side detection – cone
            WideConeSphereCast(
                origin,
                transform.forward,
                coneDistance,
                resolution: 30,
                totalAngle: 180f,
                frontThreshold: 10f,
                radius,
                trackLayer,
                out _,
                out LeftObstacleHit,
                out RightObstacleHit
            );

            ObstacleFrt = FrtObstacleHit.collider != null;
            ObstacleRight = RightObstacleHit.collider != null;
            ObstacleLeft = LeftObstacleHit.collider != null;

            yield return new WaitForFixedUpdate();
        }
    }

    private void ParallelFrontCasts(
     Vector3 origin,
     Vector3 forward,
     float width,
     int count,
     float distance,
     float radius,
     LayerMask layer,
     out RaycastHit frtHit,
     out int clearLeft,
     out int clearRight)
    {
        frtHit = new RaycastHit();
        clearLeft = 0;
        clearRight = 0;
        bool found = false;

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        float spacing = width / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float offset = -width / 2f + spacing * i;
            Vector3 start = origin + right * offset;
            Vector3 dir = forward;

            Debug.DrawRay(start, dir * distance, Color.blue,0.01f);

            if (Physics.SphereCast(start, radius, dir, out RaycastHit hit, distance, layer))
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.cyan);

                if (!found || hit.distance < frtHit.distance)
                {
                    frtHit = hit;
                    found = true;
                }
            }
            else
            {
                if (offset < 0)
                    clearLeft++;
                else if (offset > 0)
                    clearRight++;
                // middle ray (offset == 0) is neutral; ignore for side steering
            }
        }
    }



    private void WideConeSphereCast(
     Vector3 origin,
     Vector3 forward,
     float distance,
     int resolution,
     float totalAngle,
     float frontThreshold, // e.g. 15 degrees = forward cone width
     float radius,
     LayerMask layer,
     out RaycastHit frtHit,
     out RaycastHit leftHit,
     out RaycastHit rightHit)
    {
        frtHit = new RaycastHit();
        leftHit = new RaycastHit();
        rightHit = new RaycastHit();

        bool leftFound = false, rightFound = false;

        for (int i = 0; i < resolution; i++)
        {
            float angleStep = totalAngle / (resolution - 1);
            float angle = -totalAngle / 2 + i * angleStep;

            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            // Always draw debug lines
            Debug.DrawRay(origin, dir * distance, Color.green, 0.01f);

            if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, distance, layer))
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.red);

                float angleFromForward = Vector3.SignedAngle(forward, dir, Vector3.up);

                if (angleFromForward > frontThreshold)
                {
                    if (!rightFound || hit.distance < rightHit.distance)
                    {
                        rightHit = hit;
                        rightFound = true;
                    }
                }
                else if (angleFromForward < - frontThreshold)
                {
                    if (!leftFound || hit.distance < leftHit.distance)
                    {
                        leftHit = hit;
                        leftFound = true;
                    }
                }
            }
        }
    }



    private IEnumerator CheckDistancefromNextCorner()
    {
        if (CornerTarget == null) yield break;

        if(Vector3.Dot(CornerTarget.transform.position - transform.position, transform.forward) < TightCornerThreshold)
        {
            CurrentDriftTriggerDistance = SafeDriftTriggerDistance;
        }
        else if (Vector3.Dot(CornerTarget.transform.position - transform.position, transform.forward) > WideCornerThreshold)
        {
            CurrentDriftTriggerDistance = AggressiveDriftTriggerDistance;
        }

        // Wait one frame to allow position update
        yield return new WaitForFixedUpdate();


        while (Vector3.Dot(CornerTarget.transform.position - transform.position, transform.forward) > CurrentDriftTriggerDistance)
        {
            yield return new WaitForFixedUpdate();
        }

        if (DriftingCoroutine != null)
            StopCoroutine(DriftingCoroutine);

        DriftingCoroutine = StartCoroutine(StartDrifting());
    }

    private IEnumerator StartDrifting()
    {
        StopSteering();
        StartDrift();
        Debug.Log("Drift Started");

        while (Vector3.Dot(CornerTarget.transform.position - transform.position, transform.forward) > DistancetoReleaseDrift)
        {
            yield return new WaitForFixedUpdate();
        }

        ReleaseDrift();
        Debug.Log("Drift Released");
    }

    private void HandleAlternateCorners(GameObject newcornertarget)
    {
        if(AllowAlternatePath)
        {
            CornerTarget = newcornertarget;
        }
    }
}
