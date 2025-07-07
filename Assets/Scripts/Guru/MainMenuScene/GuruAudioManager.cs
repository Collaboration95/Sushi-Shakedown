using UnityEngine;
using UnityEngine.Audio;

// alias Unity’s SceneManager so we can keep our class name
using UnityEngine.SceneManagement;
// This is jsut for KitchenSceneManager.cs
public class GuruAudioManager : MonoBehaviour
{
    public AudioSource BackgroundMusicSource;
    public AudioSource SFXSource;
    public AudioClip ButtonClickClip;

    public AudioMixerSnapshot gameplaySnapshot;
    public AudioMixerSnapshot pauseSnapshot;

    private AudioClip[] musicClips;
    void Awake()
    {
        // Load all clips in that Resources folder
        musicClips = Resources.LoadAll<AudioClip>(
            "Audio/Guru/MainMenu/BackgroundTracks"
        );

        if (musicClips == null || musicClips.Length == 0)
            Debug.LogWarning("MainMenuManager: No background tracks found in Resources!");
    }

    public void TransitionToDimmedSnapshot()
    {
        pauseSnapshot.TransitionTo(0.5f);  // 0.5 seconds transition
    }

    public void TransitionToGameplaySnapshot()
    {
        gameplaySnapshot.TransitionTo(0.5f);
    }

    void Start()
    {

        Debug.Log("Starting Main Menu…");
        PlayRandomMusic();
    }

    private void PlayRandomMusic()
    {
        if (BackgroundMusicSource == null || musicClips == null || musicClips.Length == 0)
            return;

        // pick one clip at random
        int idx = Random.Range(0, musicClips.Length);
        AudioClip chosen = musicClips[idx];
        BackgroundMusicSource.clip = chosen;
        BackgroundMusicSource.loop = true;
        BackgroundMusicSource.Play();

        Debug.Log($"Now playing: {chosen.name}");

        // unload all other clips to free memory
        for (int i = 0; i < musicClips.Length; i++)
        {
            if (i == idx) continue;
            Resources.UnloadAsset(musicClips[i]);
        }

        // keep only the chosen clip in the array
        musicClips = new AudioClip[] { chosen };
    }

    public void PlayButtonClickSound()
    {
        Debug.Log("Button click sound played.");
        if (SFXSource != null && ButtonClickClip != null)
        {
            SFXSource.PlayOneShot(ButtonClickClip);
            Debug.Log("Button click sound played.");

        }
    }

}
