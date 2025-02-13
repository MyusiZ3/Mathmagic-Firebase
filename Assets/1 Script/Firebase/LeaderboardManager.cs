using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LeaderboardManager : MonoBehaviour
{
    FirebaseFirestore db;

    [Header("UI References")]
    public Transform leaderboardParent; // Parent untuk list leaderboard (rank 4 ke bawah)
    public Transform[] top3Positions;  // Posisi khusus untuk Top 3

    [Header("Prefab Leaderboard")]
    public GameObject top3Prefab;  // Prefab khusus untuk Top 3
    public GameObject listPrefab;  // Prefab untuk peringkat 4 ke bawah

    [Header("Profile Sprites")]
    public Sprite[] profileSprites; // 🔥 HARUS SAMA dengan yang ada di ProfileManager!

    [Header("Max Name Length Settings")]
    public int maxNameLengthTop3 = 12; // Panjang maksimum nama untuk Top 3
    public int maxNameLengthList = 9;  // Panjang maksimum nama untuk daftar biasa

    [Header("Top 3 Background Color Settings")]
    public Color[] top3BackgroundColors;  // Array untuk warna background Top 3

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadLeaderboard();
    }

    public async void LoadLeaderboard()
    {
        if (leaderboardParent == null || listPrefab == null || top3Prefab == null || top3Positions.Length < 3)
        {
            Debug.LogError("❌ Ada referensi UI yang belum di-assign di Inspector!");
            return;
        }

        Debug.Log("📡 Mengambil data leaderboard dari Firestore...");

        Query leaderboardQuery = db.Collection("users").OrderByDescending("score").Limit(10);
        QuerySnapshot snapshot = await leaderboardQuery.GetSnapshotAsync();

        if (snapshot.Count == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada data leaderboard yang ditemukan!");
            return;
        }

        foreach (Transform child in leaderboardParent)
        {
            Destroy(child.gameObject);
        }

        int index = 0;

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            string nama = document.GetValue<string>("name");
            string username = document.GetValue<string>("username");
            int score = document.GetValue<int>("score");
            string profileImageName = document.ContainsField("profileImage") ? document.GetValue<string>("profileImage") : "default";

            Debug.Log($"✅ Data: {nama} (@{username}) - Skor: {score}, Gambar: {profileImageName}");

            Sprite selectedSprite = System.Array.Find(profileSprites, sprite => sprite.name == profileImageName);
            if (selectedSprite == null)
            {
                Debug.LogWarning($"❌ Gambar '{profileImageName}' tidak ditemukan! Menggunakan gambar default.");
                selectedSprite = profileSprites.Length > 0 ? profileSprites[0] : null;
            }

            GameObject newItem;
            int maxNameLength = (index < 3) ? maxNameLengthTop3 : maxNameLengthList;
            Color backgroundColor = (index < 3) ? top3BackgroundColors[index] : Color.white;

            if (index < 3)
            {
                newItem = Instantiate(top3Prefab, top3Positions[index]);
            }
            else
            {
                newItem = Instantiate(listPrefab, leaderboardParent);
            }

            LeaderboardItem itemScript = newItem.GetComponent<LeaderboardItem>();

            if (itemScript != null)
            {
                itemScript.SetData(index + 1, ApplyEllipsis(nama, maxNameLength), username, score, selectedSprite, backgroundColor);
            }
            else
            {
                Debug.LogError("❌ LeaderboardItem script tidak ditemukan di prefab!");
            }

            index++;
        }
    }

    string ApplyEllipsis(string text, int maxLength)
    {
        if (text.Length > maxLength)
        {
            return text.Substring(0, maxLength) + "...";
        }
        return text;
    }
}
