using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Camera Modules/Strafe")]
public class StrafeModule : CameraModule
{
    public float maxAngleZ = 1.5f;
    public float maxAngleX = 0.5f;
    public float smoothSpeed = 2f;

    Vector3 pos;
    Quaternion rot;
    float z;
    float x;

    public override Vector3 GetMovement() => pos;
    public override Quaternion GetRotation() => rot;

    public override void Init() { }
    public override void Deinit() { }
    public override void Update()
    {
        float desiredZ = 0;
        float desiredX = 0;

        if (PlayerMovement.Grounded)
        {
            if (PlayerMovement.LocalVelocity.x > 0 && PlayerMovement.Input.x > 0)
                desiredZ = -maxAngleZ;
            else if (PlayerMovement.LocalVelocity.x < 0 && PlayerMovement.Input.x < 0)
                desiredZ = maxAngleZ;

            if (PlayerMovement.LocalVelocity.z > 0 && PlayerMovement.Input.y > 0)
                desiredX = maxAngleX;
            else if (PlayerMovement.LocalVelocity.z < 0 && PlayerMovement.Input.y < 0)
                desiredX = -maxAngleX;
        }

        z = Mathf.Lerp(z, desiredZ, Time.deltaTime * smoothSpeed);
        x = Mathf.Lerp(x, desiredX, Time.deltaTime * smoothSpeed);
        rot = Quaternion.Euler(x, 0, z);
    }
}
