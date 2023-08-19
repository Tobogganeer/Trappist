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
    public static float Scroll { get; private set; }
    public static InputAction LMB => actions.LMB;
    public static InputAction RMB => actions.RMB;
    public static InputAction Reload => actions.Reload;
    public static InputAction Crouch => actions.Crouch;
    public static InputAction Sprint => actions.Sprint;
    public static InputAction Jump => actions.Jump;
    public static InputAction Interact => actions.Interact;
    public static InputAction Escape => actions.Escape;
    /// <summary>
    /// Number keys are in the same layout as the keyboard, i.e index 0 is Alpha1
    /// </summary>
    public static InputAction[] NumberKeys { get; private set; }


    Vector2[] mouseBuffer = new Vector2[MouseBufferSize];
    const int MouseBufferSize = 4;


    private void Awake()
    {
        inputs = new Inputs();
        actions = inputs.Gameplay;

        actions.Movement.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        actions.Movement.canceled += ctx => Movement = Vector2.zero;

        actions.Look.performed += LookPerformed;
        actions.Look.canceled += ctx => Look = Vector2.zero;

        actions.Scroll.performed += ctx => Scroll = ctx.ReadValue<Vector2>().y;
        actions.Scroll.canceled += ctx => Scroll = 0;

        //actions.Interact.performed += ctx => Interact = true;

        NumberKeys = new InputAction[10]; // Hardcode to keys
        FillNumberKeys();
    }

    void FillNumberKeys()
    {
        NumberKeys[0] = actions.Alpha1;
        NumberKeys[1] = actions.Alpha2;
        NumberKeys[2] = actions.Alpha3;
        NumberKeys[3] = actions.Alpha4;
        NumberKeys[4] = actions.Alpha5;
        NumberKeys[5] = actions.Alpha6;
        NumberKeys[6] = actions.Alpha7;
        NumberKeys[7] = actions.Alpha8;
        NumberKeys[8] = actions.Alpha9;
        NumberKeys[9] = actions.Alpha0;
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
