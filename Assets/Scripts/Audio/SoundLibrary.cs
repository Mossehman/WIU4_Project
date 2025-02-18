using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Audio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public string soundName;
        public AudioClip clip;
        public AudioPriority priority = AudioPriority.Medium;
        public float volume = 1f;
    }

    public List<SoundEntry> sounds = new List<SoundEntry>();

    public AudioClip GetClipByName(string name, out float volume, out AudioPriority priority)
    {
        foreach (SoundEntry entry in sounds)
        {
            if (entry.soundName == name)
            {
                volume = entry.volume;
                priority = entry.priority;
                return entry.clip;
            }
        }

        volume = 1f;
        priority = AudioPriority.Medium;
        return null;
    }
}
