using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

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

    public void ResetScore()
    {
        currentScore = 0;
        SaveScore();
    }

    private void SaveScore()
    {
        PlayerPrefs.SetInt("SCORE", currentScore);
        PlayerPrefs.Save();
        UpdateScoreInFirestore();
    }

    private void LoadScore()
    {
        currentScore = PlayerPrefs.GetInt("SCORE", 0);
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
