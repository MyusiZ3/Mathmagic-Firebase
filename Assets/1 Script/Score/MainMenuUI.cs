using UnityEngine;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        UpdateScoreDisplay();
    }

    void OnEnable()
    {
        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance != null)
        {
            // Menunggu skor diupdate terlebih dahulu sebelum menampilkan
            int score = ScoreManager.Instance.GetCurrentScore();
            scoreText.text = score.ToString();
            Debug.Log("Skor ditampilkan di Main Menu: " + score);
        }
        else
        {
            scoreText.text = "0";
            Debug.LogWarning("ScoreManager.Instance is null. Skor ditampilkan: 0");
        }
    }
}
