using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PageManager : MonoBehaviour
{
    [Header("Question Pages")]
    public GameObject[] questionPages;
    [Tooltip("Jika true, urutan soal akan diacak otomatis saat level dimulai.")]
    public bool randomizeQuestions = false;
    private int currentPageIndex = 0;

    [Header("Level Completion Settings")]
    public UnityEvent OnLevelComplete; // Event jika ingin menampilkan UI Level Selesai (misal pop-up menang)
    public bool autoLoadNextLevel = false; // Jika true, otomatis pindah ke scene lvl_ berikutnya
    [Tooltip("Kosongkan jika ingin deteksi otomatis lvl_1 -> lvl_2. Isi jika ingin load scene spesifik.")]
    public string nextSceneOverride = ""; // Override nama scene berikutnya

    // Flag untuk menandai apakah level sudah selesai
    public bool IsLevelCompleted { get; private set; } = false;

    private void Start()
    {
        if (randomizeQuestions)
        {
            ShuffleQuestions();
        }
        ShowCurrentPage();
    }

    private void ShuffleQuestions()
    {
        if (questionPages == null || questionPages.Length <= 1) return;

        // Fisher-Yates shuffle algorithm
        for (int i = 0; i < questionPages.Length; i++)
        {
            GameObject temp = questionPages[i];
            int randomIndex = Random.Range(i, questionPages.Length);
            questionPages[i] = questionPages[randomIndex];
            questionPages[randomIndex] = temp;
        }
    }

    public bool IsLastQuestion()
    {
        if (questionPages == null || questionPages.Length == 0) return true;
        return currentPageIndex >= questionPages.Length - 1;
    }

    public void NextQuestion()
    {
        if (currentPageIndex < questionPages.Length - 1)
        {
            currentPageIndex++;
            ShowCurrentPage();

            // Sembunyikan semua overlay jawaban saat pindah ke pertanyaan berikutnya
            OverlayAnswer overlayAnswer = FindFirstObjectByType<OverlayAnswer>();
            if (overlayAnswer != null)
            {
                overlayAnswer.HideAllOverlays();
            }
        }
        else
        {
            Debug.Log("Semua soal selesai! Menyelesaikan level...");
            CompleteActiveLevel();
        }
    }

    private void ShowCurrentPage()
    {
        for (int i = 0; i < questionPages.Length; i++)
        {
            if (questionPages[i] != null)
            {
                questionPages[i].SetActive(i == currentPageIndex);
            }
        }
    }

    private void CompleteActiveLevel()
    {
        IsLevelCompleted = true; // Set status level selesai

        // Hentikan timer level saat level selesai (menang)
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.StopTimer();
        }

        string sceneName = SceneManager.GetActiveScene().name;
        bool isBonusLevel = sceneName.ToLower().Contains("bonus");

        // Tambahkan skor reward level (utama/bonus) dari Remote Settings
        if (ScoreManager.Instance != null)
        {
            int reward = 100;
            if (RemoteSettingsManager.Instance != null)
            {
                reward = isBonusLevel ? RemoteSettingsManager.Instance.bonusLevelReward : RemoteSettingsManager.Instance.mainLevelReward;
            }
            else
            {
                reward = isBonusLevel ? 250 : 100;
            }
            ScoreManager.Instance.AddScore(reward);
            Debug.Log($"[PageManager] Menambahkan skor reward level {(isBonusLevel ? "Bonus" : "Utama")} sebesar +{reward}");
        }

        if (isBonusLevel)
        {
            Debug.Log($"Menyelesaikan Level Bonus: {sceneName}...");
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteBonusLevel(sceneName);
            }
            else
            {
                Debug.LogWarning("LevelManager tidak ditemukan, tidak bisa menyelesaikan level bonus di Firestore.");
            }
        }
        else
        {
            int currentLevel = GetLevelNumberFromScene();
            Debug.Log($"Menyelesaikan Level {currentLevel}...");

            // Panggil sistem level completion untuk simpan ke Firestore & update lokal
            LevelCompletion levelCompletion = FindFirstObjectByType<LevelCompletion>();
            if (levelCompletion != null)
            {
                levelCompletion.CompleteLevel(currentLevel);
            }
            else if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteLevel(currentLevel);
            }
            else
            {
                Debug.LogWarning("Tidak menemukan LevelCompletion maupun LevelManager.");
            }
        }

        // Picu event UI (jika ada panel sukses/selesai terdaftar di Inspector)
        OnLevelComplete?.Invoke();

        if (autoLoadNextLevel)
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        string nextSceneName = "";
        string sceneName = SceneManager.GetActiveScene().name;
        bool isBonusLevel = sceneName.ToLower().Contains("bonus");

        if (!string.IsNullOrEmpty(nextSceneOverride))
        {
            nextSceneName = nextSceneOverride;
        }
        else if (isBonusLevel)
        {
            // Kembali ke Level Map (2Main_Pages atau MainMenu) setelah level bonus
            nextSceneName = "2Main_Pages";
        }
        else
        {
            int currentLevel = GetLevelNumberFromScene();
            int nextLevel = currentLevel + 1;
            nextSceneName = $"lvl_{nextLevel}";
        }

        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Jika target map '2Main_Pages' tidak terload langsung, coba muat MainMenu atau log warning
            if (isBonusLevel && nextSceneName == "2Main_Pages")
            {
                if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                {
                    SceneManager.LoadScene("MainMenu");
                    return;
                }
            }
            Debug.LogWarning($"Scene {nextSceneName} tidak terdaftar di Build Settings. Kembali ke MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
    }

    private int GetLevelNumberFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string numberString = "";
        foreach (char c in sceneName)
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
        return 1;
    }
}