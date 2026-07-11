using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    public Button achievementAButton; // Achievement A
    public Button achievementBButton; // Achievement B
    public Button achievementCButton; // Achievement C
    public Button achievementDButton; // Achievement D

    public int scoreToUnlockA = 30;
    public int scoreToUnlockB = 80;
    public int scoreToUnlockC = 150;
    public int scoreToUnlockD = 200;

    void OnEnable()
    {
        if (RemoteSettingsManager.Instance != null)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded += OnRemoteSettingsLoaded;
        }
    }

    void OnDisable()
    {
        if (RemoteSettingsManager.HasInstance)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded -= OnRemoteSettingsLoaded;
        }
    }

    void Start()
    {
        ApplyRemoteSettings();
        UpdateAchievements();
    }

    private void OnRemoteSettingsLoaded()
    {
        ApplyRemoteSettings();
        UpdateAchievements();
    }

    private void ApplyRemoteSettings()
    {
        if (RemoteSettingsManager.Instance != null && RemoteSettingsManager.Instance.IsLoaded)
        {
            scoreToUnlockA = RemoteSettingsManager.Instance.scoreToUnlockA;
            scoreToUnlockB = RemoteSettingsManager.Instance.scoreToUnlockB;
            scoreToUnlockC = RemoteSettingsManager.Instance.scoreToUnlockC;
            scoreToUnlockD = RemoteSettingsManager.Instance.scoreToUnlockD;
            Debug.Log($"[AchievementManager] Applied remote settings thresholds: A={scoreToUnlockA}, B={scoreToUnlockB}, C={scoreToUnlockC}, D={scoreToUnlockD}");
        }
    }

    // Fungsi untuk mengecek apakah achievements harus terbuka
    public void UpdateAchievements()
    {
        int currentScore = ScoreManager.Instance.GetCurrentScore();

        // Cek apakah skor cukup untuk membuka achievement A
        if (currentScore >= scoreToUnlockA)
        {
            achievementAButton.interactable = true;
        }
        else
        {
            achievementAButton.interactable = false;
        }

        // Cek apakah skor cukup untuk membuka achievement B
        if (currentScore >= scoreToUnlockB)
        {
            achievementBButton.interactable = true;
        }
        else
        {
            achievementBButton.interactable = false;
        }

        // Cek apakah skor cukup untuk membuka achievement C
        if (currentScore >= scoreToUnlockC)
        {
            achievementCButton.interactable = true;
        }
        else
        {
            achievementCButton.interactable = false;
        }

        // Cek apakah skor cukup untuk membuka achievement D
        if (currentScore >= scoreToUnlockD)
        {
            achievementDButton.interactable = true;
        }
        else
        {
            achievementDButton.interactable = false;
        }
    }

    // Fungsi ini bisa dipanggil setiap kali skor berubah
    public void OnScoreUpdated()
    {
        UpdateAchievements();
    }
}
