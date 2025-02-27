- Audio System -

1a. Create a scriptable object Sound Library (Project > Create > Audio > SoundLibrary)
1b. Add your sounds into the list of the scriptable object (Sound name, imported Audio Clip, Audio Priority, Volume)



2a. Create an empty GameObject with AudioManager in your scene.
2b. Add in your Sound Libraries. Optionally you can set:
- Sound Pool size 
- Audio Rolloff  



3a. In any script, use any of these:
AudioEventSystem.PlaySound // Plays a SFX
AudioEventSystem.PlaySoundSimple // Simplified PlaySound, only takes soundName
AudioEventSystem.PlayMusic // Plays looping music, overload for string and Audio Clip
AudioEventSystem.PlayAmbience // Plays looping ambience
AudioEventSystem.PlaySoundSmart // Uses the appropriate play function based on the parameters set
AudioManager.Instance.PlayLoopingSound // Plays looping sound 
AudioManager.Instance.PlayRandomAudio // Plays random sound 

3b. In the scene, you can use this:
SoundPlayer : Monobehaviour 
// Can be attached to any gameobject to give ambient sounds to that object
// Can also be attached to an animator, create events for various animation clips to play audio there

Tip: If you do not wish to modify a default parameter of a function, use the keyword 'default' in place of the parameter