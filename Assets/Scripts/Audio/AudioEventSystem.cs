using Assets.Scripts.AI.FiniteStateMachine;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioEventSystem : MonoBehaviour
{
    public static Action<string, AudioPriority, float?, Vector3?, bool, float, float> PlaySoundEvent;
    public static Action<string, float?> PlayMusicEventByName;
    public static Action<AudioClip, float?> PlayMusicEventByClip;
    public static Action<string, float?> PlayAmbienceEvent;

    /// <summary>
    /// This will invoke an event to play a sound
    /// </summary>
    /// <param name="soundName">The name of the sound you set in the Sound Libraries</param>
    /// <param name="priority">(Optional) Override the priority of this sound</param>
    /// <param name="volume">(Optional) The volume of the audio when you wish to override</param>
    /// <param name="position">(Optional) The position of the audio it is emitting from</param>
    /// <param name="randomPitch">(Optional) A boolean to enable playing the audio at a random pitch</param>
    /// <param name="minPitch">(Optional) The minimum random ptich shift</param>
    /// <param name="maxPitch">(Optional) The maximum random ptich shift</param>
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
    /// <summary>
    /// Simplified verison of PlaySound, made to use with animation events
    /// </summary>
    /// <param name="soundName">The name of the sound you set in the Sound Libraries</param>
    public static void PlaySoundSimple(string soundName)
    {
        PlaySoundEvent?.Invoke(soundName, AudioPriority.Medium, null, null, false, 1, 1);
    }
    /// <summary>
    /// This will invoke an event to play music, it will always loop
    /// </summary>
    /// <param name="musicName">The name of the music you set in the Sound Libraries</param>
    /// <param name="volume">(Optional) The volume of the audio when you wish to override</param>
    public static void PlayMusic(string musicName, float? volume = null)
    {
        PlayMusicEventByName?.Invoke(musicName, volume);
    }
    public static void PlayMusic(AudioClip musicName, float? volume = null)
    {
        PlayMusicEventByClip?.Invoke(musicName, volume);
    }
    /// <summary>
    /// This will invoke an event to play an ambient sound, it will always loop
    /// </summary>
    /// <param name="ambienceName">The name of the ambience you set in the Sound Libraries</param>
    /// <param name="volume">(Optional) The volume of the audio when you wish to override</param>
    public static void PlayAmbience(string ambienceName, float? volume = null)
    {
        PlayAmbienceEvent?.Invoke(ambienceName, volume);
    }

    public static void PlaySoundSmart(
        SoundInfo soundInfo,
        ref AudioSource source,
        AudioPriority priority = AudioPriority.Medium,
        float? volume = null, 
        bool is3D = false, 
        bool isNonSpammable = false, 
        float pitch = 1.0f,
        bool randomPitch = false, 
        float minPitch = 0.95f, 
        float maxPitch = 1.05f)
    {
        if (soundInfo.isLibrary)
        {
            AudioManager.Instance.PlayRandomAudio(soundInfo.name, ref source, volume, is3D, pitch, randomPitch, minPitch, maxPitch);
            return;
        }
        if (isNonSpammable)
        {
            AudioManager.Instance.PlayNonSpamAudio(soundInfo.name, ref source, volume, is3D, pitch, randomPitch, minPitch, maxPitch);
            return;
        }

        PlaySoundEvent?.Invoke(soundInfo.name, priority, volume, source.transform.position, randomPitch, minPitch, maxPitch);
    }
}
