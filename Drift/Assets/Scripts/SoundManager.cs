using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    EnemyHit,
    CarDamage,
    BulletFire,
    ShockWave,
    Explosion,
    ElectricShock,
    OverHeat
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;
    private Dictionary<SoundType, AudioSource> loopingSources = new Dictionary<SoundType, AudioSource>();

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        SoundList soundList = instance.soundList[(int)sound];
        AudioClip[] clips = soundList.Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
    }

    public static void PlayLoopSound(SoundType sound, float volume = 1)
    {
        if (instance.loopingSources.ContainsKey(sound))
            return;

        SoundList soundList = instance.soundList[(int)sound];
        AudioClip clip = soundList.Sounds[UnityEngine.Random.Range(0, soundList.Sounds.Length)];
        AudioSource loopSource = instance.gameObject.AddComponent<AudioSource>();
        loopSource.clip = clip;
        loopSource.volume = volume * soundList.volume;
        loopSource.loop = true;
        loopSource.Play();

        instance.loopingSources.Add(sound, loopSource);
    }

    public static void StopLoopSound(SoundType sound)
    {
        if (!instance.loopingSources.ContainsKey(sound))
            return;

        AudioSource src = instance.loopingSources[sound];
        src.Stop();
        Destroy(src);
        instance.loopingSources.Remove(sound);
    }
    public static bool IsLooping(SoundType sound)
    {
        return instance.loopingSources.ContainsKey(sound);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
            if (soundList[i].volume <= 0f)
                soundList[i].volume = 1.0f;
        }
    }

#endif
}


[Serializable]
public struct SoundList
{

    [HideInInspector] public string name;
    [Range(0, 1)] public float volume;
    public AudioClip[] Sounds { get => sounds; }
    [SerializeField] private AudioClip[] sounds;
}

