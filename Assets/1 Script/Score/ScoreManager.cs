using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;  // Menambahkan referensi ke Task

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;
    private FirebaseFirestore firestore;
    private string userId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            firestore = FirebaseFirestore.DefaultInstance;

            userId = PlayerPrefs.GetString("UserId", string.Empty);
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("User ID tidak ditemukan! Pastikan pengguna login.");
            }

            LoadScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        SaveScore();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    // Reset score
    public void ResetScore()
    {
        currentScore = 0;
        SaveScore();
    }

    // Reset game data (score and level)
    public async void ResetGame()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat menyetel ulang level.");
            return;
        }

        // Reset skor lokal
        ResetScore();

        // Reset level dan status level di Firebase
        Dictionary<string, object> resetData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "score", 0 },  // Mengganti SCORE menjadi score
            { "LEVEL_COMPLETED", new Dictionary<string, object>() }
        };

        DocumentReference userRef = firestore.Collection("users").Document(userId);
        await userRef.SetAsync(resetData);  // Menggunakan await di sini untuk memastikan eksekusi selesai

        Debug.Log("Game has been reset to level 1 and score 0.");
    }

    private void SaveScore()
    {
        PlayerPrefs.SetInt("score", currentScore);  // Mengganti "SCORE" menjadi "score"
        PlayerPrefs.Save();
        UpdateScoreInFirestore();
    }

    private void LoadScore()
    {
        currentScore = PlayerPrefs.GetInt("score", 0);  // Mengganti "SCORE" menjadi "score"
        Debug.Log("Score loaded: " + currentScore);
    }

    private void UpdateScoreInFirestore()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat menyimpan skor ke Firestore.");
            return;
        }

        DocumentReference docRef = firestore.Collection("users").Document(userId);
        docRef.UpdateAsync(new Dictionary<string, object> { { "score", currentScore } }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Skor berhasil diperbarui di Firestore.");
            }
            else
            {
                Debug.LogError("Gagal memperbarui skor di Firestore.");
            }
        });
    }
}
