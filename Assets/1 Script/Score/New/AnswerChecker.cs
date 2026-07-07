using UnityEngine;

public class AnswerChecker : MonoBehaviour
{
    [Header("Question Settings")]
    public bool correctAnswer;

    public void OnAnswerSelected(bool userAnswer)
    {
        if (HealthManager.Instance != null)
        {
            if (HealthManager.Instance.CurrentHealth <= 0) return;

            if (userAnswer != correctAnswer)
            {
                HealthManager.Instance.LoseHealth();

                // Panggil AnswerShuffler di parent jika ada untuk mengacak posisi tombol setelah salah
                AnswerShuffler shuffler = GetComponentInParent<AnswerShuffler>();
                if (shuffler != null)
                {
                    shuffler.Shuffle();
                }
            }
        }
        else
        {
            Debug.LogWarning("HealthManager.Instance tidak ditemukan di scene. Logika HP dilewati.");
        }
    }
}
