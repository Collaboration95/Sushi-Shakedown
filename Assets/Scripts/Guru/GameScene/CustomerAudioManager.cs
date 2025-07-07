using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class CustomerAudioManager : MonoBehaviour
{
    public AudioSource BackgroundMusicSource;
    public AudioSource SFXSource;
    public AudioClip ButtonClickClip;
    public AudioClip CoinsClip;

    private AudioClip[] musicClips;
    public AudioMixerSnapshot gameplaySnapshot;
    public AudioMixerSnapshot pauseSnapshot;
    private Coroutine musicRoutine;

    void Awake()
    {
        // load all clips once
        musicClips = Resources.LoadAll<AudioClip>(
            "Audio/Guru/MainMenu/BackgroundTracks"
        );

        if (musicClips == null || musicClips.Length == 0)
            Debug.LogWarning("CustomerAudioManager: No background tracks found!");
    }
    void Start()
    {
        if (BackgroundMusicSource != null && musicClips.Length > 0)
            StartCoroutine(PlayMusicLoop());
    }
    private IEnumerator PlayMusicLoop()
    {
        // 1) pick one random clip
        int idx = Random.Range(0, musicClips.Length);
        AudioClip clip = musicClips[idx];

        // 2) assign it, turn on loop, and play
        BackgroundMusicSource.clip = clip;
        BackgroundMusicSource.loop = true;
        BackgroundMusicSource.Play();

        // 3) we’re done—exit the coroutine
        yield break;
    }

    public void TransitionToDimmedSnapshot()
    {
        pauseSnapshot.TransitionTo(0.5f);  // 0.5 seconds transition
    }

    public void TransitionToGameplaySnapshot()
    {
        gameplaySnapshot.TransitionTo(0.5f);
    }



    public void PlayButtonClickSound()
    {
        if (SFXSource != null && ButtonClickClip != null)
            SFXSource.PlayOneShot(ButtonClickClip);
    }

    public void PlayCoinsSound()
    {
        if (SFXSource != null && CoinsClip != null)
            SFXSource.PlayOneShot(CoinsClip);
    }
}
