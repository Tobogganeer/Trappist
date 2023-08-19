using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using UnityEngine.Rendering.HighDefinition;

public class VMTest : MonoBehaviour
{
    public Animator animator;
    public VisualEffect muzzleFlash;
    public ParticleSystem casings;
    public HDAdditionalLightData muzzleLight;
    float muzzleLightIntensity;
    LightUnit unit;
    public float muzzleLightTime = 0.1f;
    Counter counter;

    private void Start()
    {
        if (muzzleLight != null)
        {
            muzzleLightIntensity = muzzleLight.intensity;
            unit = muzzleLight.lightUnit;
            muzzleLight.SetIntensity(0);
        }
    }

    // Viewmodel, not virtual machine ya gooks
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
            Play("Equip");
        if (Keyboard.current.qKey.wasPressedThisFrame)
            Play("Draw");
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Play("Fire");
            //AudioManager.Play(Sound.ID.P510_Fire, transform.position);
            //AudioManager.Play(Sound.ID.P510_Fire_Mech, transform.position);
            Sound.ID.P510_Fire.Play(transform.position);
            Sound.ID.P510_Fire_Mech.Play(transform.position);

            if (muzzleFlash != null)
                muzzleFlash.Play();
            if (casings != null)
                casings.Play();
            counter = muzzleLightTime;
        }
        if (Keyboard.current.fKey.wasPressedThisFrame)
            Play("Inspect");
        if (Keyboard.current.rKey.wasPressedThisFrame)
            Play("Reload");
        if (Keyboard.current.tKey.wasPressedThisFrame)
            Play("Reload Empty");

        if (muzzleLight != null)
        {
            float t = counter / muzzleLightTime;
            muzzleLight.SetIntensity(Mathf.Lerp(0, muzzleLightIntensity, t), unit);
        }
    }

    void Play(string state)
    {
        animator.Play(state, 0, 0f);
    }
}
