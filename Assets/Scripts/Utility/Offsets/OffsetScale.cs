using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetScale : MonoBehaviour
{
    public Vector3 scale;

    private void LateUpdate()
    {
        transform.localScale = scale;
    }
}
