using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public SoundLibrary dayplaylist;
    public SoundLibrary nightplaylist;
    private SoundLibrary currentplaylist;
    [Range(0f, 1f)]
    public float volume = 1f;
    public string currentMusicName;
    public string currentPlaylistName;

    private int currentMusic = -1;
    private int[] previousMusicDay = new int[2];
    private int[] previousMusicNight = new int[2];
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
            currentplaylist = dayplaylist;
            currentPlaylistName = currentplaylist.libraryname; forceChange = true;
        }
        else if (TimeManager.Instance.IsWithinCurrentTimePeriod(TimeOfTheDay.Night, TimeOfTheDay.Midnight) && currentplaylist != nightplaylist)
        {
            currentplaylist = nightplaylist;
            currentPlaylistName = currentplaylist.libraryname; forceChange = true;
        }
        if (currentplaylist == null) return;
        if (currentplaylist.sounds.Count > 0 && musicSource != null && AudioManager.Instance != null)
        {
            if (!musicSource.isPlaying || Input.GetKeyDown(KeyCode.Backslash) || forceChange) {
                if (currentplaylist == dayplaylist)
                {
                    previousMusicDay[1] = previousMusicDay[0];
                    previousMusicDay[0] = currentMusic;
                }
                else if (currentplaylist == nightplaylist)
                {
                    previousMusicNight[1] = previousMusicNight[0];
                    previousMusicNight[0] = currentMusic;
                }
                while (true)
                {
                    currentMusic = UnityEngine.Random.Range(0, currentplaylist.sounds.Count - 1);
                    if ((currentplaylist == dayplaylist && previousMusicDay[0] == currentMusic && previousMusicDay[1] == currentMusic) &&
                        (currentplaylist == nightplaylist && previousMusicNight[0] == currentMusic && previousMusicNight[1] == currentMusic)) continue;
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