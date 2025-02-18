using System;
using UnityEngine;

public class AudioEventSystem : MonoBehaviour
{
    public static Action<string, AudioPriority, float?, Vector3?, bool, float, float> PlaySoundEvent;
    public static Action<string, float?> PlayMusicEvent;
    public static Action<string, float?> PlayAmbienceEvent;

    // Called from other scripts to play a sound
    public static void PlaySound(
        string soundName, 
        AudioPriority priority = AudioPriority.Medium, 
        float? volume = null, 
        Vector3? position = null,
        bool randomPitch = false, 
        float minPitch = 0.95f, 
        float maxPitch = 1.05f)
    {
        PlaySoundEvent?.Invoke(soundName, priority, volume, position, randomPitch, minPitch, maxPitch);
    }

    public static void PlayMusic(string musicName, float? volume = null)
    {
        PlayMusicEvent?.Invoke(musicName, volume);
    }

    public static void PlayAmbience(string ambienceName, float? volume = null)
    {
        PlayAmbienceEvent?.Invoke(ambienceName, volume);
    }
}
