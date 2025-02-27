using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public SoundLibrary musicplaylist;
    [Range(0f, 1f)]
    public float volume = 1f;

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
                AudioEventSystem.PlayMusic(musicplaylist.sounds[currentMusic].clip, volume);
                currentMusic = (currentMusic + 1) % musicplaylist.sounds.Count;
            }
        }
    }
}