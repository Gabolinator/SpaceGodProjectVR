using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{

    public Transform FollowTarget;
    public bool MatchRotation = true;

    public Vector3 _offsetPosition = Vector3.zero;
    public Vector3 _offsetEulerRotation = Vector3.zero;


    private void FixedUpdate()
    {
        FollowTargetPosition(FollowTarget, _offsetPosition);
        if (MatchRotation) MatchTargetRotation(FollowTarget, _offsetPosition);
    }

    private void FollowTargetPosition(Transform followTarget, Vector3 offsetPosition)
    {
        if (!followTarget) return;
        transform.position = followTarget.position;
        transform.localPosition += offsetPosition;
    }

    private void MatchTargetRotation(Transform followTarget, Vector3 offsetEulerRotation) 
    {
        if (!followTarget) return;
        {
            transform.rotation = followTarget.rotation;
            transform.localEulerAngles += offsetEulerRotation;
        }
    }
}
