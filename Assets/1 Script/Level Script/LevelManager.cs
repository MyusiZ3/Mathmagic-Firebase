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
    private int lastCompletedLevel = 0;

    void Start()
    {
        // Inisialisasi Firebase Firestore
        db = FirebaseFirestore.DefaultInstance;

        // Cek level dari Firebase
        CheckLevelProgress();
    }

    private async void CheckLevelProgress()
    {
        try
        {
            DocumentSnapshot snapshot = await db.Collection("users").Document("player1").GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("lastLevel"))
            {
                lastCompletedLevel = snapshot.GetValue<int>("lastLevel");
                Debug.Log($"Last Completed Level: {lastCompletedLevel}");

                UpdateLevelButtons();
            }
            else
            {
                Debug.LogWarning("Field 'lastLevel' not found in Firestore. Ensure data is properly set.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to fetch level data from Firestore: " + e.Message);
        }
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < listButtonLevel.Length; i++)
        {
            // Activate buttons up to the last completed level
            listButtonLevel[i].interactable = i < lastCompletedLevel;
            Debug.Log($"Button Level {i + 1}: {(listButtonLevel[i].interactable ? "Active" : "Inactive")}");
        }
    }

    public void SelectLevel(int levelNumber)
    {
        StartCoroutine(LoadLevelWithDelay($"lvl_{levelNumber}"));
    }

    private System.Collections.IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(1f); // Adjust delay as needed
        SceneManager.LoadScene(levelName);
    }

    public void CompleteLevel(int currentLevel)
    {
        int nextLevel = currentLevel + 1;

        // Update last completed level in Firestore
        DocumentReference docRef = db.Collection("users").Document("player1");
        docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "lastLevel", nextLevel }
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                lastCompletedLevel = nextLevel;
                Debug.Log($"Level {currentLevel} completed. Next level: {nextLevel}");
                UpdateLevelButtons(); // Refresh button states
            }
            else
            {
                Debug.LogError("Failed to save level data to Firestore: " + task.Exception);
            }
        });
    }

    public void ResetLevels()
    {
        StartCoroutine(ResetLevelsWithDelay());
    }

    private System.Collections.IEnumerator ResetLevelsWithDelay()
    {
        // Reset last completed level in Firestore
        DocumentReference docRef = db.Collection("users").Document("player1");
        docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "lastLevel", 1 }
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                lastCompletedLevel = 1;
                Debug.Log("Levels have been reset to level 1.");
                UpdateLevelButtons(); // Refresh button states
            }
            else
            {
                Debug.LogError("Failed to reset level data in Firestore: " + task.Exception);
            }
        });

        yield return new WaitForSeconds(1f); // Add delay if needed
    }
}