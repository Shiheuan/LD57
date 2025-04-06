using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("[S]AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Init();
    }
    
    public Sound[] sounds;

    private AudioSource tempSource;
    
    void Init()
    {
        foreach (var sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            sound.Init(source);
        }

        tempSource = gameObject.AddComponent<AudioSource>();

        //PlaySound("intro");
    }

    public void PlaySound(string name)
    {
        Sound sound = Array.Find(sounds, (s) => s.soundName == name);
        if (sound == null)
        {
            Debug.LogWarning($"sound: {name} not found in sounds, create temp one");
            AudioClip clip = Resources.Load<AudioClip>($"Sound/SFX/{name}");
            if (clip == null)
                return;
            sound = Sound.CreateFromAudioClip(clip);
            sound.Init(tempSource);
        }
        sound.audioSource.Play();
    }
    
    public void StopSound(string name)
    {
        Sound sound = Array.Find(sounds, (s) => s.soundName == name);
        if (sound == null)
        {
            return;
        }
        sound.audioSource.Stop();
    }

    public void StopAllSounds()
    {
        foreach (var sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            if (sound.audioSource != null)
                sound.audioSource.Stop();
        }

    }
}

[Serializable]
public class Sound
{
    public string soundName;

    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1f;

    [Range(0.5f, 2)]
    public float pitch = 1f;

    public bool loop = false;

    [HideInInspector]
    public AudioSource audioSource;

    public void Init(AudioSource source)
    {
        source.clip = clip;
        source.loop = loop;
        source.volume = volume;
        source.pitch = pitch;
        audioSource = source;
    }

    public static Sound CreateFromAudioClip(AudioClip clip)
    {
        Sound sound = new Sound();
        sound.clip = clip;
        return sound;
    }
}
