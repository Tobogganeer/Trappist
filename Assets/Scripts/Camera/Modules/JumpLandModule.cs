using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Camera Modules/Jump Land")]
public class JumpLandModule : CameraModule
{
    public float smoothing = 4f;
    public float speed = 6f;
    public float rotationMultiplier = 10f;
    public float jumpDip = 0.35f;
    public float jumpBumpUp = 1.25f;
    public float jumpBumpSpeed = 3f;

    float verticalDip = 0f;
    float jumpBump = 0f;

    Vector3 pos;
    Quaternion rot;

    public override Vector3 GetMovement() => pos;

    public override Quaternion GetRotation() => rot;

    public override void Init()
    {
        PlayerMovement.OnLand += PlayerMovement_OnLand;
        PlayerMovement.OnJump += PlayerMovement_OnJump;
    }

    public override void Deinit()
    {
        PlayerMovement.OnLand -= PlayerMovement_OnLand;
        PlayerMovement.OnJump -= PlayerMovement_OnJump;
    }

    private void PlayerMovement_OnLand(float airtime)
    {
        verticalDip += Mathf.Lerp(0.0f, 2f, airtime * 0.6f);
    }

    private void PlayerMovement_OnJump(float jumpFactor)
    {
        jumpBump -= jumpFactor * jumpBumpUp;
    }

    public override void Update()
    {
        //FPSCameraOG.VerticalDip += Mathf.Lerp(0.0f, 2f, airtime * 0.6f);
        float jumpChargeFactor = PlayerMovement.Grounded ? PlayerMovement.JumpChargeFactor : 0f;
        verticalDip = Mathf.Max(verticalDip, jumpChargeFactor * jumpDip);

        pos = Vector3.Lerp(pos, Vector3.down * verticalDip, Time.deltaTime * smoothing);
        rot = Quaternion.Slerp(rot, Quaternion.Euler((verticalDip + jumpBump) * rotationMultiplier, 0, 0), Time.deltaTime * smoothing);

        //VerticalDip = Mathf.MoveTowards(VerticalDip, 0, Time.deltaTime * VertDipSpeed);
        verticalDip = Mathf.Lerp(verticalDip, 0, Time.deltaTime * speed);
        jumpBump = Mathf.Lerp(jumpBump, 0, Time.deltaTime * jumpBumpSpeed);
    }
}
