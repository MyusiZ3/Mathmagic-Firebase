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
        if (scene.name == "MainMenu")
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
        // Misalnya, tag-nya adalah "LevelButton"
        listButtonLevel = new Button[5]; // Sesuaikan dengan jumlah tombol yang ada
        GameObject[] levelButtons = GameObject.FindGameObjectsWithTag("LevelButton");

        if (levelButtons.Length == 0)
        {
            // Debug.LogError("Tombol level tidak ditemukan! Pastikan ada di scene dan tag-nya sesuai.");
            return;
        }

        // Konversi GameObject ke Button dan simpan ke listButtonLevel
        for (int i = 0; i < levelButtons.Length; i++)
        {
            listButtonLevel[i] = levelButtons[i].GetComponent<Button>();
        }

        if (listButtonLevel.Length == 0)
        {
            Debug.LogError("Tidak ada tombol dengan tag yang sesuai.");
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
// debug version
    // private void UpdateLevelButtons()
    // {
    //     if (listButtonLevel == null || listButtonLevel.Length == 0)
    //     {
    //         Debug.LogError("listButtonLevel tidak diinisialisasi! Pastikan tombol level sudah diassign di Inspector.");
    //         return;
    //     }

    //     for (int i = 0; i < listButtonLevel.Length; i++)
    //     {
    //         if (listButtonLevel[i] == null)
    //         {
    //             Debug.LogError("Tombol level index " + i + " tidak diassign!");
    //             continue;
    //         }

    //         bool isLevelCompleted = levelCompleted.ContainsKey((i + 1).ToString()) && (bool)levelCompleted[(i + 1).ToString()];
    //         listButtonLevel[i].interactable = isLevelCompleted || (i + 1) == currentLevel;
    //     }
    // }

    // Non Debug
    private void UpdateLevelButtons()
    {
        if (listButtonLevel == null || listButtonLevel.Length == 0)
            return;

        for (int i = 0; i < listButtonLevel.Length; i++)
        {
            if (listButtonLevel[i] == null)
                continue;

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

    private IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(levelName);
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

