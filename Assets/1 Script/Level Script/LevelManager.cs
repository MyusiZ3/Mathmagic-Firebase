using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections;

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

        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            userId = user.UserId;
            StartCoroutine(WaitAndRefreshLevel());
        }
        else
        {
            Debug.LogError("User belum login! Pastikan login terlebih dahulu.");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private IEnumerator WaitAndRefreshLevel()
    {
        yield return new WaitForSeconds(0.5f);
        CheckLevelProgress();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindLevelButtons();
        if (scene.name == "1Main_Menu" || scene.name == "2Main_Pages" || scene.name == "MainMenu")
        {
            CheckLevelProgress();
        }
        else
        {
            UpdateLevelButtons();
        }
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(userId))
        {
            CheckLevelProgress();
        }
    }

    private void FindLevelButtons()
    {
        GameObject[] levelButtons = GameObject.FindGameObjectsWithTag("LevelButton");

        if (levelButtons.Length == 0)
        {
            // Jika tidak ada tombol dengan tag di scene ini, jangan overwrite listButtonLevel yang mungkin sudah di-assign dari Inspector
            return;
        }

        // Urutkan tombol berdasarkan angka dalam namanya agar urutannya benar (Level 1, Level 2, dst)
        System.Array.Sort(levelButtons, (a, b) =>
        {
            int numA = GetLevelNumberFromName(a.name);
            int numB = GetLevelNumberFromName(b.name);
            if (numA != -1 && numB != -1)
            {
                return numA.CompareTo(numB);
            }
            return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
        });

        listButtonLevel = new Button[levelButtons.Length];
        for (int i = 0; i < levelButtons.Length; i++)
        {
            listButtonLevel[i] = levelButtons[i].GetComponent<Button>();
        }

        Debug.Log($"Ditemukan dan diurutkan {listButtonLevel.Length} tombol level.");
    }

    private int GetLevelNumberFromName(string name)
    {
        string numberString = "";
        foreach (char c in name)
        {
            if (char.IsDigit(c))
            {
                numberString += c;
            }
        }
        if (int.TryParse(numberString, out int levelNum))
        {
            return levelNum;
        }
        return -1;
    }

    private async void CheckLevelProgress()
    {
        if (string.IsNullOrEmpty(userId)) return;

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = db.Collection("users").Document(shortId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.ContainsField("LEVEL"))
                currentLevel = snapshot.GetValue<int>("LEVEL");
            
            if (snapshot.ContainsField("LEVEL_COMPLETED"))
            {
                var data = snapshot.GetValue<Dictionary<string, object>>("LEVEL_COMPLETED");
                levelCompleted = data ?? new Dictionary<string, object>();
            }
            else
            {
                levelCompleted = new Dictionary<string, object>();
            }
            
            Debug.Log($"Data dari Firestore: LEVEL={currentLevel}, LEVEL_COMPLETED={levelCompleted.Count}");
            UpdateLevelButtons();
        }
        else
        {
            Debug.LogWarning("Data pengguna tidak ditemukan di Firestore.");
        }
    }

    // Non Debug
    private void UpdateLevelButtons()
    {
        if (listButtonLevel == null || listButtonLevel.Length == 0)
            return;

        for (int i = 0; i < listButtonLevel.Length; i++)
        {
            if (listButtonLevel[i] == null)
                continue;

            // Level 1 selalu terbuka
            if (i == 0)
            {
                listButtonLevel[i].interactable = true;
                continue;
            }

            // Level (i + 1) terbuka jika level sebelumnya (i) sudah selesai
            bool previousLevelCompleted = levelCompleted.ContainsKey(i.ToString()) && (bool)levelCompleted[i.ToString()];
            
            // Atau jika level ini di bawah atau sama dengan currentLevel yang aktif
            bool isCurrentLevel = (i + 1) <= currentLevel;

            listButtonLevel[i].interactable = previousLevelCompleted || isCurrentLevel;
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

    private IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(levelName);
    }

    // Modifikasi kode CompleteLevel
    public async void CompleteLevel(int levelNumber)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Pastikan pengguna telah login.");
            return;
        }

        // Periksa apakah level sudah selesai sebelumnya secara lokal
        if (levelCompleted.ContainsKey(levelNumber.ToString()) && (bool)levelCompleted[levelNumber.ToString()])
        {
            Debug.Log($"Level {levelNumber} sudah selesai sebelumnya secara lokal, tidak perlu update.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = db.Collection("users").Document(shortId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        bool needFirestoreUpdate = true;

        if (snapshot.Exists)
        {
            var levelCompletedData = snapshot.GetValue<Dictionary<string, object>>("LEVEL_COMPLETED");
            if (levelCompletedData != null && levelCompletedData.ContainsKey(levelNumber.ToString()) && (bool)levelCompletedData[levelNumber.ToString()])
            {
                Debug.Log($"Level {levelNumber} sudah selesai di Firestore. Sinkronisasi data lokal...");
                needFirestoreUpdate = false;
            }
        }

        if (needFirestoreUpdate)
        {
            // Menandai level sebagai selesai di Firestore
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { $"LEVEL_COMPLETED.{levelNumber}", true },
                { "LEVEL", levelNumber + 1 } // Menambah level setelah level selesai
            };

            // Update data di Firestore
            await docRef.UpdateAsync(updates);
            Debug.Log($"Level {levelNumber} completed di Firestore. Next level: {levelNumber + 1}");
        }

        // Update status lokal dan perbarui tombol UI
        currentLevel = Mathf.Max(currentLevel, levelNumber + 1);
        levelCompleted[levelNumber.ToString()] = true;
        UpdateLevelButtons();
    }
}

