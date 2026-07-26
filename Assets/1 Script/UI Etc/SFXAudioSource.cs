using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXAudioSource : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ApplyVolume(AudioManager.SFXVolume);
        AudioManager.OnSFXVolumeChanged += ApplyVolume;
    }

    private void OnDestroy()
    {
        AudioManager.OnSFXVolumeChanged -= ApplyVolume;
    }

    private void ApplyVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
