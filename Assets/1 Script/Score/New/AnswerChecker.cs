using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class AnswerChecker : MonoBehaviour
{
    public TMP_InputField numeratorInput;  // Input untuk angka atas
    public TMP_InputField denominatorInput; // Input untuk angka bawah
    public int correctNumerator;
    public int correctDenominator;
    public PageManager pageManager;
    public HealthManager healthManager;
    public TextMeshProUGUI timerText; // Menampilkan waktu regenerasi nyawa
    public int pointsPerCorrectAnswer = 10; // Poin tambahan jika benar
    public int pointsPerWrongAnswer = -5;   // Poin dikurangi jika salah
    public ScoreManager scoreManager; // Ambil referensi dari ScoreManager

    private FirebaseFirestore firestore;
    private string userId;
    private const string SCORE_KEY = "score"; // Key di Firestore untuk skor

    private void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("UserId", string.Empty);
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan! Pastikan pengguna login.");
        }

        UpdateTimerText();
    }

    public void CheckAnswer()
    {
        int userNumerator, userDenominator;

        if (int.TryParse(numeratorInput.text, out userNumerator) && int.TryParse(denominatorInput.text, out userDenominator))
        {
            if (userNumerator == correctNumerator && userDenominator == correctDenominator)
            {
                Debug.Log("Jawaban benar! Poin bertambah.");
                scoreManager.AddScore(pointsPerCorrectAnswer); // Tambah skor
                UpdateScoreInFirestore(pointsPerCorrectAnswer); // Simpan ke Firebase
                pageManager.NextQuestion();
            }
            else
            {
                Debug.Log("Jawaban salah! Nyawa berkurang & poin dikurangi.");
                scoreManager.AddScore(pointsPerWrongAnswer); // Kurangi skor
                UpdateScoreInFirestore(pointsPerWrongAnswer); // Simpan ke Firebase
                healthManager.LoseHealth();
                UpdateTimerText();
            }
        }
        else
        {
            Debug.Log("Input tidak valid! Masukkan angka yang benar.");
        }
    }

    private void UpdateTimerText()
    {
        int missingHealth = healthManager.maxHealth - healthManager.GetCurrentHealth();
        int timeToFullRecovery = missingHealth * 5; // 5 menit per nyawa

        if (missingHealth > 0)
        {
            timerText.text = $"Nyawa akan terisi dalam {timeToFullRecovery} menit";
        }
        else
        {
            timerText.text = "";
        }
    }

    private void UpdateScoreInFirestore(int scoreChange)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat memperbarui skor.");
            return;
        }

        DocumentReference docRef = firestore.Collection("users").Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int currentScore = task.Result.ContainsField(SCORE_KEY) ? task.Result.GetValue<int>(SCORE_KEY) : 0;
                int newScore = Mathf.Max(0, currentScore + scoreChange); // Skor tidak boleh negatif

                docRef.UpdateAsync(new Dictionary<string, object> { { SCORE_KEY, newScore } })
                    .ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log($"Skor berhasil diperbarui: {newScore}");
                        }
                        else
                        {
                            Debug.LogError("Gagal memperbarui skor di Firestore.");
                        }
                    });
            }
            else
            {
                Debug.LogError("Dokumen pengguna tidak ditemukan di Firestore.");
            }
        });
    }
}
