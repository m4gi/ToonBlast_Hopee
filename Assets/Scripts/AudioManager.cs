using deVoid.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClipDictionary : SerializableDictionary<string, AudioClip> { }

public class AudioMangerSignal : ASignal<string> { }
public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField]
    private AudioClipDictionary audioClipDics;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Signals.Get<AudioMangerSignal>().AddListener(PlaySound);
    }

    private void OnDestroy()
    {
        Signals.Get<AudioMangerSignal>().RemoveListener(PlaySound);
    }

    public void PlaySound(string soundName)
    {
        audioSource.clip = audioClipDics[soundName];
        audioSource.Play();
    }
}
