using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public float volume;
    private AudioSource audioSource;

    private bool isBgmPlay;
    public bool IsBgmPlay
    {
        get { return isBgmPlay; }
    }

    public void Init()
    {
        isBgmPlay = false;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume = 0.5f;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangedSlider(float value)
    {
        volume = value;
        audioSource.volume = volume;
    }

    public void PlayBGM(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
        isBgmPlay = true;
    }

}
