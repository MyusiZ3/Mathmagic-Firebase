// Buat ambil data
using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    FirebaseFirestore db;
    
    [Header("Leaderboard Settings")]
    public int maxEntries = 10; // Jumlah yang ditampilkan (bisa diubah dari Inspector)
    
    [Header("UI References")]
    public Transform leaderboardParent; // Parent tempat menampilkan leaderboard
    public GameObject leaderboardItemPrefab; // Prefab item leaderboard

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadLeaderboard();
    }

    public async void LoadLeaderboard()
    {
        Query leaderboardQuery = db.Collection("users").OrderByDescending("score").Limit(maxEntries);
        QuerySnapshot snapshot = await leaderboardQuery.GetSnapshotAsync();

        // Hapus data lama sebelum menampilkan yang baru
        foreach (Transform child in leaderboardParent)
        {
            Destroy(child.gameObject);
        }

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            string nama = document.ContainsField("name") ? document.GetValue<string>("name") : "No Name";
            string username = document.ContainsField("username") ? document.GetValue<string>("username") : "No Username";
            int score = document.ContainsField("score") ? document.GetValue<int>("score") : 0;
            string profileImage = document.ContainsField("profileImage") ? document.GetValue<string>("profileImage") : "default";

            GameObject newItem = Instantiate(leaderboardItemPrefab, leaderboardParent);
            newItem.GetComponent<LeaderboardItem>().SetData(nama, username, score, profileImage);
        }

    }
}
