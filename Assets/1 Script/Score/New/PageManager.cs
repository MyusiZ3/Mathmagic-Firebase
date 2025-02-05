using UnityEngine;

public class PageManager : MonoBehaviour
{
    public GameObject[] questions; // Array berisi semua soal dalam level
    private int currentQuestionIndex = 0;

    private void Start()
    {
        ShowQuestion(0);
    }

    public void NextQuestion()
    {
        if (currentQuestionIndex < questions.Length - 1)
        {
            questions[currentQuestionIndex].SetActive(false);
            currentQuestionIndex++;
            questions[currentQuestionIndex].SetActive(true);
        }
        else
        {
            Debug.Log("Semua soal dalam level ini telah selesai!");
        }
    }

    private void ShowQuestion(int index)
    {
        for (int i = 0; i < questions.Length; i++)
        {
            questions[i].SetActive(i == index);
        }
    }
}
