using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public SoundLibrary dayplaylist;
    public SoundLibrary nightplaylist;
    private SoundLibrary currentplaylist;
    //private SoundLibrary previousplaylist;
    [Range(0f, 1f)]
    public float volume = 1f;
    public string currentMusicName;
    public string currentPlaylistName;

    private int currentMusic = -1;
    private int previousMusic = -1;
    private int previousMusic2 = -1;
    private AudioSource musicSource;
    private bool forceChange = false;
    void Start()
    {
        musicSource = AudioManager.Instance.GetMusicSource();
        currentplaylist = dayplaylist;
    }

    void Update()
    {
        if (TimeManager.Instance == null) currentplaylist = dayplaylist;
        else if (TimeManager.Instance.IsWithinCurrentTimePeriod(TimeOfTheDay.Morning, TimeOfTheDay.Afternoon) && currentplaylist != dayplaylist)
        {
            //previousplaylist = currentplaylist;
            currentplaylist = dayplaylist;
            previousMusic = -1;
            previousMusic2 = -1;
            currentPlaylistName = currentplaylist.libraryname; forceChange = true;
        }
        else if (TimeManager.Instance.IsWithinCurrentTimePeriod(TimeOfTheDay.Night, TimeOfTheDay.Midnight) && currentplaylist != nightplaylist)
        {
            //previousplaylist = currentplaylist;
            currentplaylist = nightplaylist;
            previousMusic = -1;
            previousMusic2 = -1;
            currentPlaylistName = currentplaylist.libraryname; forceChange = true;
        }
        if (currentplaylist == null) return;
        //if (currentplaylist != previousplaylist) forceChange = true;
        if (currentplaylist.sounds.Count > 0 && musicSource != null && AudioManager.Instance != null)
        {
            if (!musicSource.isPlaying || Input.GetKeyDown(KeyCode.Backslash) || forceChange) {

                previousMusic2 = previousMusic;
                previousMusic = currentMusic;
                while (true)
                {
                    currentMusic = UnityEngine.Random.Range(0, currentplaylist.sounds.Count - 1);
                    if (currentMusic == previousMusic2 || currentMusic == previousMusic) { continue; }
                    else break;
                }
                AudioEventSystem.PlayMusic(currentplaylist.sounds[currentMusic].clip, volume);
                currentMusicName = currentplaylist.sounds[currentMusic].soundName;
                currentMusic = (currentMusic + 1) % currentplaylist.sounds.Count;
                forceChange = false;
            }
        }
    }
}