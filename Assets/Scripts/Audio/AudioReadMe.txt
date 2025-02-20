- Audio System -



1a. Create a scriptable object Sound Library (Project > Create > Audio > SoundLibrary)
1b. Add your sounds into the list of the scriptable object (Sound name, imported Audio Clip, Audio Priority, Volume)

2a. Create an empty GameObject with AudioManager in your scene.
2b. Add in your Sound Libraries. Optionally you can set:
- Sound Pool size 
- Audio Rolloff  

3. In any script, use any of these:
AudioEventSystem.PlaySound // Plays a SFX
AudioEventSystem.PlayMusic // Plays looping music
AudioEventSystem.PlayAmbience // Plays looping ambience
AudioManager.Instance.PlayLoopingSound // Plays looping sound 