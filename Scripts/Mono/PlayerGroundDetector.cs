using System.Collections;
using UnityEngine;

public class PlayerGroundDetector : MonoBehaviour
{
    public delegate void leaveground();
    public event leaveground onLeaveGround;

    public delegate void stayground();
    public event stayground onStayGround;

    public delegate void enterground();
    public event enterground onEnterGround;

    [SerializeField] private bool OnGround;
    [SerializeField] private bool OnSlope;
    public bool onslope => OnSlope;

    [SerializeField] private float raycastStartOffset; // Start slightly above player's feet
    [SerializeField] private float raycastMaxDistance = 10f; // Maximum distance to check for ground
    [SerializeField] private LayerMask groundLayer; // Set this in Inspector to only detect ground

    public float GetGroundPosition(float originalY)
    {
        // Default to false in case no ground is detected
       bool isSlope = false;

        // Adjust these values in the Inspector
        Vector3 raycastStart = transform.position + Vector3.up * raycastStartOffset;

        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo, raycastMaxDistance, groundLayer))
        {
            // Calculate the angle between the hit normal and world up
            float angle = Vector3.Angle(hitInfo.normal, Vector3.up);

            // Define what angle you consider a slope (e.g., 30 degrees)
            float slopeThreshold = 30f;
            isSlope = angle > slopeThreshold;
            return hitInfo.point.y;
        }

        // If no ground detected, return current position (or handle differently)
        return originalY;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Overworld_Ground"))
        {
            onEnterGround?.Invoke();
            GetGroundPosition(transform.position.y);
        }

    }
    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Overworld_Ground"))
        {
            onStayGround?.Invoke();
            OnGround = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("Overworld_Ground") )
        {
            OnGround = false;
            GetGroundPosition(transform.position.y);
        }
        CheckLeftGround();
    }

    public void CheckLeftGround()
    {
        StartCoroutine(waitforGround());
    }

    private IEnumerator waitforGround()
    {
        yield return new WaitForSeconds(0.05f);
        if (!OnGround)
        {
            onLeaveGround?.Invoke();
        }
    }
}
