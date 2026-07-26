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

    private void OnEnable()
    {
        Debug.Log("[Timer] OnEnable called.");
        if (RemoteSettingsManager.Instance != null)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded += OnRemoteSettingsLoaded;
            Debug.Log("[Timer] Subscribed to OnSettingsLoaded.");
        }
    }

    private void OnDisable()
    {
        Debug.Log("[Timer] OnDisable called.");
        if (RemoteSettingsManager.HasInstance)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded -= OnRemoteSettingsLoaded;
            Debug.Log("[Timer] Unsubscribed from OnSettingsLoaded.");
        }
    }

    void Start()
    {
        Debug.Log("[Timer] Start called.");
        ApplyRemoteSettings();

        // Inisialisasi timer
        stopTimer = false;
        elapsedTime = 0f;

        if (timerSlider != null)
        {
            timerSlider.minValue = 0f; // Mulai dari 0
            timerSlider.maxValue = gameTime > 0f ? gameTime : 60f; // Set nilai maksimal slider ke waktu permainan (fallback jika 0)
            timerSlider.value = 0f; // Set slider ke posisi kosong di awal
        }
        else
        {
            Debug.LogWarning("[Timer] timerSlider belum di-assign di Inspector!");
        }

        // Pastikan waktu berjalan normal (timeScale = 1) saat level dimulai
        Time.timeScale = 1f;

        // Pastikan overlay "Time Over" dan lainnya dikelola oleh OverlayManager
        if (OverlayManager.Instance != null)
        {
            OverlayManager.Instance.SetGameState(GameState.Playing);
        }
    }

    private void OnRemoteSettingsLoaded()
    {
        Debug.Log("[Timer] OnRemoteSettingsLoaded event received!");
        ApplyRemoteSettings();
        if (timerSlider != null)
        {
            timerSlider.maxValue = gameTime > 0f ? gameTime : 60f;
            Debug.Log($"[Timer] Set slider maxValue to: {timerSlider.maxValue}");
        }
    }

    private void ApplyRemoteSettings()
    {
        if (RemoteSettingsManager.Instance != null)
        {
            float remoteTime = RemoteSettingsManager.Instance.questionTimerSeconds;
            if (remoteTime > 0f)
            {
                gameTime = remoteTime;
            }
            else
            {
                // Fallback jika remote settings belum ter-load atau bernilai 0
                gameTime = RemoteSettingsManager.Instance.defaultQuestionTimerSeconds > 0f ? RemoteSettingsManager.Instance.defaultQuestionTimerSeconds : 60f;
            }
            Debug.Log($"[Timer] Applied Remote Settings: gameTime={gameTime}");
        }
    }

    void Update()
    {
        // 1. Cek apakah HP habis (0)
        bool isHealthZero = HealthManager.HasInstance && HealthManager.Instance != null && HealthManager.Instance.CurrentHealth == 0;

        // 2. Cek apakah state sedang Playing (jika menggunakan OverlayManager)
        bool isPlayingState = true;
        if (OverlayManager.Instance != null)
        {
            isPlayingState = OverlayManager.Instance.GetCurrentState() == GameState.Playing;
        }

        // 3. Cek apakah level sudah selesai
        bool isLevelCompleted = false;
        PageManager pageManager = FindFirstObjectByType<PageManager>();
        if (pageManager != null && pageManager.IsLevelCompleted)
        {
            isLevelCompleted = true;
        }

        // 4. Cek juga apakah ada overlay dari OverlayAnswer yang aktif
        bool isOverlayActive = false;
        OverlayAnswer overlayAnswer = FindFirstObjectByType<OverlayAnswer>();
        if (overlayAnswer != null)
        {
            bool correctActive = overlayAnswer.correctOverlay != null && overlayAnswer.correctOverlay.activeInHierarchy;
            bool wrongActive = overlayAnswer.wrongOverlay != null && overlayAnswer.wrongOverlay.activeInHierarchy;
            isOverlayActive = correctActive || wrongActive;
        }

        if (OverlayManager.Instance != null)
        {
            bool managerCorrectActive = OverlayManager.Instance.answerCorrectOverlay != null && OverlayManager.Instance.answerCorrectOverlay.activeInHierarchy;
            bool managerWrongActive = OverlayManager.Instance.answerWrongOverlay != null && OverlayManager.Instance.answerWrongOverlay.activeInHierarchy;
            if (managerCorrectActive || managerWrongActive)
            {
                isOverlayActive = true;
            }
        }

        // Auto-recovery: Jika overlay sudah tidak aktif di hierarki tapi GameState masih AnswerCorrect/AnswerWrong,
        // dan level belum selesai, kembalikan state ke Playing agar Time.timeScale kembali 1f dan timer berjalan.
        if (!isOverlayActive && !isLevelCompleted && OverlayManager.Instance != null)
        {
            GameState state = OverlayManager.Instance.GetCurrentState();
            if (state == GameState.AnswerCorrect || state == GameState.AnswerWrong)
            {
                Debug.Log($"[Timer] Answer overlay closed, but GameState was still {state}. Auto-restoring GameState to Playing.");
                OverlayManager.Instance.SetGameState(GameState.Playing);
                isPlayingState = true;
            }
        }

        // Pastikan Time.timeScale = 1f jika state Playing dan tidak ada overlay/pause yang aktif
        if (isPlayingState && !isOverlayActive && !isLevelCompleted && !isHealthZero && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }

        // Jika HP habis, level selesai, state tidak bermain, atau overlay aktif, pastikan stopTimer bernilai true
        if (isHealthZero || isLevelCompleted || !isPlayingState || isOverlayActive)
        {
            stopTimer = true;
        }
        else
        {
            // Jika semua kondisi normal, game state Playing, level belum selesai, dan tidak ada overlay aktif,
            // otomatis pulihkan stopTimer ke false jika sebelumnya terhenti sementara karena overlay
            if (stopTimer)
            {
                stopTimer = false;
            }
        }

        float maxTime = gameTime > 0f ? gameTime : 60f;

        // Timer hanya bertambah jika tidak di-stop, HP tidak 0, level belum selesai, state bermain, dan tidak ada overlay aktif
        if (!stopTimer && !isHealthZero && isPlayingState && !isOverlayActive && !isLevelCompleted && maxTime > 0f)
        {
            // Hitung waktu yang telah berlalu
            elapsedTime += Time.deltaTime;
            float timeRemaining = maxTime - elapsedTime;

            // Perbarui UI slider selama waktu belum habis
            if (timeRemaining > 0)
            {
                if (timerSlider != null)
                {
                    timerSlider.value = elapsedTime;
                }
            }
            else
            {
                // Ketika waktu habis
                stopTimer = true;
                if (timerSlider != null)
                {
                    timerSlider.value = maxTime;
                }

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
        PageManager pageManager = FindFirstObjectByType<PageManager>();
        if (pageManager != null && pageManager.IsLevelCompleted)
        {
            stopTimer = true;
            return;
        }
        stopTimer = false;
    }

    // Fungsi untuk Reset Timer (Opsional)
    public void ResetTimer()
    {
        elapsedTime = 0f;
        stopTimer = false;
        if (timerSlider != null)
        {
            timerSlider.value = 0f;
        }
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
