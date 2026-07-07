using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LevelCompletion : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;

    async void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            userId = user.UserId; // Ambil ID pengguna yang sudah login
            await CheckAndUpdateUserData(); // Pastikan data pengguna tersedia di Firestore
        }
        else
        {
            Debug.LogError("User belum login! Pastikan login terlebih dahulu.");
        }
    }

    private async Task CheckAndUpdateUserData()
    {
        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = db.Collection("users").Document(shortId);
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            // Tambahkan data default untuk pengguna baru
            await userRef.SetAsync(new Dictionary<string, object>
            {
                { "LEVEL", 1 },
                { "score", 0 },
                { "LEVEL_COMPLETED", new Dictionary<string, object>() } // Inisialisasi LEVEL_COMPLETED
            });

            Debug.Log("Data pengguna baru dibuat di Firestore.");
        }
        else
        {
            Debug.Log("Data pengguna ditemukan di Firestore.");
        }
    }
    // Modikasi levelcomplete
    public async void CompleteLevel(int levelNumber)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Pastikan pengguna telah login.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = db.Collection("users").Document(shortId);

        // Ambil data pengguna dari Firestore
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            Debug.LogError("Pengguna tidak ditemukan di Firestore.");
            return;
        }

        // Periksa apakah level sudah selesai di Firestore
        var levelCompleted = snapshot.GetValue<Dictionary<string, object>>("LEVEL_COMPLETED");
        if (levelCompleted.ContainsKey(levelNumber.ToString()) && (bool)levelCompleted[levelNumber.ToString()])
        {
            Debug.Log($"Level {levelNumber} sudah selesai sebelumnya.");
            return; // Jika level sudah selesai, jangan lakukan update lagi
        }

        // Menandai level sebagai selesai
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"LEVEL_COMPLETED.{levelNumber}", true },
            { "LEVEL", levelNumber + 1 } // Menambah level setelah level selesai
        };

        // Update data di Firestore
        await userRef.UpdateAsync(updates);

        // Debug log untuk mengecek
        Debug.Log($"Level {levelNumber} completed. Next level: {levelNumber + 1}");

        // Panggil CompleteLevel dari LevelManager untuk memperbarui UI
        LevelManager.Instance?.CompleteLevel(levelNumber);
    }


    // OLD CODE
    // public async void CompleteLevel(int levelNumber)
    // {
    //     if (string.IsNullOrEmpty(userId))
    //     {
    //         Debug.LogError("User ID tidak ditemukan. Pastikan pengguna telah login.");
    //         return;
    //     }

    //     DocumentReference userRef = db.Collection("users").Document(userId);

    //     // Menandai level sebagai selesai
    //     Dictionary<string, object> updates = new Dictionary<string, object>
    //     {
    //         { $"LEVEL_COMPLETED.{levelNumber}", true },
    //         { "LEVEL", levelNumber + 1 }
    //     };

    //     await userRef.UpdateAsync(updates);

    //     Debug.Log($"Level {levelNumber} completed. Next level: {levelNumber + 1}");

    //     // Panggil CompleteLevel dari LevelManager untuk memperbarui UI
    //     LevelManager.Instance?.CompleteLevel(levelNumber);
    // }


}