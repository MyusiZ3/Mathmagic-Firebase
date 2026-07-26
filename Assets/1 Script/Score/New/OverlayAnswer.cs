using UnityEngine;

public class OverlayAnswer : MonoBehaviour
{
    [Header("Overlay Panels")]
    public GameObject correctOverlay;  // Panel untuk jawaban benar
    public GameObject wrongOverlay;    // Panel untuk jawaban salah

    [Header("Audio Clips")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    private void Start()
    {
        // Pastikan overlay jawaban tidak aktif di awal scene agar timer bisa berjalan normal
        HideAllOverlays();
    }
    public void ShowCorrectOverlay()
    {
        ShowCorrectOverlay(10); // Default score reward adalah 10
    }

    public void ShowCorrectOverlay(int scoreAmount)
    {
        // Tambah skor otomatis ketika jawaban benar
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreAmount);
            Debug.Log($"Skor +{scoreAmount} ditambahkan secara otomatis lewat OverlayAnswer.");
        }

        PageManager pageManager = FindFirstObjectByType<PageManager>();
        if (pageManager != null && pageManager.IsLastQuestion())
        {
            // Jika ini soal terakhir, langsung selesaikan level tanpa menampilkan overlay perantara
            PlaySound(correctSound);
            pageManager.NextQuestion();
        }
        else
        {
            HideAllOverlays(); // Pastikan overlay lain mati sebelum menampilkan yang benar
            correctOverlay.SetActive(true);
            PlaySound(correctSound);

            // Hentikan timer level agar tidak memotong waktu saat membaca hasil
            Timer timer = FindFirstObjectByType<Timer>();
            if (timer != null)
            {
                timer.StopTimer();
            }
        }
    }

    public void ShowWrongOverlay()
    {
        ShowWrongOverlay(0); // Default penalty adalah 0
    }

    public void ShowWrongOverlay(int scorePenalty)
    {
        HideAllOverlays(); // Pastikan overlay lain mati sebelum menampilkan yang salah
        wrongOverlay.SetActive(true);
        PlaySound(wrongSound);

        // Kurangi HP secara otomatis saat jawaban salah
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.LoseHealth();
            Debug.Log("HP berkurang -1 otomatis lewat OverlayAnswer.");
        }

        // Kurangi skor jika ada penalti
        if (scorePenalty > 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(-scorePenalty);
            Debug.Log($"Skor berkurang -{scorePenalty} karena jawaban salah.");
        }

        // Hentikan timer level agar tidak memotong waktu saat membaca hasil
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.StopTimer();
        }
    }

    public void HideAllOverlays()
    {
        correctOverlay?.SetActive(false);
        wrongOverlay?.SetActive(false);

        PageManager pageManager = FindFirstObjectByType<PageManager>();
        bool isLevelCompleted = pageManager != null && pageManager.IsLevelCompleted;

        // Lanjutkan kembali timer level saat overlay ditutup (kecuali jika level sudah selesai)
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            if (isLevelCompleted)
            {
                timer.StopTimer();
            }
            else
            {
                timer.ResumeTimer();
            }
        }

        // Kembalikan status game ke Playing agar Time.timeScale kembali ke 1.0f dan isPlayingState bernilai true
        // Hanya jika level belum selesai
        if (!isLevelCompleted && OverlayManager.Instance != null)
        {
            OverlayManager.Instance.SetGameState(GameState.Playing);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.PlaySFX(audioSource, clip);
        }
    }
}
