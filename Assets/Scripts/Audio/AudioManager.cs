//#define MULTIPLAYER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MULTIPLAYER
using Tobo.Net;
#endif

[RequireComponent(typeof(AudioMaster))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    private void Awake()
    {
        instance = this;

        // Attached to App object, already in DontDestroyOnLoad

        /*
        
        if (Instance == null) Instance = this;

        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        */

        Init();
    }

    private void Init()
    {
        for (int i = 0; i < soundLibrary.sounds.Length; i++)
        {
            if (soundsDictionary.ContainsKey(soundLibrary.sounds[i].SoundID))
            {
                Debug.LogError($"Tried to add with same ID twice: {soundsDictionary[soundLibrary.sounds[i].SoundID].name}" +
                    $" was added first with ID {soundLibrary.sounds[i].SoundID}, and {soundLibrary.sounds[i].name} has the same ID!");
            }
            soundsDictionary.Add(soundLibrary.sounds[i].SoundID, soundLibrary.sounds[i]);
        }

        if (!soundsDictionary.ContainsKey(Sound.ID.None))
        {
            throw new System.Exception("AudioManager requires a sound with ID " + Sound.ID.None + "!");
        }
        /*
        clips.Clear();
        clipNameToIndex.Clear();

        int count = 0;

        for (int i = 0; i < singleClips.Length; i++)
        {
            clips.Add(singleClips[i]);
            if (clipNameToIndex.ContainsKey(clips[count].name))
                Debug.LogError("Duplicate single audio key: " + clips[count].name);
            clipNameToIndex.Add(clips[count].name, count++);
        }

        for (int i = 0; i < clipGroups.Length; i++)
        {
            int[] indices = new int[clipGroups[i].clips.Length];

            for (int j = 0; j < clipGroups[i].clips.Length; j++)
            {
                clips.Add(clipGroups[i].clips[j]);
                if (clipNameToIndex.ContainsKey(clips[count].name))
                    Debug.LogError("Duplicate group audio clip key: " + clips[count].name);
                clipNameToIndex.Add(clips[count].name, count);
                indices[j] = count++;
            }

            if (groupNameToIndices.ContainsKey(clipGroups[i].name))
                Debug.LogError("Duplicate group audio group key: " + clipGroups[i].name);
            groupNameToIndices.Add(clipGroups[i].name, indices);
        }
        */
    }

    //public AudioClip[] singleClips;
    //public AudioClipGroup[] clipGroups;
    //public Sound[] sounds;
    public SoundLibrary soundLibrary;

    private static readonly Dictionary<Sound.ID, Sound> soundsDictionary = new Dictionary<Sound.ID, Sound>();
    //private static readonly List<AudioClip> clips = new List<AudioClip>();
    //private static readonly Dictionary<string, int> clipNameToIndex = new Dictionary<string, int>();
    //private static readonly Dictionary<string, int[]> groupNameToIndices = new Dictionary<string, int[]>();

    /*
    public static int GetClipIndex(string clipOrGroup)
    {
        if (groupNameToIndices.TryGetValue(clipOrGroup, out int[] indices))
            return indices[Random.Range(0, indices.Length)];
        else if (clipNameToIndex.TryGetValue(clipOrGroup, out int clip))
            return clip;
        else
        {
            Debug.LogWarning("Could not get clip for " + clipOrGroup);
            return -1;
        }
    }
    */


    public static Sound GetSound(Sound.ID id)
    {
        if (!soundsDictionary.TryGetValue(id, out Sound sound))
        {
            Debug.LogWarning("No sound assigned for ID " + id);
            return soundsDictionary[Sound.ID.None];
        }

        return sound;
    }

    #region Play Methods
    public static void Play(Sound sound, Vector3 position, Transform parent = null)
    {
        PlayAudio(sound.GetAudio().SetPosition(position).SetParent(parent));
    }

    public static void Play2D(Sound sound)
    {
        PlayAudio(sound.GetAudio().Set2D());
    }

    public static void Play(Sound.ID soundID, Vector3 position, Transform parent = null)
    {
        Play(GetSound(soundID), position, parent);
    }

    public static void Play2D(Sound.ID soundID)
    {
        Play2D(GetSound(soundID));
    }

    public static void PlayLocal(Sound sound, Vector3 position, Transform parent = null)
    {
        PlayAudioLocal(sound.GetAudio().SetPosition(position).SetParent(parent));
    }

    public static void PlayLocal2D(Sound sound)
    {
        PlayAudioLocal(sound.GetAudio().Set2D());
    }

    public static void PlayLocal(Sound.ID soundID, Vector3 position, Transform parent = null)
    {
        PlayLocal(GetSound(soundID), position, parent);
    }

    public static void PlayLocal2D(Sound.ID soundID)
    {
        PlayLocal2D(GetSound(soundID));
    }
    #endregion

    public static void PlayAudio(Audio audio)
    {
        // Send over network
#if MULTIPLAYER
        AudioPacket p = new AudioPacket(audio);

        if (NetworkManager.IsServer)
            p.SendTo(S_Client.This, true); // Send to everyone but us
        else if (NetworkManager.ConnectedToServer)
            p.Send(Client.This);
#endif

        // Play on our side
        PlayAudioLocal(audio);
    }

    public static void PlayAudioLocal(Audio audio)
    {
        if (!soundsDictionary.TryGetValue(audio.ID, out Sound sound))
        {
            Debug.LogWarning("Tried to play audio with no corresponding Sound, ID " + audio.ID);
        }

        if (sound.SoundID == Sound.ID.None)
            return;

        if (sound.Clips == null || sound.Clips.Length == 0)
        {
            Debug.LogWarning("Tried to play sound with no clips assigned, ID " + sound.SoundID);
        }

        if (audio.ClipIndex < 0 || audio.ClipIndex >= sound.Clips.Length)
        {
            Debug.LogWarning($"Clip (index: {audio.ClipIndex}) was outside range for Sound.ID {audio.ID} (sound has {sound.Clips.Length} registered clips).");
            return;
        }

        GameObject sourceObj = AudioMaster.GetAudioSource();

        if (audio.Parent != null && !audio.Parent.gameObject.activeInHierarchy)
        {
            // Parent is turned off
            Debug.Log($"Skipping audio played on disabled parent ({audio.Parent.name})");
            sourceObj.SetActive(false);
            return;
        }

        sourceObj.transform.SetParent(audio.Parent);
        sourceObj.transform.position = audio.Position;

        AudioSource source = sourceObj.GetComponent<AudioSource>();

        source.clip = sound.Clips[audio.ClipIndex];
        if (source.clip == null)
            Debug.LogWarning($"Chosen audio clip was null - Sound.ID: {sound.SoundID} - Clip Index: {audio.ClipIndex}");
        source.maxDistance = audio.MaxDistance;
        source.pitch = audio.Pitch;
        source.volume = audio.Volume;
        source.spatialBlend = audio.Flags.HasFlag(Audio.AudioFlags.Global) ? 0f : 1f; // 0 for 2d, 1 for 3d
        source.outputAudioMixerGroup = AudioMaster.GetGroup(audio.Category);
        source.Play();

        sourceObj.GetComponent<PooledAudioSource>().DisableAfterTime(source.clip.length / audio.Pitch + 0.25f); // 0.25 seconds extra for good measure
    }

    public static void OnNetworkAudio(Audio audio)
    {
        PlayAudioLocal(audio);
    }
}

public class Audio
#if MULTIPLAYER
    : IBufferStruct
#endif
{
    public Sound.ID ID;
    public int ClipIndex { get; private set; }
    public Vector3 Position { get; private set; }
    public Transform Parent { get; private set; }
    public float MaxDistance { get; private set; }
    public AudioCategory Category { get; private set; }
    public float Volume { get; private set; }
    public float Pitch { get; private set; }

    public AudioFlags Flags { get; private set; }

    public Audio() { }
    public Audio(Sound sound)
    {
        LoadDefaultsFrom(sound);
    }

    void LoadDefaultsFrom(Sound sound)
    {
        ID = sound.SoundID;
        MaxDistance = sound.MaxDistance;
        Category = sound.Category;
        Volume = sound.Volume;
        Pitch = Random.Range(sound.MinPitch, sound.MaxPitch);
        if (sound.Is2d) Flags = AudioFlags.Global;
        if (sound.Clips.Length > 1)
        {
            Flags |= AudioFlags.Index;
            ClipIndex = Random.Range(0, sound.Clips.Length);
        }
    }

    #region Args

    public Audio SetClip(int clipIndex)
    {
        ClipIndex = clipIndex;
        Flags |= AudioFlags.Index;
        return this;
    }

    public Audio SetPosition(Vector3 position)
    {
        Position = position;
        return this;
    }

    public Audio SetParent(Transform parent)
    {
        Parent = parent;
        if (parent != null)
            Flags |= AudioFlags.Parent;
        return this;
    }

    public Audio SetDistance(float maxDistance)
    {
        if (MaxDistance != maxDistance)
        {
            MaxDistance = maxDistance;
            Flags |= AudioFlags.Distance;
        }
        return this;
    }

    public Audio SetVolume(float volume)
    {
        if (Volume != volume)
        {
            Volume = volume;
            Flags |= AudioFlags.Volume;
        }
        return this;
    }

    public Audio SetPitch(float min, float max)
    {
        Pitch = Random.Range(min, max);
        return this;
    }

    public Audio SetPitch(float pitch)
    {
        Pitch = pitch;
        return this;
    }

    public Audio SetCategory(AudioCategory category)
    {
        if (Category != category)
        {
            Category = category;
            Flags |= AudioFlags.Category;
        }
        return this;
    }

    public Audio SetGlobal()
    {
        Flags |= AudioFlags.Global;
        return this;
    }

    public Audio Set2D()
    {
        return SetGlobal();
    }

    #endregion

    #region Net
#if MULTIPLAYER
    public void Serialize(ByteBuffer message)
    {
        message.Add((ushort)ID);
        message.Add((byte)Flags);
        message.Add(Pitch);

        if (Flags.HasFlag(AudioFlags.Index))
            message.Add((byte)ClipIndex);

        if (!Flags.HasFlag(AudioFlags.Global))
        {
            message.Add(Position);

            //NetworkID netObj = Parent != null ? Parent.GetComponent<NetworkID>() : null;
            //if (Flags.HasFlag(AudioFlags.Parent) && netObj != null)
            //    message.Add(netObj);

            if (Flags.HasFlag(AudioFlags.Distance))
                message.Add(MaxDistance);
        }

        if (Flags.HasFlag(AudioFlags.Volume))
            message.Add(Volume);

        if (Flags.HasFlag(AudioFlags.Category))
            message.Add((byte)Category);
    }

    public void Deserialize(ByteBuffer message)
    {
        ID = (Sound.ID)message.Read<ushort>();
        LoadDefaultsFrom(Sound.From(ID));

        Flags = (AudioFlags)message.Read<byte>();
        Pitch = message.Read<float>();

        if (Flags.HasFlag(AudioFlags.Index))
            ClipIndex = message.Read<byte>();

        if (!Flags.HasFlag(AudioFlags.Global))
        {
            Position = message.Read<Vector3>();

            //if (Flags.HasFlag(AudioFlags.Parent))
            //    SetParent(message.GetNetworkID()?.transform);

            if (Flags.HasFlag(AudioFlags.Distance))
                MaxDistance = message.Read<float>();
        }

        if (Flags.HasFlag(AudioFlags.Volume))
            Volume = message.Read<float>();

        if (Flags.HasFlag(AudioFlags.Category))
            Category = (AudioCategory)message.GetByte();
    }
#endif
    #endregion

    public override string ToString()
    {
        return $"Audio:" +
            $" - ID: {ID}" +
            $" - ClipIndex: {ClipIndex}" +
            $" - Position: {Position}" +
            $" - Parent: {(Parent == null ? "null" : Parent)}" +
            $" - MaxDistance: {MaxDistance}" +
            $" - Category: {Category}" +
            $" - Volume: {Volume}" +
            $" - Pitch: {Pitch}" +
            $" - Flags: {Flags}";
    }

    [System.Flags]
    public enum AudioFlags : byte
    {
        None = 0,
        Global = 1 << 0,
        Parent = 1 << 1,
        Distance = 1 << 2,
        Volume = 1 << 3,
        //Pitch = 1 << 4, // Will have pitch every time
        Category = 1 << 5,
        Index = 1 << 6
    }
}

public enum AudioCategory : byte
{
    Master,
    SFX,
    Ambient,
    Music
}
