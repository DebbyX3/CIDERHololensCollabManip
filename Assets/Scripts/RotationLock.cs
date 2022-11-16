using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is useless now, not used: to lock rotations I use the object manipulator constraints

public class RotationLock : MonoBehaviour
{
    public bool LockX, LockY, LockZ;
    private Vector3 StartRotation;

    void Start() { StartRotation = transform.rotation.eulerAngles; }

    void LateUpdate()
    {
        Vector3 newRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(
            LockX ? StartRotation.x : newRotation.x,
            LockY ? StartRotation.y : newRotation.y,
            LockZ ? StartRotation.z : newRotation.z
        );
    }
}