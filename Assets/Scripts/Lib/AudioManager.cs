using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    public AudioMixerGroup[] mixerGroups;

    public Sound[] sounds;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = mixerGroups[s.mixerGroup];
        }
    }

    public void Play(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
        s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

        s.source.Play();
    }

    public void Stop(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
    }

    public void StopBgMusic()
    {
        // Stop previous music
        if (GameManagerScript.juiceProductive)
        {
            Stop("JuicyTheme");
        }
        else if (GameManagerScript.juiceUnproductive)
        {
            Stop("DubstepTheme");
        }
        else
        {
            Stop("CalmTheme");
        }
    }

    public void StopAndPlay(string music)
    {
        StopBgMusic();
        Play(music);
    }

    public IEnumerator PlayRandomLoop(string[] sounds)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 3f));

        while (true)
        {
            Play(sounds[UnityEngine.Random.Range(0, sounds.Length)]);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 3f));
        }
    }

    public IEnumerator PlayMultiple(string sound, int amount, float delay = 0.2f)
    {
        for (int i = 0; i < amount; ++i)
        {
            Play(sound);
            yield return new WaitForSeconds(delay);
        }
    }
}
