using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<Sound> _sounds;
    [SerializeField, Min(0)] private float _minDistance;
    [SerializeField, Min(0)] private float _maxDistance;

    void Awake()
    {
        foreach (Sound s in _sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = false;
            s.source.loop = s.loop;
            s.source.priority = s.priority;
            s.source.minDistance = _minDistance;
            if (_maxDistance > _minDistance) s.source.maxDistance = _maxDistance;
        }
    }

    public void Play(string name)
    {
        Sound s = _sounds.FirstOrDefault(sound => sound.name == name);
        Play(s);
    }

    public void Play(Sound s)
    {
        if (s == null)
        {
            Debug.Log("Sound " + name + " not Found");
            return;
        }
        else if (!s.source.isPlaying)
        {
            s.source.Play();
            Debug.Log(s.name);
        }
    }

    public void PlayRandom()
    {
        var rand = Random.Range(0, _sounds.Count);
        Play(_sounds[rand]);
    }

    public void Stop(string name)
    {
        Sound s = _sounds.FirstOrDefault(sound => sound.name == name);

        if (s == null)
            return;

        s.source.Stop();
    }
}