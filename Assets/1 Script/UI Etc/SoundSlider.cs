using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundSlider : MonoBehaviour
{
    public static SoundSlider Instance; 
    private Slider soundSlider;

    private const string VolumePrefKey = "BackgroundVolume"; // Tetap gunakan key yang sama agar kompatibel

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Terapkan volume global segera pada startup
        ApplyGlobalVolume();
    }

    void Start()
    {
        ApplyGlobalVolume();
        
        // Daftarkan event untuk scene loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Cari dan assign slider di scene saat ini
        FindAndAssignSlider();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cari dan assign slider setiap kali scene dimuat
        FindAndAssignSlider(); 
    }

    private void ApplyGlobalVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1.0f);
        AudioListener.volume = savedVolume;
        Debug.Log("[SoundSlider] Global volume diinisialisasi ke: " + savedVolume);
    }

    private void FindAndAssignSlider()
    {
        // Cari semua slider di scene
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slider slider in sliders)
        {
            // Pastikan slider memiliki tag "VolumeSlider"
            if (slider.CompareTag("VolumeSlider"))
            {
                soundSlider = slider;
                // Nilai slider disesuaikan dengan volume AudioListener global saat ini
                soundSlider.value = AudioListener.volume;
                soundSlider.onValueChanged.RemoveAllListeners(); // Hapus listener lama
                soundSlider.onValueChanged.AddListener(SetVolume); // Tambahkan listener baru
                Debug.Log("Universal Volume Slider ditemukan dan di-assign.");
                break;
            }
        }

        if (soundSlider == null)
        {
            Debug.LogWarning("Slider dengan tag 'VolumeSlider' tidak ditemukan di scene ini.");
        }
    }

    public void OnSettingsOpened()
    {
        // Cari ulang slider saat menu setting dibuka
        FindAndAssignSlider(); 
    }

    void SetVolume(float volume)
    {
        // Set global volume dan simpan ke PlayerPrefs
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
        Debug.Log("Global Master Volume diatur ke: " + volume);
    }
}
