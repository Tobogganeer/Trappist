using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public CameraModule[] modules;

    public event System.Action<Vector3, Quaternion> OnUpdate;

    private void OnEnable()
    {
        for (int i = 0; i < modules.Length; i++)
        {
            modules[i].Init();
        }
    }

    private void Update()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        for (int i = 0; i < modules.Length; i++)
        {
            modules[i].Update();
            position += modules[i].GetMovement();
            rotation *= modules[i].GetRotation();
        }

        transform.localPosition = position;
        transform.localRotation = rotation;
        OnUpdate?.Invoke(position, rotation);
    }

    private void OnDisable()
    {
        for (int i = 0; i < modules.Length; i++)
        {
            modules[i].Deinit();
        }
    }
}
