using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VMTest : MonoBehaviour
{
    public Animator animator;

    // Viewmodel, not virtual machine ya gooks
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
            Play("Equip");
        if (Keyboard.current.qKey.wasPressedThisFrame)
            Play("Draw");
        if (Mouse.current.leftButton.wasPressedThisFrame)
            Play("Fire");
        if (Keyboard.current.fKey.wasPressedThisFrame)
            Play("Inspect");
        if (Keyboard.current.rKey.wasPressedThisFrame)
            Play("Reload");
        if (Keyboard.current.tKey.wasPressedThisFrame)
            Play("Reload Empty");
    }

    void Play(string state)
    {
        animator.Play(state, 0, 0f);
    }
}
