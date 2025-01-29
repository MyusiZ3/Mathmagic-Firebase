using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;

public class AnswerButton : MonoBehaviour
{
    public int scoreValue = 10; // Nilai skor yang ditambahkan saat tombol diklik
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;
    private Button button;

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

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError("Komponen Button tidak ditemukan.");
        }
    }

    void OnButtonClicked()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
            Debug.Log($"Skor ditambahkan: {scoreValue}");
        }
        else
        {
            Debug.LogError("ScoreManager instance tidak ditemukan.");
        }

        UpdateScoreInFirestore(scoreValue);

        // Panggil CompleteLevel dari LevelManager jika level selesai
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            // Misalnya, level selesai jika skor mencapai nilai tertentu
            if (ScoreManager.Instance.GetCurrentScore() >= 100) // Ganti dengan logika level selesai
            {
                int currentLevel = levelManager.GetCurrentLevel(); // Anda perlu menambahkan metode GetCurrentLevel di LevelManager
                levelManager.CompleteLevel(currentLevel);
            }
        }
        else
        {
            Debug.LogError("LevelManager tidak ditemukan di scene.");
        }
    }

    public async void UpdateScoreInFirestore(int scoreToAdd)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat memperbarui skor.");
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(userId);

        // Update skor di Firestore
        await userRef.UpdateAsync("score", FieldValue.Increment(scoreToAdd));  // Mengganti SCORE menjadi score
        Debug.Log($"Skor berhasil ditambahkan: {scoreToAdd}");
    }
}