using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetRotation : MonoBehaviour
{
    public Vector3 rotation;

    private void LateUpdate()
    {
        transform.Rotate(rotation, Space.Self);
    }
}
