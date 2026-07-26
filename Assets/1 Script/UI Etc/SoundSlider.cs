using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundSlider : MonoBehaviour
{
    public static SoundSlider Instance; 

    [Header("Slider References (Opsional, otomatis dicari jika kosong)")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        AudioManager.EnsureInstance();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindAndAssignSliders();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignSliders();
    }

    public void FindAndAssignSliders()
    {
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (Slider slider in sliders)
        {
            string tag = slider.tag;
            string name = slider.gameObject.name.ToLower();

            // Deteksi BGM Slider (Tag 'BGMSlider' / 'VolumeSlider' atau Nama berunsur bgm/music/background)
            if (bgmSlider == null || bgmSlider == slider)
            {
                if (tag == "BGMSlider" || tag == "VolumeSlider" || name.Contains("bgm") || name.Contains("music") || name.Contains("background"))
                {
                    bgmSlider = slider;
                    bgmSlider.value = AudioManager.BGMVolume;
                    bgmSlider.onValueChanged.RemoveAllListeners();
                    bgmSlider.onValueChanged.AddListener(SetBGMVolume);
                    Debug.Log($"[SoundSlider] BGM Slider ditemukan dan di-assign: {slider.gameObject.name}");
                }
            }

            // Deteksi SFX Slider (Tag 'SFXSlider' / 'SoundSlider' atau Nama berunsur sfx/sound/effect/button)
            if (sfxSlider == null || sfxSlider == slider)
            {
                if (tag == "SFXSlider" || tag == "SoundSlider" || name.Contains("sfx") || name.Contains("sound") || name.Contains("effect") || name.Contains("button"))
                {
                    sfxSlider = slider;
                    sfxSlider.value = AudioManager.SFXVolume;
                    sfxSlider.onValueChanged.RemoveAllListeners();
                    sfxSlider.onValueChanged.AddListener(SetSFXVolume);
                    Debug.Log($"[SoundSlider] SFX Slider ditemukan dan di-assign: {slider.gameObject.name}");
                }
            }
        }

        // Fallback jika hanya ada 1 slider umum (VolumeSlider) dan belum ter-assign ke SFX
        if (bgmSlider != null && sfxSlider == null)
        {
            // Jika hanya ada 1 slider umum, pastikan BGM slider tetap terhubung ke AudioManager.SetBGMVolume
            bgmSlider.value = AudioManager.BGMVolume;
        }
    }

    public void OnSettingsOpened()
    {
        FindAndAssignSliders(); 
    }

    public void SetBGMVolume(float volume)
    {
        AudioManager.SetBGMVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        AudioManager.SetSFXVolume(volume);
    }
}
