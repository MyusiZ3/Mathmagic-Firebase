using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons; // Array tombol level
    public Text scoreText; // Teks untuk menampilkan skor
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        // Autentikasi pengguna secara anonim
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                userId = auth.CurrentUser.UserId;
                Debug.Log("User authenticated with ID: " + userId);
                CheckLevels(); // Cek status level dari Firebase
            }
            else
            {
                Debug.LogError("Authentication failed: " + task.Exception);
            }
        });
    }

    /// <summary>
    /// Cek status level dan skor dari Firebase.
    /// </summary>
    private async void CheckLevels()
    {
        DocumentReference userRef = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            // Ambil level terakhir dan status level selesai
            int currentLevel = snapshot.GetValue<int>("LEVEL");
            int score = snapshot.GetValue<int>("score");
            Dictionary<string, object> levelCompleted = snapshot.GetValue<Dictionary<string, object>>("LEVEL_COMPLETED");

            // Sesuaikan tombol level berdasarkan data Firebase
            for (int i = 0; i < levelButtons.Length; i++)
            {
                bool isCompleted = levelCompleted != null && levelCompleted.ContainsKey((i + 1).ToString()) && (bool)levelCompleted[(i + 1).ToString()];
                levelButtons[i].interactable = i + 1 <= currentLevel || isCompleted;
            }

            // Menampilkan skor di UI
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }
        else
        {
            Debug.Log("User data not found, initializing...");
            await InitializeUserData(); // Inisialisasi data pengguna jika belum ada
        }
    }

    /// <summary>
    /// Inisialisasi data pengguna baru di Firebase.
    /// </summary>
    private async Task InitializeUserData()
    {
        DocumentReference userRef = db.Collection("users").Document(userId);
        Dictionary<string, object> initialData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "score", 0 },
            { "LEVEL_COMPLETED", new Dictionary<string, object>() }
        };
        await userRef.SetAsync(initialData);
        Debug.Log("User data initialized.");
    }

    /// <summary>
    /// Tandai level selesai dan buka level berikutnya.
    /// </summary>
    /// <param name="currentLevel">Level saat ini.</param>
    public async void CompleteLevel(int currentLevel)
    {
        DocumentReference userRef = db.Collection("users").Document(userId);

        // Tandai level selesai dan buka level berikutnya
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"LEVEL_COMPLETED.{currentLevel}", true },
            { "LEVEL", currentLevel + 1 }
        };

        await userRef.UpdateAsync(updates);

        Debug.Log($"Level {currentLevel} completed. Next level unlocked.");
        CheckLevels(); // Update UI level
    }

    /// <summary>
    /// Reset level dan skor pengguna di Firebase, tanpa menghapus data lainnya.
    /// </summary>
    public async void ResetLevelsAndScore()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat mereset data.");
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(userId);

        // Setel ulang level, skor, dan status level selesai
        Dictionary<string, object> resetData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "score", 0 },
            { "LEVEL_COMPLETED", new Dictionary<string, object>() }
        };

        await userRef.SetAsync(resetData, SetOptions.MergeFields("LEVEL", "score", "LEVEL_COMPLETED"));
        Debug.Log("Levels and score reset.");
        CheckLevels(); // Update UI level
    }
}
