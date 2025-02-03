using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundSlider : MonoBehaviour
{
    public static SoundSlider Instance; 
    private Slider soundSlider;
    [SerializeField] private AudioSource backgroundMusic; // Kembalikan field untuk assign manual

    private const string VolumePrefKey = "BackgroundVolume"; 

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

        // Automatically assign background music if not set
        if (backgroundMusic == null)
        {
            backgroundMusic = FindFirstObjectByType<AudioSource>();
            if (backgroundMusic == null)
            {
                Debug.LogError("Background Music (AudioSource) tidak ditemukan di scene!");
                // Optionally, you can add a fallback here, e.g., create a new AudioSource
            }
        }
    }
    void Start()
    {
        // Load volume dari PlayerPrefs atau set default ke 1.0
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1.0f);
        Debug.Log("Volume yang disimpan: " + savedVolume);
        backgroundMusic.volume = savedVolume;
        
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

    private void FindAndAssignSlider()
    {
        // Cari semua slider di scene (gunakan metode baru)
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slider slider in sliders)
        {
            // Debugging: Print out all sliders found
            Debug.Log("Slider ditemukan: " + slider.name);

            // Pastikan slider memiliki tag "VolumeSlider"
            if (slider.CompareTag("VolumeSlider"))
            {
                soundSlider = slider;
                soundSlider.value = backgroundMusic.volume;
                soundSlider.onValueChanged.RemoveAllListeners(); // Hapus listener lama
                soundSlider.onValueChanged.AddListener(SetVolume); // Tambahkan listener baru
                Debug.Log("Slider ditemukan dan di-assign.");
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
        // Set volume background music dan simpan ke PlayerPrefs
        backgroundMusic.volume = volume;
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
        Debug.Log("Volume diatur ke: " + volume);
    }
}
