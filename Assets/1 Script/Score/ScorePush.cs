using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;

public class ScorePush : MonoBehaviour
{
    public int scoreValue = 10; // Nilai skor yang ditambahkan atau dikurangi
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;
    private bool isScoreIncrease = true; // Default adalah menambah skor. Set ke false untuk mengurangi skor.

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            userId = user.UserId; // Ambil ID pengguna yang sudah login
        }
        else
        {
            Debug.LogError("User belum login! Pastikan login terlebih dahulu.");
        }
    }

    public void OnButtonClicked()
    {
        if (isScoreIncrease)
        {
            AddScore(scoreValue); // Menambah skor
        }
        else
        {
            SubtractScore(scoreValue); // Mengurangi skor
        }
    }

    private async void AddScore(int scoreToAdd)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat memperbarui skor.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = db.Collection("users").Document(shortId);

        // Update skor di Firestore
        await userRef.UpdateAsync("score", Firebase.Firestore.FieldValue.Increment(scoreToAdd));
        Debug.Log($"Skor berhasil ditambahkan: {scoreToAdd}");
    }

    private async void SubtractScore(int scoreToSubtract)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat memperbarui skor.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = db.Collection("users").Document(shortId);

        // Mengurangi skor di Firestore
        await userRef.UpdateAsync("score", Firebase.Firestore.FieldValue.Increment(-scoreToSubtract));
        Debug.Log($"Skor berhasil dikurangi: {scoreToSubtract}");
    }

    public void SetScoreIncrease(bool increase)
    {
        isScoreIncrease = increase; // Set apakah menambah atau mengurangi skor
    }
}
