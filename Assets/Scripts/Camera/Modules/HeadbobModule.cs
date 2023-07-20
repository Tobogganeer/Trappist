using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Camera Modules/Headbob")]
public class HeadbobModule : CameraModule
{
    public float stepSpeed = 2.5f;

    [Space]
    public float bobScale = 0.06f;
    public float bobVerticalMultiplier = 1.5f;
    public float rotationAmount = 0.2f;
    public float smoothSpeed = 5f;

    Vector3 pos;
    Quaternion rot;

    float time;
    float sinValue;

    public float TimeValue => time;
    public float SinValue => sinValue;


    public override Vector3 GetMovement() => pos;
    public override Quaternion GetRotation() => rot;

    public override void Init() { }
    public override void Deinit() { }
    public override void Update()
    {
        CalculateTime();
        Bob();
        Rot();
    }

    void CalculateTime()
    {
        Vector3 actualHorizontalVelocity = PlayerMovement.LocalVelocity.Flattened();

        float velocityMag = actualHorizontalVelocity.magnitude;

        time += Time.deltaTime * stepSpeed * velocityMag;

        sinValue = Mathf.Sin(time);

        if (velocityMag < 1.0f || !PlayerMovement.Grounded)
        {
            time = 0;
            sinValue = 0;
        }
    }

    void Bob()
    {
        Vector3 bob = Vector3.zero;
        if (PlayerMovement.Grounded)
            bob = new Vector3(sinValue, -Mathf.Abs(sinValue) * bobVerticalMultiplier);
        bob *= bobScale;
        pos = Vector3.Lerp(pos, bob, Time.deltaTime * smoothSpeed);
    }

    void Rot()
    {
        Quaternion desired = Quaternion.identity;
        if (PlayerMovement.Grounded)
            desired = Quaternion.Euler(0, -sinValue * rotationAmount, 0);
        rot = Quaternion.Slerp(rot, desired, Time.deltaTime * smoothSpeed);
    }
}
