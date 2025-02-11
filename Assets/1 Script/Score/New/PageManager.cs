using UnityEngine;

public class PageManager : MonoBehaviour
{
    public GameObject[] questionPages;
    private int currentPageIndex = 0;

    private void Start()
    {
        ShowCurrentPage();
    }

    public void NextQuestion()
    {
        if (currentPageIndex < questionPages.Length - 1)
        {
            currentPageIndex++;
            ShowCurrentPage();
        }
        else
        {
            Debug.Log("Semua soal selesai!");
        }
    }

    private void ShowCurrentPage()
    {
        for (int i = 0; i < questionPages.Length; i++)
        {
            questionPages[i].SetActive(i == currentPageIndex);
        }
    }
}