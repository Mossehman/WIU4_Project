using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public SoundLibrary musicplaylist;

    private int currentMusic = 0;
    private AudioSource musicSource;

    void Start()
    {
        musicSource = AudioManager.Instance.GetMusicSource();
    }

    void Update()
    {
        if (musicplaylist.sounds.Count > 0 && musicSource != null && AudioManager.Instance != null)
        {
            if (!musicSource.isPlaying)
            {
                AudioEventSystem.PlayMusic(musicplaylist.sounds[currentMusic].clip);
                currentMusic = (currentMusic + 1) % musicplaylist.sounds.Count;
            }
        }
    }
}