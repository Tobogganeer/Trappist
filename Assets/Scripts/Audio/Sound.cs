using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Sound")]
public class Sound : ScriptableObject
{
    public enum ID : ushort
    {
        None,
        UIClick,
        UIHover,
        LeftFootstepOutside = 20,
        LeftFootstepInside,
        RightFootstepOutside,
        RightFootstepInside,
        DropOutside,
        DropInside,
        JumpOutside,
        JumpInside,
        AirlockOpen = 50,
        AirlockClose,
        CorridorOpen,
        CorridorClose,
        Depressurize = 60,
        Repressurize,
        MetalHit = 80,
        FabricHit,
    }

    [SerializeField] private ID soundID;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float maxDistance = 35f;
    [SerializeField] private AudioCategory category = AudioCategory.SFX;
    [SerializeField] private float volume = 1.0f;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.1f;
    [SerializeField] private bool is2d = false;

    public ID SoundID => soundID;
    public AudioClip[] Clips => clips;
    public float MaxDistance => maxDistance;
    public AudioCategory Category => category;
    public float Volume => volume;
    public float MinPitch => minPitch;
    public float MaxPitch => maxPitch;
    public bool Is2d => is2d;

    public static Sound From(ID id)
    {
        return AudioManager.GetSound(id);
    }

    public static Audio Override(ID id)
    {
        return From(id).Override();
    }

    public Audio Override()
    {
        return GetAudio();
    }

    public Audio GetAudio()
    {
        return new Audio(this);
    }
}
