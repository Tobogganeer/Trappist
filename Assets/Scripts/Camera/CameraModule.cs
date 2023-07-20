using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraModule : ScriptableObject
{
    public abstract Vector3 GetMovement();
    public abstract Quaternion GetRotation();
    public abstract void Init();
    public abstract void Update();
    public abstract void Deinit();
}