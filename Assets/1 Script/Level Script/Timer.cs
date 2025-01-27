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

    void Update()
    {
        if (!stopTimer && OverlayManager.Instance.GetCurrentState() == GameState.Playing)
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
