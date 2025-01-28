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
            await CheckAndUpdateUserData(); // Contoh panggilan operasi asinkron
        }
        else
        {
            Debug.LogError("User belum login! Pastikan login terlebih dahulu.");
        }
    }

    private async Task CheckAndUpdateUserData()
    {
        DocumentReference userRef = db.Collection("users").Document(userId);
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

    public async void CompleteLevel(int levelNumber)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Pastikan pengguna telah login.");
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(userId);

        // Menandai level sebagai selesai
        await userRef.UpdateAsync($"LEVEL_COMPLETED.{levelNumber}", true);

        // Update level berikutnya
        int nextLevel = levelNumber + 1;
        await userRef.UpdateAsync("LEVEL", nextLevel);

        Debug.Log($"Level {levelNumber} completed. Next level: {nextLevel}");
    }
}
