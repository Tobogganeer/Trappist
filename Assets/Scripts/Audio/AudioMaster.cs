using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMaster : MonoBehaviour
{
    public static AudioMaster Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;

        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < DefaultAudioSources; i++)
        {
            audioSources.Enqueue(Instantiate(audioSourcePrefab, transform));
        }
    }

    public GameObject audioSourcePrefab;

    [Space]
    public AudioMixer masterMixer;
    public AudioMixerGroup masterGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup ambientGroup;
    public AudioMixerGroup musicGroup;
    public bool updatePitchWithTimeScale = true;
    

    static float oldPercent_lowpass;
    static float oldPercent_pitch;

    Queue<GameObject> audioSources = new Queue<GameObject>(DefaultAudioSources);
    const int DefaultAudioSources = 32;
    const int MaxAudioSources = 128;

    public static AudioMixerGroup GetGroup(AudioCategory category)
    {
        switch (category)
        {
            case AudioCategory.Master:
                return Instance.masterGroup;
            case AudioCategory.SFX:
                return Instance.sfxGroup;
            case AudioCategory.Ambient:
                return Instance.ambientGroup;
            case AudioCategory.Music:
                return Instance.musicGroup;
        }

        return null;
    }

    public static GameObject GetAudioSource()
    {
        Queue<GameObject> srcs = Instance.audioSources;
        GameObject obj = srcs.Dequeue();
        int index = srcs.Count;

        while (obj == null || (obj.activeSelf && index > 0))
        {
            index--;
            obj = srcs.Dequeue();
            if (obj != null)
                srcs.Enqueue(obj);
        }

        srcs.Enqueue(obj);

        if (index == 0 && obj.activeSelf)
        {
            if (srcs.Count < MaxAudioSources)
            {
                Debug.Log("Could not find any inactive audio sources, doubling pool size to " + srcs.Count * 2);
                int count = srcs.Count;
                for (int i = 0; i < count; i++)
                {
                    obj = Instantiate(Instance.audioSourcePrefab, Instance.transform);
                    srcs.Enqueue(obj);
                }
            }
            else
            {
                Debug.Log("Could not find any inactive audio sources, returning last dequeued source.");
            }
        }

        obj.SetActive(true);
        return obj;
    }

    public static void OnAudioSourceDestroyed(PooledAudioSource src)
    {
        if (Instance == null) return;
        if (src.transform.parent != Instance.transform)
            Instance.audioSources.Enqueue(Instantiate(Instance.audioSourcePrefab, Instance.transform));
    }



    #region Mixer Const Values

    private const string MASTER_VOLUME_PARAM = "master_volume";
    private const string AMBIENT_VOLUME_PARAM = "ambient_volume";
    private const string SFX_VOLUME_PARAM = "sfx_volume";
    private const string MUSIC_VOLUME_PARAM = "music_volume";

    private const float MIN_AUDIO_DB = -70;
    private const float MAX_AUDIO_DB = 5;

    private const float MIN_LOG_OUT = -60;
    private const float MAX_LOG_OUT = 0;

    private const string CUTOFF_FREQ_PARAM = "cutoff_freq";
    private const string PITCH_PARAM = "pitch";

    #endregion

    #region Mixer Methods

    public static void SetMasterVolume(float volume0_1) => SetVolume(MASTER_VOLUME_PARAM, volume0_1);

    public static void SetAmbientVolume(float volume0_1) => SetVolume(AMBIENT_VOLUME_PARAM, volume0_1);

    public static void SetSFXVolume(float volume0_1) => SetVolume(SFX_VOLUME_PARAM, volume0_1);

    public static void SetMusicVolume(float volume0_1) => SetVolume(MUSIC_VOLUME_PARAM, volume0_1);



    private static void SetVolume(string paramName, float volume0_1)
    {
        Instance.masterMixer.SetFloat(paramName, GetVolume(volume0_1));
    }

    private static float GetVolume(float volume0_1)
    {
        float clamped = Mathf.Clamp(volume0_1, 0.001f, 1f);

        float remapped = 20f * Mathf.Log10(clamped);
        remapped = Remap.Float(remapped, MIN_LOG_OUT, MAX_LOG_OUT, MIN_AUDIO_DB, MAX_AUDIO_DB);

        return remapped;
    }

    public static void SetLowPass(float percent0_1)
    {
        if (oldPercent_lowpass == percent0_1) return;
        else oldPercent_lowpass = percent0_1;

        float remapped = Remap.Float(percent0_1, 0, 1, 10, 22000);

        Instance.masterMixer.SetFloat(CUTOFF_FREQ_PARAM, remapped);
    }

    public static void SetPitch(float percent0_1)
    {
        if (oldPercent_pitch == percent0_1) return;
        else oldPercent_pitch = percent0_1;

        Instance.masterMixer.SetFloat(PITCH_PARAM, percent0_1);
    }

    private void Update()
    {
        if (updatePitchWithTimeScale)
            SetPitch(Time.timeScale);
    }

    #endregion
}
