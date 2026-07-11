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
    }

    void Start()
    {
        ApplySavedVolume();
    }

    public void ApplySavedVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1.0f);
        AudioListener.volume = savedVolume;
        Debug.Log($"[BackgroundMusic] Global Master Volume diinisialisasi ke: {savedVolume}");
    }
}
