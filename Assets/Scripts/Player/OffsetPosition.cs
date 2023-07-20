using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetPosition : MonoBehaviour
{
    public Vector3 offset;

    void LateUpdate()
    {
        transform.Translate(offset, Space.Self);
    }
}
