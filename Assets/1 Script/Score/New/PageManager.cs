using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PageManager : MonoBehaviour
{
    [Header("Question Pages")]
    public GameObject[] questionPages;
    private int currentPageIndex = 0;

    [Header("Level Completion Settings")]
    public UnityEvent OnLevelComplete; // Event jika ingin menampilkan UI Level Selesai (misal pop-up menang)
    public bool autoLoadNextLevel = false; // Jika true, otomatis pindah ke scene lvl_ berikutnya
    [Tooltip("Kosongkan jika ingin deteksi otomatis lvl_1 -> lvl_2. Isi jika ingin load scene spesifik.")]
    public string nextSceneOverride = ""; // Override nama scene berikutnya

    private void Start()
    {
        ShowCurrentPage();
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
        if (!string.IsNullOrEmpty(nextSceneOverride))
        {
            nextSceneName = nextSceneOverride;
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