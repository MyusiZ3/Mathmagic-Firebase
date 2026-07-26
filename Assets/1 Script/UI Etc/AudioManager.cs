using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public const string BGM_PREF_KEY = "BGMVolume";
    public const string SFX_PREF_KEY = "SFXVolume";
    public const string LEGACY_BGM_PREF_KEY = "BackgroundVolume"; // Kompatibilitas mundur

    public static float BGMVolume { get; private set; } = 1.0f;
    public static float SFXVolume { get; private set; } = 1.0f;

    public static event Action<float> OnBGMVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;

    [Header("Default Sound Settings (Optional)")]
    public AudioClip defaultClickSound;

    private static AudioSource sfxAudioSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitOnLoad()
    {
        EnsureInstance();
    }

    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("AudioManager (Auto-Created)");
            Instance = go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        // Load BGM Volume (dengan fallback ke key legacy 'BackgroundVolume')
        if (PlayerPrefs.HasKey(BGM_PREF_KEY))
        {
            BGMVolume = PlayerPrefs.GetFloat(BGM_PREF_KEY, 1.0f);
        }
        else
        {
            BGMVolume = PlayerPrefs.GetFloat(LEGACY_BGM_PREF_KEY, 1.0f);
        }

        // Load SFX Volume
        SFXVolume = PlayerPrefs.GetFloat(SFX_PREF_KEY, 1.0f);

        Debug.Log($"[AudioManager] Loaded Volume Settings - BGM: {BGMVolume}, SFX: {SFXVolume}");
    }

    public static void SetBGMVolume(float volume)
    {
        EnsureInstance();
        BGMVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGM_PREF_KEY, BGMVolume);
        PlayerPrefs.SetFloat(LEGACY_BGM_PREF_KEY, BGMVolume); // Simpan juga di key lama demi kompatibilitas
        PlayerPrefs.Save();

        OnBGMVolumeChanged?.Invoke(BGMVolume);
        Debug.Log($"[AudioManager] BGM Volume diatur ke: {BGMVolume}");
    }

    public static void SetSFXVolume(float volume)
    {
        EnsureInstance();
        SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_PREF_KEY, SFXVolume);
        PlayerPrefs.Save();

        OnSFXVolumeChanged?.Invoke(SFXVolume);
        Debug.Log($"[AudioManager] SFX Volume diatur ke: {SFXVolume}");
    }

    public static void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        EnsureInstance();

        if (sfxAudioSource == null)
        {
            GameObject go = new GameObject("GlobalSFXAudioSource");
            if (Instance != null)
            {
                go.transform.SetParent(Instance.transform);
            }
            sfxAudioSource = go.AddComponent<AudioSource>();
        }

        sfxAudioSource.volume = SFXVolume;
        sfxAudioSource.PlayOneShot(clip, SFXVolume);
    }

    public static void PlaySFX(AudioSource customSource, AudioClip clip)
    {
        if (clip == null) return;
        if (customSource != null)
        {
            customSource.volume = SFXVolume;
            customSource.PlayOneShot(clip, SFXVolume);
        }
        else
        {
            PlaySFX(clip);
        }
    }

    public static void PlayDefaultClickSound()
    {
        if (Instance != null && Instance.defaultClickSound != null)
        {
            PlaySFX(Instance.defaultClickSound);
        }
    }
}
