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
        }
    }

    private void ShowCorrectOverlay()
    {
        correctOverlay.SetActive(true);
        wrongOverlay.SetActive(false); // Sembunyikan overlay salah
    }

    private void ShowWrongOverlay()
    {
        wrongOverlay.SetActive(true);
        correctOverlay.SetActive(false); // Sembunyikan overlay benar
    }
}
