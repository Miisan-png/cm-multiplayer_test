using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatTrackDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask trackLayer;
    private RaycastHit groundHit;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float GroundCheckTimer;
    [SerializeField] private float currentGroundCheckTimer;
    [SerializeField] private float LastY;

    public Vector3 CheckGroundOnce(Vector3 CurrentPos)
    {
        Vector3 raycastoffset = Vector3.up * 20f;
        Vector3 sphereOrigin = CurrentPos + raycastoffset;
        float sphereRadius = 0.25f; // Adjust based on player width/scale
        bool hitGround = Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out groundHit, 100f, trackLayer);

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



    private void LockY()
    {
        // Freeze Y position and all rotations except Y
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;
    }

    private void UnlockY()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;
    }

}