using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBInterpolator
{
    // https://forum.unity.com/threads/interpolating-rigidbody-rotations-properly-solved-i-have-code-people-can-use.492413/

    public Quaternion previousRotation { get; private set; } //previous rigidbody rotation
    public Vector3 previousPosition { get; private set; } //previous rigidbody position
    public Rigidbody cache { get; private set; } //Rigidbody cache
    public Transform renderNode { get; private set; } //transform with renderers/particle system objects as children.
    bool removed = false;

    public RBInterpolator(Rigidbody parent, Transform renderChild)
    {
        renderNode = renderChild;
        cache = parent;
        previousPosition = cache.position;
        previousRotation = cache.rotation;
    }

    public void OnUpdate()
    {
        if (!removed)
        {
            removed = true;
            renderNode.SetParent(null);
            renderNode.name += $" (From {cache.name})";
        }

        float interpFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        renderNode.position = Vector3.Lerp(previousPosition, cache.position, interpFactor);
        //renderNode.rotation = Quaternion.Lerp(previousRotation, cache.rotation, interpFactor);
        //renderNode.rotation = cache.rotation; // hack for player rotation (this isnt used for anything else rn)
    }

    public void OnLateUpdate()
    {
        renderNode.rotation = cache.transform.rotation;
    }

    public void OnFixedUpdate()
    {
        renderNode.position = cache.position;
        //renderNode.rotation = cache.rotation;
        previousPosition = renderNode.position;
        previousRotation = renderNode.rotation;
    }
}
