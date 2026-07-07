using UnityEngine;

public class SimpleAnswerManager : MonoBehaviour
{
    public GameObject correctOverlay; // Overlay untuk jawaban benar
    public GameObject wrongOverlay; // Overlay untuk jawaban salah

    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            ShowCorrectOverlay();
        }
        else
        {
            ShowWrongOverlay();

            // Cari AnswerShuffler untuk mengacak posisi tombol setelah salah
            AnswerShuffler shuffler = GetComponentInChildren<AnswerShuffler>();
            if (shuffler == null) shuffler = GetComponentInParent<AnswerShuffler>();
            if (shuffler != null)
            {
                shuffler.Shuffle();
            }
        }
    }

    private void ShowCorrectOverlay()
    {
        correctOverlay.SetActive(true);
        wrongOverlay.SetActive(false); // Sembunyikan overlay salah

        // Tambah skor otomatis ketika jawaban benar
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10);
            Debug.Log("Skor +10 ditambahkan secara otomatis lewat SimpleAnswerManager.");
        }
    }

    private void ShowWrongOverlay()
    {
        wrongOverlay.SetActive(true);
        correctOverlay.SetActive(false); // Sembunyikan overlay benar
    }
}
