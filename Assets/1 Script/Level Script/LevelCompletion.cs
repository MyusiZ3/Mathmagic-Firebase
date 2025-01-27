using UnityEngine;

public class LevelCompletion : MonoBehaviour
{
    public void CompleteLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("LEVEL_COMPLETED_" + levelNumber, 1); // Menyimpan status level yang telah selesai
        PlayerPrefs.SetInt("LEVEL", levelNumber + 1); // Menyiapkan level berikutnya
        PlayerPrefs.Save();
    }
}
