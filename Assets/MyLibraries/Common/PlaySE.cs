using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySE : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isPlay = false;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isPlay) 
        {
            if (audioSource.time == 0.0f && !audioSource.isPlaying) 
            {
                Destroy(gameObject);
            }
        }
    }

    public void PlaySe(AudioClip clip, float volume)
    {
        audioSource.volume = volume;
        audioSource.clip = clip;
        audioSource.Play();
        isPlay = true;
    }

}
