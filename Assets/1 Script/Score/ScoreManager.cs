using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public event System.Action OnScoreChanged;
    private int currentScore = 0;
    private FirebaseFirestore firestore;
    private string userId;
    private bool isUpdatingScore = false;

    private const string SCORE_PREF_KEY = "local_score"; // Key untuk menyimpan skor lokal
    private const string PENDING_UPDATE_KEY = "pending_score"; // Key untuk menyimpan update tertunda

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            firestore = FirebaseFirestore.DefaultInstance;

            userId = PlayerPrefs.GetString("UserId", string.Empty);
            if (string.IsNullOrEmpty(userId) && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                PlayerPrefs.SetString("UserId", userId);
                PlayerPrefs.Save();
            }

            LoadScore();

            if (!string.IsNullOrEmpty(userId))
            {
                InitializeUserScore(userId);
            }
            else
            {
                Debug.LogWarning("User ID tidak ditemukan saat Awake. Menunggu login.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private ListenerRegistration userScoreListener;

    public void InitializeUserScore(string newUserId)
    {
        userId = newUserId;
        int pendingScore = PlayerPrefs.GetInt(PENDING_UPDATE_KEY, 0);
        if (pendingScore > 0)
        {
            currentScore = pendingScore;
            PlayerPrefs.SetInt(SCORE_PREF_KEY, currentScore);
            PlayerPrefs.Save();
            TryUpdateFirestore();
            OnScoreChanged?.Invoke();
        }
        
        LoadScoreFromFirestore();
    }

    private void LoadScoreFromFirestore()
    {
        if (string.IsNullOrEmpty(userId)) return;

        if (userScoreListener != null)
        {
            userScoreListener.Stop();
            userScoreListener = null;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = firestore.Collection("users").Document(shortId);

        userScoreListener = docRef.Listen(snapshot =>
        {
            if (snapshot.Exists && snapshot.ContainsField("score"))
            {
                int remoteScore = System.Convert.ToInt32(snapshot.GetValue<object>("score"));
                if (currentScore != remoteScore)
                {
                    currentScore = remoteScore;
                    Debug.Log($"[ScoreManager] Skor real-time dari Firestore: {currentScore}");
                    UpdateLocalScore();
                }
            }
        });
    }

    public void AddScore(int amount)
    {
        int oldScore = currentScore;
        currentScore = Mathf.Max(0, currentScore + amount);
        
        if (amount >= 0)
        {
            Debug.Log($"<color=green>[ScoreManager] Skor BERTAMBAH +{amount}. (Sebelumnya: {oldScore} -> Sekarang: {currentScore})</color>");
        }
        else
        {
            Debug.Log($"<color=red>[ScoreManager] Skor BERKURANG {amount}. (Sebelumnya: {oldScore} -> Sekarang: {currentScore})</color>");
        }

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
        ResetLevelInFirestore(); // Tambahkan fungsi ini untuk mereset level ke 1
    }

    // Reset level ke 1 di Firestore dan Reset Skor ke 0
    private void ResetLevelInFirestore()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat mereset level.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = firestore.Collection("users").Document(shortId);

        // Reset LEVEL, LEVEL_COMPLETED, dan BONUS_COMPLETED untuk level pertama yang true
        Dictionary<string, object> resetData = new Dictionary<string, object>
        {
            { "LEVEL", 1 },
            { "score", 0 },
            { "LEVEL_COMPLETED", new Dictionary<string, object> { { "1", true } } }, // Reset LEVEL_COMPLETED dengan level 1 selesai
            { "BONUS_COMPLETED", new Dictionary<string, object>() } // Reset level bonus
        };

        // Set ulang data di Firestore
        docRef.SetAsync(resetData, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("LEVEL berhasil direset ke 1, score direset ke 0, LEVEL_COMPLETED direset, dan BONUS_COMPLETED direset di Firestore.");
                LoadLevelData(); // Memastikan data Firestore di-load ulang
                UpdateLocalScore(); // Update skor di aplikasi
                LevelManager.Instance?.ResetLocalProgress(); // Reset progress lokal LevelManager
            }
            else
            {
                Debug.LogError("Gagal mereset LEVEL, score, LEVEL_COMPLETED, dan BONUS_COMPLETED di Firestore.");
            }
        });
    }

    // Fungsi untuk membaca ulang data Firestore setelah reset
    private void LoadLevelData()
    {
        if (string.IsNullOrEmpty(userId)) return;

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = firestore.Collection("users").Document(shortId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Dictionary<string, object> data = task.Result.ToDictionary();
                if (data.ContainsKey("LEVEL"))
                {
                    int newLevel = System.Convert.ToInt32(data["LEVEL"]);
                    Debug.Log("LEVEL diperbarui: " + newLevel);
                }

                if (data.ContainsKey("LEVEL_COMPLETED"))
                {
                    Dictionary<string, object> levelCompleted = (Dictionary<string, object>)data["LEVEL_COMPLETED"];
                    Debug.Log("LEVEL_COMPLETED diperbarui: " + levelCompleted.Count + " level.");
                }

                if (data.ContainsKey("score"))
                {
                    currentScore = System.Convert.ToInt32(data["score"]);
                    Debug.Log("Skor diperbarui dari Firestore: " + currentScore);
                    OnScoreChanged?.Invoke();
                }
            }
        });
    }

    // Update skor lokal setelah pembaruan di Firestore
    private void UpdateLocalScore()
    {
        PlayerPrefs.SetInt(SCORE_PREF_KEY, currentScore);
        PlayerPrefs.Save();
        Debug.Log("Skor lokal diperbarui: " + currentScore);
        OnScoreChanged?.Invoke();
    }

    // Save Skor
    private void SaveScore()
    {
        PlayerPrefs.SetInt(SCORE_PREF_KEY, currentScore);
        PlayerPrefs.Save();
        TryUpdateFirestore();
        OnScoreChanged?.Invoke();
    }

    private void LoadScore()
    {
        currentScore = PlayerPrefs.GetInt(SCORE_PREF_KEY, 0);
        Debug.Log("Score loaded: " + currentScore);
    }

    private void TryUpdateFirestore()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Tidak dapat menyimpan skor ke Firestore.");
            return;
        }

        if (isUpdatingScore) return; // Cegah update bertumpuk
        isUpdatingScore = true;

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = firestore.Collection("users").Document(shortId);
        docRef.UpdateAsync(new Dictionary<string, object> { { "score", currentScore } }).ContinueWithOnMainThread(task =>
        {
            isUpdatingScore = false;

            if (task.IsCompleted)
            {
                Debug.Log("Skor berhasil diperbarui di Firestore.");
                PlayerPrefs.SetInt(PENDING_UPDATE_KEY, 0); // Reset pending update
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("Gagal memperbarui skor di Firestore, menyimpan secara lokal.");
                PlayerPrefs.SetInt(PENDING_UPDATE_KEY, currentScore);
                PlayerPrefs.Save();
            }
        });
    }

    private async void SyncPendingScore()
    {
        while (true)
        {
            await Task.Delay(5000); // Cek setiap 5 detik

            int pendingScore = PlayerPrefs.GetInt(PENDING_UPDATE_KEY, 0);
            if (pendingScore > 0)
            {
                Debug.Log("Mencoba mengirim skor yang tertunda: " + pendingScore);
                TryUpdateFirestore();
            }
        }
    }

    public static void ClearLocalUserData()
    {
        // 1. Hapus semua key cache lokal yang berkaitan dengan data pengguna
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("local_score");
        PlayerPrefs.DeleteKey("pending_score");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("HasSeenWelcome");
        PlayerPrefs.DeleteKey("profileImageName");
        PlayerPrefs.Save();

        Debug.Log("[ScoreManager] Seluruh cache data pengguna lokal berhasil dibersihkan.");

        // 2. Hancurkan instance persistent manager agar di-instansiasi ulang dengan data baru
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        if (LevelManager.Instance != null)
        {
            Destroy(LevelManager.Instance.gameObject);
        }

        if (HealthManager.Instance != null)
        {
            Destroy(HealthManager.Instance.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (userScoreListener != null)
        {
            userScoreListener.Stop();
            userScoreListener = null;
        }
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
