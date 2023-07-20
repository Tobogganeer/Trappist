using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    public static FPSCamera instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform yawTransform;
    public Transform pitchTransform;
    public float sensitivity = 3.5f;


    const float MaxVerticalRotation = 90;


    float pitch;


    public static Transform Transform => instance.transform;
    public static Vector3 Position => instance.transform.forward;
    public static Vector3 ViewDir => instance.transform.forward;


    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        MouseLook();
    }

    private void MouseLook()
    {
        Vector2 look = PlayerInputs.Look;

        yawTransform.Rotate(Vector3.up * look.x * sensitivity);

        pitch = Mathf.Clamp(pitch - look.y * sensitivity, -MaxVerticalRotation, MaxVerticalRotation);

        pitchTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}
