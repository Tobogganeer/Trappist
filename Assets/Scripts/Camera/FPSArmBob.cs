using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSArmBob : MonoBehaviour
{
    public CameraMovement movement;

    [Space]
    public bool invertMouse = false;
    public float mouseSwayAmount = 0.01f;
    public float mouseMaxAmount = 0.06f;
    public float mouseSwayYMult = 2;
    public float mouseSmoothAmount = 12f;
    public float mouseRotAmount = 0.5f;
    public float mouseRotMaxAmount = 3f;
    public float mouseRotZMult = 1f;
    [Space]
    public float movementAmount = 0.5f;
    public float verticalDipAmount = 0.2f;

    float vertOffset;

    Vector3 swayPosition;
    Quaternion swayRotation;

    private void Start()
    {
        movement.OnUpdate += Movement_OnUpdate;
        vertOffset = transform.localPosition.y;
    }

    private void Movement_OnUpdate(Vector3 position, Quaternion rotation)
    {
        transform.localPosition = position.Mult(-movementAmount, verticalDipAmount, -1) + new Vector3(0, vertOffset, 0) + swayPosition;
        transform.localRotation = rotation * swayRotation;
    }

    private void Update()
    {
        MouseSway();
    }

    void MouseSway()
    {
        float invert = invertMouse ? -1 : 1;
        Vector2 desiredMovement = PlayerInputs.Look * (invert * mouseSwayAmount);
        //desiredMovement *= Time.deltaTime;
        desiredMovement.x = Mathf.Clamp(desiredMovement.x, -mouseMaxAmount, mouseMaxAmount);
        desiredMovement.y = Mathf.Clamp(desiredMovement.y * mouseSwayYMult, -mouseMaxAmount, mouseMaxAmount);

        swayPosition = Vector3.Lerp(swayPosition, desiredMovement, Time.deltaTime * mouseSmoothAmount);


        Vector3 rotMovement = new Vector3(-PlayerInputs.Look.y, PlayerInputs.Look.x, PlayerInputs.Look.x) * (invert * mouseRotAmount);
        //Debug.Log("Mouse: " + Input.GetAxis("Mouse X") + " " + Input.GetAxis("Mouse Y"));

        rotMovement.x = Mathf.Clamp(rotMovement.x, -mouseRotMaxAmount, mouseRotMaxAmount);
        rotMovement.y = Mathf.Clamp(-rotMovement.y, -mouseRotMaxAmount, mouseRotMaxAmount);
        rotMovement.z = Mathf.Clamp(-rotMovement.z * mouseRotZMult, -mouseRotMaxAmount * mouseRotZMult, mouseRotMaxAmount * mouseRotZMult);

        //mouseSwayObj.localRotation = Quaternion.Slerp(mouseSwayObj.localRotation, Quaternion.Euler(rotMovement), Time.deltaTime * mouseSmoothAmount);
        swayRotation = Quaternion.Slerp(swayRotation, Quaternion.Euler(rotMovement), Time.deltaTime * mouseSmoothAmount);
    }

    private void OnDestroy()
    {
        movement.OnUpdate -= Movement_OnUpdate;
    }
}
