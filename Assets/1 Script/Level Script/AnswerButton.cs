using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    public int scoreValue = 10; // Nilai skor yang ditambahkan ketika tombol diklik

    private Button button;

    void Start()
    {
        // Mencari komponen Button pada GameObject ini
        button = GetComponent<Button>();

        // Cek apakah button null untuk menghindari error
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError("Button component not found on this GameObject. Please attach the Button component.");
        }
    }

    // Fungsi ini dipanggil saat tombol diklik
    void OnButtonClicked()
    {
        // Periksa apakah ScoreManager tersedia
        if (ScoreManager.Instance != null)
        {
            // Tambahkan skor setiap kali tombol diklik
            ScoreManager.Instance.AddScore(scoreValue);
            Debug.Log("Skor ditambahkan: " + scoreValue);
        }
        else
        {
            Debug.LogError("ScoreManager instance is not found.");
        }
    }
}
