using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    // UI Components
    public Slider timerSlider; // Slider yang akan mengisi seiring waktu
    public float gameTime = 60f; // Waktu total untuk level dalam detik

    private bool stopTimer = false;
    private float elapsedTime = 0f; // Waktu yang telah berlalu

    void Start()
    {
        ApplyRemoteSettings();
        if (RemoteSettingsManager.Instance != null)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded += OnRemoteSettingsLoaded;
        }

        // Inisialisasi timer
        stopTimer = false;
        timerSlider.minValue = 0f; // Mulai dari 0
        timerSlider.maxValue = gameTime; // Set nilai maksimal slider ke waktu permainan
        timerSlider.value = 0f; // Set slider ke posisi kosong di awal

        // Pastikan overlay "Time Over" dan lainnya dikelola oleh OverlayManager
        if (OverlayManager.Instance != null)
        {
            OverlayManager.Instance.SetGameState(GameState.Playing);
        }
    }

    void OnDestroy()
    {
        if (RemoteSettingsManager.HasInstance)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded -= OnRemoteSettingsLoaded;
        }
    }

    private void OnRemoteSettingsLoaded()
    {
        ApplyRemoteSettings();
        if (timerSlider != null)
        {
            timerSlider.maxValue = gameTime;
        }
    }

    private void ApplyRemoteSettings()
    {
        if (RemoteSettingsManager.Instance != null)
        {
            gameTime = RemoteSettingsManager.Instance.questionTimerSeconds;
            Debug.Log($"[Timer] Applied Remote Settings: gameTime={gameTime}");
        }
    }

    void Update()
    {
        // 1. Cek apakah HP habis (0)
        bool isHealthZero = HealthManager.HasInstance && HealthManager.Instance.CurrentHealth == 0;

        // 2. Cek apakah state sedang Playing (jika menggunakan OverlayManager)
        bool isPlayingState = true;
        if (OverlayManager.Instance != null)
        {
            isPlayingState = OverlayManager.Instance.GetCurrentState() == GameState.Playing;
        }

        // Timer hanya bertambah jika tidak di-stop, HP tidak 0, dan state adalah Playing
        if (!stopTimer && !isHealthZero && isPlayingState)
        {
            // Hitung waktu yang telah berlalu
            elapsedTime += Time.deltaTime;
            float timeRemaining = gameTime - elapsedTime;

            // Perbarui UI slider selama waktu belum habis
            if (timeRemaining > 0)
            {
                timerSlider.value = elapsedTime;
            }
            else
            {
                // Ketika waktu habis
                stopTimer = true;
                timerSlider.value = gameTime;

                // Tampilkan overlay "Time Over"
                if (OverlayManager.Instance != null)
                {
                    OverlayManager.Instance.SetGameState(GameState.TimeOver);
                }
            }
        }
    }

    // Fungsi untuk menghentikan timer
    public void StopTimer()
    {
        stopTimer = true;
    }

    // Fungsi untuk melanjutkan timer
    public void ResumeTimer()
    {
        stopTimer = false;
    }

    // Fungsi untuk Reset Timer (Opsional)
    public void ResetTimer()
    {
        elapsedTime = 0f;
        stopTimer = false;
        timerSlider.value = 0f;
        if (OverlayManager.Instance != null)
        {
            OverlayManager.Instance.SetGameState(GameState.Playing);
        }
    }

    // Fungsi untuk Restart Level (Opsional)
    public void RestartLevel()
    {
        ResetTimer();
        // Reload scene saat ini
        UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }

    // Fungsi untuk Keluar dari Game (Opsional)
    public void QuitGame()
    {
        // Keluar dari aplikasi (hanya berfungsi pada build)
        Application.Quit();

        // Untuk editor Unity, hentikan play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
