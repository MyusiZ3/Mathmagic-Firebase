using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public Button[] listButtonLevel; // Daftar tombol level
    private FirebaseFirestore db;
    private int currentLevel = 1; // Level saat ini
    private Dictionary<string, object> levelCompleted = new Dictionary<string, object>(); // Status level yang diselesaikan

    void Start()
    {
        // Inisialisasi Firebase Firestore
        db = FirebaseFirestore.DefaultInstance;

        // Cek progress level dari Firestore
        CheckLevelProgress();
    }

    private async void CheckLevelProgress()
    {
        try
        {
            DocumentSnapshot snapshot = await db.Collection("users").Document("player1").GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Ambil data LEVEL dan LEVEL_COMPLETED dari Firestore
                if (snapshot.ContainsField("LEVEL"))
                {
                    currentLevel = snapshot.GetValue<int>("LEVEL");
                }

                if (snapshot.ContainsField("LEVEL_COMPLETED"))
                {
                    levelCompleted = snapshot.GetValue<Dictionary<string, object>>("LEVEL_COMPLETED");
                }

                Debug.Log($"Current Level: {currentLevel}");
                UpdateLevelButtons();
            }
            else
            {
                Debug.LogWarning("Data pengguna tidak ditemukan di Firestore.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gagal mengambil data level dari Firestore: " + e.Message);
        }

    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < listButtonLevel.Length; i++)
        {
            // Tombol level aktif jika level sudah diselesaikan atau level saat ini
            bool isLevelCompleted = levelCompleted.ContainsKey((i + 1).ToString()) && (bool)levelCompleted[(i + 1).ToString()];
            listButtonLevel[i].interactable = isLevelCompleted || (i + 1) == currentLevel;
            Debug.Log($"Button Level {i + 1}: {(listButtonLevel[i].interactable ? "Active" : "Inactive")}");
        }
    }

    public void SelectLevel(int levelNumber)
    {
        StartCoroutine(LoadLevelWithDelay($"lvl_{levelNumber}"));
    }
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    private System.Collections.IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(1f); // Sesuaikan delay jika diperlukan
        SceneManager.LoadScene(levelName);
    }

    public async void CompleteLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > listButtonLevel.Length)
        {
            Debug.LogError("Nomor level tidak valid.");
            return;
        }

        // Update LEVEL_COMPLETED di Firestore
        DocumentReference docRef = db.Collection("users").Document("player1");
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"LEVEL_COMPLETED.{levelNumber}", true },
            { "LEVEL", levelNumber + 1 } // Update ke level berikutnya
        };

        await docRef.UpdateAsync(updates);

        // Perbarui UI
        currentLevel = levelNumber + 1;
        levelCompleted[levelNumber.ToString()] = true;
        UpdateLevelButtons();

        Debug.Log($"Level {levelNumber} selesai. Level berikutnya: {currentLevel}");
    }

    public async void ResetLevels()
    {
        DocumentReference docRef = db.Collection("users").Document("player1");
        Dictionary<string, object> resetData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "LEVEL_COMPLETED", new Dictionary<string, object>() }
        };

        await docRef.SetAsync(resetData, SetOptions.MergeAll);

        // Perbarui UI
        currentLevel = 1;
        levelCompleted.Clear();
        UpdateLevelButtons();

        Debug.Log("Level telah direset ke level 1.");
    }
}