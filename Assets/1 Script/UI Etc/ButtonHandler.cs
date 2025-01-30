using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public SimpleAnswerManager answerManager; // Referensi ke SimpleAnswerManager
    public bool isCorrect; // Apakah jawaban ini benar

    public void OnButtonClicked()
    {
        Debug.Log("Button clicked. Is Correct: " + isCorrect);
        answerManager.CheckAnswer(isCorrect);
        
        // Mengatur status game sesuai dengan jawaban
        if (isCorrect)
        {
            OverlayManager.Instance.SetGameState(GameState.AnswerCorrect);
        }
        else
        {
            OverlayManager.Instance.SetGameState(GameState.AnswerWrong);
        }
    }
}
