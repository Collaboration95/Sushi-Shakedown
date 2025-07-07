using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    // Keeps track of looping sounds (e.g., grills)
    private Dictionary<Transform, AudioSource> loopAudioSources = new();

    [SerializeField] private AudioSource oneShotSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // One-shot SFX event subscriptions
        EventManager.Instance.Subscribe<object>("ObjectCutAudio", OnCut);
        EventManager.Instance.Subscribe<object>("ObjectTrashedAudio", OnTrash);
        EventManager.Instance.Subscribe<object>("AddPlateIngredientAudio", OnAddPlateIngredient);
        EventManager.Instance.Subscribe<object>("AddCupIngredientAudio", OnAddCupIngredient);
        EventManager.Instance.Subscribe<object>("PlaceItemAudio", OnPlaceItem);

        // Looping SFX (grill) event subscriptions
        EventManager.Instance.Subscribe<object>("GrillStartAudioLoop", OnGrillStart);
        EventManager.Instance.Subscribe<object>("GrillStopAudioLoop", OnGrillStop);
    }



    private void OnDestroy()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.Unsubscribe<object>("ObjectCutAudio", OnCut);
        EventManager.Instance.Unsubscribe<object>("ObjectTrashedAudio", OnTrash);
        EventManager.Instance.Unsubscribe<object>("AddPlateIngredientAudio", OnAddPlateIngredient);
        EventManager.Instance.Unsubscribe<object>("AddCupIngredientAudio", OnAddCupIngredient);
        EventManager.Instance.Unsubscribe<object>("GrillStartAudioLoop", OnGrillStart);
        EventManager.Instance.Unsubscribe<object>("GrillStopAudioLoop", OnGrillStop);
        EventManager.Instance.Unsubscribe<object>("PlaceItemAudio", OnPlaceItem);

    }

    // ����� One-Shot SFX �����
    private void OnPlaceItem(object sender)
    {
        PlayOneShot(audioClipRefsSO.addPlateIngredient, GetSenderPos(sender));
    }
    private void OnCut(object sender)
    {
        PlayOneShot(audioClipRefsSO.cut, GetSenderPos(sender));
    }

    private void OnTrash(object sender)
    {
        PlayOneShot(audioClipRefsSO.trash, GetSenderPos(sender));
    }

    private void OnAddPlateIngredient(object sender)
    {
        PlayOneShot(audioClipRefsSO.addPlateIngredient, GetSenderPos(sender));
    }

    private void OnAddCupIngredient(object sender)
    {
        PlayOneShot(audioClipRefsSO.addCupIngredient, GetSenderPos(sender));
    }

    // ����� Looping Grill SFX �����

    private void OnGrillStart(object sender)
    {
        Transform grillTransform = (sender as MonoBehaviour)?.transform;
        if (grillTransform == null || loopAudioSources.ContainsKey(grillTransform)) return;

        GameObject audioGO = new GameObject("GrillLoopSFX");
        audioGO.transform.position = grillTransform.position;

        AudioSource source = audioGO.AddComponent<AudioSource>();
        source.clip = audioClipRefsSO.grill;
        source.loop = true;
        source.spatialBlend = 1f;
        source.Play();

        loopAudioSources[grillTransform] = source;
    }

    private void OnGrillStop(object sender)
    {
        Transform grillTransform = (sender as MonoBehaviour)?.transform;
        if (grillTransform == null || !loopAudioSources.TryGetValue(grillTransform, out AudioSource source)) return;

        source.Stop();
        Destroy(source.gameObject);
        loopAudioSources.Remove(grillTransform);
    }

    // ����� Helper Methods �����

    private void PlayOneShot(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null && oneShotSource != null)
        {
            oneShotSource.transform.position = position;
            oneShotSource.PlayOneShot(clip, volume);
        }
    }

    private Vector3 GetSenderPos(object sender)
    {
        return (sender as MonoBehaviour)?.transform.position ?? Vector3.zero;
    }
}
