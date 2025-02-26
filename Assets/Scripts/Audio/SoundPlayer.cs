using System.Collections;
using UnityEngine;

/// <summary>
/// Meant to attach to gameobjects with an animator, allowing the calling of events to play sounds
/// </summary>
public class SoundPlayer : MonoBehaviour
{
    /// <summary>
    /// Simplified verison of PlaySound, made to use with animation events
    /// </summary>
    /// <param name="soundName">The name of the sound you set in the Sound Libraries</param>
    public void PlaySound(string soundName)
    {
        AudioEventSystem.PlaySoundSimple(soundName);
    }
}