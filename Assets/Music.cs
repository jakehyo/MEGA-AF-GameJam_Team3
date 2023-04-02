using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public AudioClip music;
    public AudioClip finish;
    AudioSource source;


    void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = music;
        source.Play();
    }

    public void switchMusic()
    {
        source.clip = finish;
        source.Play();
    }
}
