using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    EnemyHit,
    CarDamage,
    BulletFire,
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;

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

