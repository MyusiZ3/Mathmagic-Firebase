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
    private HashSet<string> completedBonusLevels = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Salin referensi tombol dari Inspector duplicate ke Instance yang persistent
            if (this.listButtonLevel != null && this.listButtonLevel.Length > 0)
            {
                Instance.listButtonLevel = this.listButtonLevel;
            }
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

        // Filter out any buttons that are actually BonusLevelButtons to avoid indexing and sorting conflicts
        System.Collections.Generic.List<GameObject> filteredButtons = new System.Collections.Generic.List<GameObject>();
        foreach (var btnObj in levelButtons)
        {
            if (btnObj != null && btnObj.GetComponent<BonusLevelButton>() == null)
            {
                filteredButtons.Add(btnObj);
            }
        }

        GameObject[] mainLevelButtons = filteredButtons.ToArray();

        // Urutkan tombol berdasarkan angka dalam namanya agar urutannya benar (Level 1, Level 2, dst)
        System.Array.Sort(mainLevelButtons, (a, b) =>
        {
            int numA = GetLevelNumberFromName(a.name);
            int numB = GetLevelNumberFromName(b.name);
            if (numA != -1 && numB != -1)
            {
                return numA.CompareTo(numB);
            }
            return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
        });

        listButtonLevel = new Button[mainLevelButtons.Length];
        for (int i = 0; i < mainLevelButtons.Length; i++)
        {
            listButtonLevel[i] = mainLevelButtons[i].GetComponent<Button>();
        }

        Debug.Log($"Ditemukan dan diurutkan {listButtonLevel.Length} tombol level utama.");
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

            completedBonusLevels.Clear();
            if (snapshot.ContainsField("BONUS_COMPLETED"))
            {
                var bonusData = snapshot.GetValue<Dictionary<string, object>>("BONUS_COMPLETED");
                if (bonusData != null)
                {
                    foreach (var key in bonusData.Keys)
                    {
                        if (bonusData[key] is bool && (bool)bonusData[key])
                        {
                            completedBonusLevels.Add(key);
                        }
                    }
                }
            }
            
            Debug.Log($"Data dari Firestore: LEVEL={currentLevel}, LEVEL_COMPLETED={levelCompleted.Count}, BONUS_COMPLETED={completedBonusLevels.Count}");
            UpdateLevelButtons();
        }
        else
        {
            Debug.LogWarning("Data pengguna tidak ditemukan di Firestore.");
        }
    }

    private void UpdateLevelButtons()
    {
        // 1. Update tombol level utama bawaan
        if (listButtonLevel != null && listButtonLevel.Length > 0)
        {
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
                bool previousLevelCompleted = levelCompleted.ContainsKey(i.ToString()) && System.Convert.ToBoolean(levelCompleted[i.ToString()]);
                
                // Atau jika level ini di bawah atau sama dengan currentLevel yang aktif
                bool isCurrentLevel = (i + 1) <= currentLevel;

                listButtonLevel[i].interactable = previousLevelCompleted || isCurrentLevel;
                Debug.Log($"[LevelManager] Button {listButtonLevel[i].gameObject.name} (index {i}, Level {i+1}) set to interactable={listButtonLevel[i].interactable} (prevCompleted={previousLevelCompleted}, isCurrent={isCurrentLevel}, currentLevel={currentLevel})");
            }
        }

        // 2. Update tombol level bonus (mencari yang aktif maupun nonaktif)
        BonusLevelButton[] bonusButtons = FindObjectsByType<BonusLevelButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var bonusBtn in bonusButtons)
        {
            if (bonusBtn != null)
            {
                bonusBtn.RefreshState(currentLevel, completedBonusLevels);
            }
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

    public void ResetLocalProgress()
    {
        currentLevel = 1;
        levelCompleted.Clear();
        completedBonusLevels.Clear();
        UpdateLevelButtons();
        Debug.Log("Progress lokal direset: level=1, levelCompleted dikosongkan, completedBonusLevels dikosongkan.");
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

    public async void CompleteBonusLevel(string bonusId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak ditemukan. Pastikan pengguna telah login.");
            return;
        }

        if (completedBonusLevels.Contains(bonusId))
        {
            Debug.Log($"Level bonus {bonusId} sudah selesai sebelumnya secara lokal.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = db.Collection("users").Document(shortId);

        // Menandai level bonus sebagai selesai di Firestore
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"BONUS_COMPLETED.{bonusId}", true }
        };

        await docRef.UpdateAsync(updates);
        Debug.Log($"Level bonus {bonusId} completed di Firestore.");

        // Update lokal dan perbarui tombol UI
        completedBonusLevels.Add(bonusId);
        UpdateLevelButtons();
    }

    public bool IsMainLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;

        int prevLevelIndex = levelNumber - 1;
        bool previousLevelCompleted = levelCompleted.ContainsKey(prevLevelIndex.ToString()) && System.Convert.ToBoolean(levelCompleted[prevLevelIndex.ToString()]);
        bool isCurrentLevel = levelNumber <= currentLevel;

        bool result = previousLevelCompleted || isCurrentLevel;
        Debug.Log($"[LevelManager] IsMainLevelUnlocked({levelNumber}): {result} (prevLevelCompleted={previousLevelCompleted}, isCurrentLevel={isCurrentLevel}, currentLevel={currentLevel})");
        return result;
    }
}

