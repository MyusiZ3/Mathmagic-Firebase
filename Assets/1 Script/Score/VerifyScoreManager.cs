using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;

public class VerifyScoreManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            userId = user.UserId;
            VerifyAndUpdateScore();
        }
        else
        {
            Debug.LogError("User belum login! Pastikan login terlebih dahulu.");
        }
    }

    private async void VerifyAndUpdateScore()
    {
        DocumentReference userRef = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            int currentScore = snapshot.GetValue<int>("score");
            Debug.Log("Skor pengguna saat ini: " + currentScore);
        }
        else
        {
            Debug.LogError("Data pengguna tidak ditemukan di Firestore.");
        }
    }
}
