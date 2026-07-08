using UnityEngine;

public class SimpleAnswerManager : MonoBehaviour
{
    public GameObject correctOverlay; // Overlay untuk jawaban benar
    public GameObject wrongOverlay; // Overlay untuk jawaban salah

    [Header("Score Settings")]
    [Tooltip("Jumlah skor yang ditambahkan saat jawaban benar.")]
    public int scoreReward = 10;

    [Tooltip("Jumlah skor yang dikurangi saat jawaban salah.")]
    public int scorePenalty = 5;

    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            ShowCorrectOverlay();
        }
        else
        {
            ShowWrongOverlay();

            // Cari AnswerShuffler untuk mengacak posisi tombol setelah salah
            AnswerShuffler shuffler = GetComponentInChildren<AnswerShuffler>();
            if (shuffler == null) shuffler = GetComponentInParent<AnswerShuffler>();
            if (shuffler != null)
            {
                shuffler.Shuffle();
            }
        }
    }

    private void ShowCorrectOverlay()
    {
        // Tambah skor otomatis ketika jawaban benar
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreReward);
            Debug.Log($"Skor +{scoreReward} ditambahkan secara otomatis lewat SimpleAnswerManager.");
        }

        PageManager pageManager = FindFirstObjectByType<PageManager>();
        if (pageManager != null && pageManager.IsLastQuestion())
        {
            // Jika ini soal terakhir, langsung selesaikan level tanpa menampilkan overlay perantara
            pageManager.NextQuestion();
        }
        else
        {
            correctOverlay.SetActive(true);
            wrongOverlay.SetActive(false); // Sembunyikan overlay salah

            // Hentikan timer level agar tidak memotong waktu saat membaca hasil
            Timer timer = FindFirstObjectByType<Timer>();
            if (timer != null)
            {
                timer.StopTimer();
            }
        }
    }

    private void ShowWrongOverlay()
    {
        wrongOverlay.SetActive(true);
        correctOverlay.SetActive(false); // Sembunyikan overlay benar

        // Kurangi HP secara otomatis saat jawaban salah
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.LoseHealth();
            Debug.Log("HP berkurang -1 otomatis lewat SimpleAnswerManager.");
        }

        // Kurangi skor jika ada penalti
        if (scorePenalty > 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(-scorePenalty);
            Debug.Log($"Skor berkurang -{scorePenalty} karena jawaban salah lewat SimpleAnswerManager.");
        }

        // Hentikan timer level agar tidak memotong waktu saat membaca hasil
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.StopTimer();
        }
    }

    // Fungsi jembatan jika Anda ingin melanjutkan timer secara manual saat menyembunyikan overlay
    public void ResumeLevelTimer()
    {
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.ResumeTimer();
        }
    }
}
