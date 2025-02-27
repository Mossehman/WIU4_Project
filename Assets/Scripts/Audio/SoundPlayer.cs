using Assets.Scripts.AI.FiniteStateMachine;
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

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public string sound;
    AudioSource audioSource;
    public bool isLibrary = false;
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool randomPitch = false;
    [ConditionalHide("randomPitch", true)]
    public float minPitch = 1f;
    [ConditionalHide("randomPitch", true)]
    public float maxPitch = 1f;

    private void Update()
    {
        if (string.IsNullOrEmpty(sound)) return;
        if (isLibrary)
            AudioManager.Instance.PlayRandomAudio(sound, ref audioSource, volume, true, 1, randomPitch, minPitch, maxPitch);
        else
            AudioManager.Instance.PlayNonSpamAudio(sound, ref audioSource, volume, true, 1, randomPitch, minPitch, maxPitch);
    }
}