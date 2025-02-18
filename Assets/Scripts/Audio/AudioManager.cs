using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

[System.Serializable]
public enum AudioPriority
{
    Low = 256,    // Lowest priority (background sounds)
    Medium = 128, // Default priority
    High = 64,    // Important effects
    Critical = 0  // Always plays, interrupts if necessary
}

[System.Serializable]
public class AudioSettings
{
    public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
    public float MinimumRolloffDistance = 10f;
    public float MaximumRolloffDistance = 100f;
}

public class AudioManager : MonoBehaviour
{
    static public AudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    public List<SoundLibrary> soundLibraries; // List of sound libraries
    [SerializeField] private int maxSFXSources = 20;
    [SerializeField] private AudioSettings audioSettings;
    private AudioSource BackgroundAmbience;
    private AudioSource BackgroundMusic;
    private Queue<AudioSource> sfxQueue;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        InitializePool();

        // Subscribe to AudioEventSystem
        AudioEventSystem.PlaySoundEvent += PlaySoundFromEvent;
        AudioEventSystem.PlayMusicEvent += PlayMusicFromEvent;
        AudioEventSystem.PlayAmbienceEvent += PlayAmbienceFromEvent;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = BackgroundAmbience.clip;
        source.volume = BackgroundAmbience.volume;
        source.pitch = BackgroundAmbience.pitch;
        source.loop = BackgroundAmbience.loop;

        BackgroundAmbience = gameObject.AddComponent<AudioSource>();
        BackgroundMusic = gameObject.AddComponent<AudioSource>();

        source.Play();
    }

    private void InitializePool()
    {
        sfxQueue = new Queue<AudioSource>();
        for (int i = 0; i < maxSFXSources; i++)
        {
            GameObject sourceObj = new GameObject($"SFX_Src-{i}");
            sourceObj.transform.parent = transform;
            AudioSource source = sourceObj.AddComponent<AudioSource>();

            // Configure 3D sound properties
            source.spatialBlend = 1.0f; // Fully 3D
            source.rolloffMode = audioSettings.RolloffMode;
            source.minDistance = audioSettings.MinimumRolloffDistance;
            source.maxDistance = audioSettings.MaximumRolloffDistance;
            source.playOnAwake = false;

            sfxQueue.Enqueue(source);
        }
    }

    private void PlaySoundFromEvent(string soundName, AudioPriority priority, float? volume = null, Vector3? position = null, bool randomPitch = false, float minPitch = 0.95f, float maxPitch = 1.05f)
    {
        if (soundLibraries == null || soundLibraries.Count == 0)
        {
            Debug.LogWarning("No sound libraries assigned");
            return;
        }

        AudioClip clip = null;
        float defaultVolume = 1f; // Default fallback volume
        AudioPriority defaultPriority = priority;
        
        foreach (var library in soundLibraries)
        {
            clip = library.GetClipByName(soundName, out defaultVolume, out defaultPriority);
            if (clip != null) break;
        }

        if (clip == null)
        {
            Debug.LogWarning($"Sound " + soundName + " not found");
            return;
        }

        // Use provided volume or fallback to default
        float finalVolume = volume ?? defaultVolume;

        // Use the provided priority if specified
        priority = priority != defaultPriority ? priority : defaultPriority;

        PlaySFX(clip, finalVolume, priority, position, randomPitch, minPitch, maxPitch);
    }

    private void PlayMusicFromEvent(string musicName, float? volume = null)
    {
        if (soundLibraries == null || soundLibraries.Count == 0)
        {
            Debug.LogWarning("No sound libraries assigned");
            return;
        }

        AudioClip clip = null;
        float defaultVolume = 1f; // Default fallback volume
        AudioPriority defaultPriority = AudioPriority.Critical;

        foreach (var library in soundLibraries)
        {
            clip = library.GetClipByName(musicName, out defaultVolume, out defaultPriority);
            if (clip != null) break;
        }

        if (clip == null)
        {
            Debug.LogWarning($"Sound " + musicName + " not found");
            return;
        }

        // Use provided volume or fallback to default
        float finalVolume = volume ?? defaultVolume;

        BackgroundMusic.Stop();
        BackgroundMusic.clip = clip;
        BackgroundMusic.volume = finalVolume;
        BackgroundMusic.loop = true;
        BackgroundMusic.Play();
    }

    private void PlayAmbienceFromEvent(string ambienceName, float? volume = null)
    {
        if (soundLibraries == null || soundLibraries.Count == 0)
        {
            Debug.LogWarning("No sound libraries assigned");
            return;
        }

        AudioClip clip = null;
        float defaultVolume = 1f; // Default fallback volume
        AudioPriority defaultPriority = AudioPriority.Critical;

        foreach (var library in soundLibraries)
        {
            clip = library.GetClipByName(ambienceName, out defaultVolume, out defaultPriority);
            if (clip != null) break;
        }

        if (clip == null)
        {
            Debug.LogWarning($"Sound " + ambienceName + " not found");
            return;
        }

        // Use provided volume or fallback to default
        float finalVolume = volume ?? defaultVolume;

        BackgroundAmbience.Stop();
        BackgroundAmbience.clip = clip;
        BackgroundAmbience.volume = finalVolume;
        BackgroundAmbience.loop = true;
        BackgroundAmbience.Play();
    }

    public AudioSource PlayLoopingSound(string soundName, GameObject targetObject, float? volume = null, float pitch = 1f)
    {
        if (soundLibraries == null || soundLibraries.Count == 0)
        {
            Debug.LogWarning("No sound libraries assigned");
            return null;
        }

        AudioClip clip = null;
        float defaultVolume = 1f; // Fallback volume
        foreach (var library in soundLibraries)
        {
            clip = library.GetClipByName(soundName, out defaultVolume, out _);
            if (clip != null) break;
        }

        if (clip == null)
        {
            Debug.LogWarning($"Looping sound " + soundName + " not found");
            return null;
        }

        // Use the provided volume or fallback to the default
        float finalVolume = volume ?? defaultVolume;

        // Attach an AudioSource to the target object
        AudioSource source = targetObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = finalVolume;
        source.pitch = pitch;
        source.loop = true;
        source.spatialBlend = 1.0f; // Fully 3D sound
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;
        source.maxDistance = 50f;

        source.Play();
        return source; // Return the AudioSource for external control
    }

    private void PlaySFX(AudioClip clip, float volume, AudioPriority priority, Vector3? position, bool randomPitch, float minPitch, float maxPitch)
    {
        if (sfxQueue.Count == 0)
        {
            Debug.LogWarning("No available audio sources");
            return;
        }

        AudioSource source = sfxQueue.Dequeue();
        sfxQueue.Enqueue(source); // Recycle the source

        source.clip = clip;
        source.volume = volume;
        source.priority = (int)priority;

        if (position.HasValue)
        {
            source.spatialBlend = 1.0f; // Fully 3D sound
            source.transform.position = position.Value;
        }
        else
        {
            source.spatialBlend = 0.0f; // 2D sound
        }

        // Apply random pitch shift if enabled
        source.pitch = randomPitch ? UnityEngine.Random.Range(minPitch, maxPitch) : 1f;

        source.Play();
    }

}
