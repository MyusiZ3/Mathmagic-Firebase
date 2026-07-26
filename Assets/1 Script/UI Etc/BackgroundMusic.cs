using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }
    private AudioSource audioSource;
    private const string VolumePrefKey = "BackgroundVolume";

    void Awake()
    {
        // Cek apakah sudah ada instance yang sama, jika iya, hancurkan yang baru
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // Simpan instance dan pastikan objek ini tidak hancur saat pindah scene
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Cari AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }

        ApplySavedVolume();
        AudioManager.OnBGMVolumeChanged += OnBGMVolumeChanged;
    }

    void Start()
    {
        ApplySavedVolume();
    }

    private void OnDestroy()
    {
        AudioManager.OnBGMVolumeChanged -= OnBGMVolumeChanged;
    }

    private void OnBGMVolumeChanged(float newVolume)
    {
        if (audioSource != null)
        {
            audioSource.volume = newVolume;
        }
    }

    public void ApplySavedVolume()
    {
        float volume = AudioManager.BGMVolume;
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        // Biarkan AudioListener.volume tetap 1.0f agar BGM dan SFX bisa dikontrol terpisah
        AudioListener.volume = 1.0f;
        Debug.Log($"[BackgroundMusic] BGM Volume diinisialisasi ke: {volume}");
    }
}
