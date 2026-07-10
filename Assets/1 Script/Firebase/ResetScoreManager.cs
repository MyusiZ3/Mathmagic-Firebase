using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;

public class ResetScoreManager : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private string userId;
    private const string SCORE_PREF_KEY = "local_score"; // Key untuk menyimpan skor lokal

    private void Awake()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("UserId", string.Empty);
        if (string.IsNullOrEmpty(userId) && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            PlayerPrefs.SetString("UserId", userId);
            PlayerPrefs.Save();
        }
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan! Pastikan pengguna login.");
        }
    }

    public async void ResetScore()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat mereset skor.");
            return;
        }

        PlayerPrefs.SetInt(SCORE_PREF_KEY, 0);
        PlayerPrefs.Save();
        Debug.Log("Skor lokal direset ke 0.");

        await ResetLevelInFirestore(); // Menunggu hingga Firestore selesai
    }

    private async Task ResetLevelInFirestore()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat mereset level.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = firestore.Collection("users").Document(shortId);

        Dictionary<string, object> resetData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "score", 0 },
            { "LEVEL_COMPLETED", FieldValue.Delete }, // Hapus LEVEL_COMPLETED
            { "BONUS_COMPLETED", FieldValue.Delete } // Hapus BONUS_COMPLETED
        };

        try
        {
            await docRef.SetAsync(resetData, SetOptions.MergeAll);
            Debug.Log("LEVEL berhasil direset ke 1, score direset ke 0, LEVEL_COMPLETED, dan BONUS_COMPLETED dihapus di Firestore.");
            
            // Reset status lokal LevelManager agar langsung sinkron tanpa reload scene
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ResetLocalProgress();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Gagal mereset progress di Firestore. Error: {e.Message}");
        }
    }
}
