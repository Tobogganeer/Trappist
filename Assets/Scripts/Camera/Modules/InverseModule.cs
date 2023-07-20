using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Camera Modules/Inverse")]
public class InverseModule : CameraModule
{
    public override Vector3 GetMovement() => -Vector3.one;
    public override Quaternion GetRotation() => Quaternion.identity;

    public override void Init() { }
    public override void Deinit() { }
    public override void Update() { }
}
