using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//[DefaultExecutionOrder(-105)] // Input system is -100
public class PlayerInputs : MonoBehaviour
{
    /*
    public bool controllerViewPow = false;
    public float pow = 2;
    public bool controllerViewSelfMult = false;
    public float selfMult = 0.2f;
    */

    private Inputs inputs;
    private static Inputs.GameplayActions actions;

    public static Vector2 Movement { get; private set; }
    public static Vector2 Look { get; private set; }
    public static Vector2 LookNoSmooth { get; private set; }
    public static float Lean { get; private set; }
    public static InputAction Primary => actions.Primary;
    public static InputAction Secondary => actions.Secondary;
    public static InputAction Reload => actions.Reload;
    public static InputAction Crouch => actions.Crouch;
    public static InputAction Sprint => actions.Sprint;
    public static InputAction Swap => actions.SwapWeapons;
    public static InputAction Jump => actions.Jump;
    public static InputAction Interact => actions.Interact;


    Vector2[] mouseBuffer = new Vector2[MouseBufferSize];
    const int MouseBufferSize = 4;


    private void Awake()
    {
        inputs = new Inputs();
        actions = inputs.Gameplay;

        actions.Movement.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        actions.Movement.canceled += ctx => Movement = Vector2.zero;

        actions.Lean.performed += ctx => Lean = ctx.ReadValue<float>();
        actions.Lean.canceled += ctx => Lean = 0f;

        actions.Look.performed += LookPerformed;
        actions.Look.canceled += ctx => Look = Vector2.zero;

        //actions.Interact.performed += ctx => Interact = true;
    }

    void LookPerformed(InputAction.CallbackContext ctx)
    {
        LookNoSmooth = ctx.ReadValue<Vector2>();
        Look = GetBufferedMouse(LookNoSmooth);
    }

    private void OnEnable()
    {
        inputs.Gameplay.Enable();
    }

    private void OnDisable()
    {
        inputs.Gameplay.Disable();
    }

    Vector2 GetBufferedMouse(Vector2 thisFrame)
    {
        //return thisFrame;
        
        Vector2 avg = thisFrame;
        for (int i = 0; i < mouseBuffer.Length - 1; i++)
        {
            mouseBuffer[i] = mouseBuffer[i + 1];
            avg += mouseBuffer[i];
        }
        mouseBuffer[mouseBuffer.Length - 1] = thisFrame;
        return avg / mouseBuffer.Length;
        
    }
}
