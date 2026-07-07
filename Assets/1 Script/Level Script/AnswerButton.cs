using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    public int scoreValue = 10; // Nilai skor yang ditambahkan saat tombol diklik
    private Button button;

    void Start()
    {
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
    }
}
