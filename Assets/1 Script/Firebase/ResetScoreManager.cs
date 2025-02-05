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

        DocumentReference docRef = firestore.Collection("users").Document(userId);

        Dictionary<string, object> resetData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "score", 0 },
            { "LEVEL_COMPLETED", FieldValue.Delete } // Hapus LEVEL_COMPLETED
        };

        try
        {
            await docRef.SetAsync(resetData, SetOptions.MergeAll);
            Debug.Log("LEVEL berhasil direset ke 1, score direset ke 0, dan LEVEL_COMPLETED dihapus di Firestore.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Gagal mereset LEVEL, score, dan menghapus LEVEL_COMPLETED di Firestore. Error: {e.Message}");
        }
    }
}
