using UnityEngine;

public class AnswerChecker : MonoBehaviour
{
    [Header("Question Settings")]
    public bool correctAnswer;

    public void OnAnswerSelected(bool userAnswer)
    {
        if (HealthManager.Instance.CurrentHealth <= 0) return; // Ubah akses

        if (userAnswer != correctAnswer)
        {
            HealthManager.Instance.LoseHealth();
        }
    }
}
