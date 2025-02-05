using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class AnswerChecker : MonoBehaviour
{
    [Header("Question Settings")]
    public bool correctAnswer;

    [Header("Scoring")]
    public int pointsPerCorrect = 10;
    public int pointsPerWrong = -5;

    private FirebaseFirestore firestore;
    private string userId;

    private void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("UserId");
    }

    public void OnAnswerSelected(bool userAnswer)
    {
        if (!HealthManager.Instance.HasEnoughHealth())
        {
            Debug.Log("HP habis, tunggu regenerasi nyawa.");
            return;
        }

        if (userAnswer == correctAnswer)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }

    private void HandleCorrectAnswer()
    {
        Debug.Log("Jawaban Benar!");
        ScoreManager.Instance.AddScore(pointsPerCorrect);
        UpdateFirestoreScore(pointsPerCorrect);
    }

    private void HandleWrongAnswer()
    {
        Debug.Log("Jawaban Salah!");
        ScoreManager.Instance.AddScore(pointsPerWrong);
        UpdateFirestoreScore(pointsPerWrong);
        HealthManager.Instance.LoseHealth();
    }

    private void UpdateFirestoreScore(int scoreChange)
    {
        DocumentReference userRef = firestore.Collection("users").Document(userId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int currentScore = task.Result.GetValue<int>("score");
                int newScore = Mathf.Max(0, currentScore + scoreChange);

                userRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "score", newScore }
                });
            }
        });
    }
}