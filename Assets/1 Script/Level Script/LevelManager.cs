using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public Button[] listButtonLevel;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;
    private int currentLevel = 1;
    private Dictionary<string, object> levelCompleted = new Dictionary<string, object>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        if (listButtonLevel == null || listButtonLevel.Length == 0)
        {
            listButtonLevel = FindObjectsByType<Button>(FindObjectsSortMode.None); // Menggunakan metode baru
            if (listButtonLevel.Length == 0)
            {
                Debug.LogError("Tombol level tidak ditemukan! Pastikan ada di scene.");
            }
        }
        else
        {
            Debug.Log("Jumlah tombol level: " + listButtonLevel.Length);
        }

        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            userId = user.UserId;
            CheckLevelProgress();
        }
        else
        {
            Debug.LogError("User belum login! Pastikan login terlebih dahulu.");
        }
    }

    private async void CheckLevelProgress()
    {
        if (string.IsNullOrEmpty(userId)) return;

        DocumentReference docRef = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.ContainsField("LEVEL"))
                currentLevel = snapshot.GetValue<int>("LEVEL");
            if (snapshot.ContainsField("LEVEL_COMPLETED"))
                levelCompleted = snapshot.GetValue<Dictionary<string, object>>("LEVEL_COMPLETED");
            
            Debug.Log($"Data dari Firestore: LEVEL={currentLevel}, LEVEL_COMPLETED={levelCompleted.Count}");
            UpdateLevelButtons();
        }
        else
        {
            Debug.LogWarning("Data pengguna tidak ditemukan di Firestore.");
        }
    }

    private void UpdateLevelButtons()
    {
        if (listButtonLevel == null || listButtonLevel.Length == 0)
        {
            Debug.LogError("listButtonLevel tidak diinisialisasi! Pastikan tombol level sudah diassign di Inspector.");
            return;
        }

        for (int i = 0; i < listButtonLevel.Length; i++)
        {
            if (listButtonLevel[i] == null)
            {
                Debug.LogError("Tombol level index " + i + " tidak diassign!");
                continue;
            }

            bool isLevelCompleted = levelCompleted.ContainsKey((i + 1).ToString()) && (bool)levelCompleted[(i + 1).ToString()];
            listButtonLevel[i].interactable = isLevelCompleted || (i + 1) == currentLevel;
        }
    }

    public void SelectLevel(int levelNumber)
    {
        StartCoroutine(LoadLevelWithDelay($"lvl_{levelNumber}"));
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    private System.Collections.IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }

    public async void CompleteLevel(int levelNumber)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Pastikan pengguna telah login.");
            return;
        }

        if (!levelCompleted.ContainsKey(levelNumber.ToString()))
        {
            levelCompleted[levelNumber.ToString()] = true;
        }

        DocumentReference docRef = db.Collection("users").Document(userId);
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "LEVEL_COMPLETED." + levelNumber, true },
            { "LEVEL", levelNumber + 1 }
        };

        await docRef.UpdateAsync(updates);
        
        currentLevel = levelNumber + 1;
        levelCompleted[levelNumber.ToString()] = true;
        UpdateLevelButtons();
    }
}
