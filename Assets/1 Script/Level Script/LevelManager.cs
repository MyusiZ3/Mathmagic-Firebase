using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public Button[] listButtonLevel; // Daftar tombol level
    private string welcomeKey = "HasSeenWelcome"; // Kunci untuk menyimpan status welcome message

    void Start()
    {
        // Cek apakah welcome message sudah pernah dilihat
        if (PlayerPrefs.HasKey(welcomeKey) && PlayerPrefs.GetInt(welcomeKey) == 1)
        {
            // Welcome message sudah pernah dilihat, langsung cek level
            CekLevel();
        }
        else
        {
            // Jika belum pernah dilihat, arahkan ke scene welcome message
            SceneManager.LoadScene("WelcomeScene");
        }
    }

    public void CekLevel()
    {
        int levelTerakhirMain;

        // Cek level terakhir yang dimainkan
        if (!PlayerPrefs.HasKey("LEVEL"))
        {
            levelTerakhirMain = 1; // Level default jika belum ada
        }
        else
        {
            levelTerakhirMain = PlayerPrefs.GetInt("LEVEL");
        }

        // Looping sesuai dengan jumlah tombol
        for (int i = 0; i < listButtonLevel.Length; i++)
        {
            // Aktifkan tombol jika level sebelumnya sudah selesai atau level itu sendiri sudah selesai
            if (i < levelTerakhirMain || PlayerPrefs.HasKey("LEVEL_COMPLETED_" + (i + 1)))
            {
                listButtonLevel[i].interactable = true; // Aktifkan tombol
            }
            else
            {
                listButtonLevel[i].interactable = false; // Nonaktifkan tombol
            }

            // Debugging log untuk status level
            Debug.Log("Button Level " + (i + 1) + ": " + (listButtonLevel[i].interactable ? "Aktif" : "Tidak Aktif"));
        }
        
        Debug.Log("Level terakhir yang dimainkan: " + levelTerakhirMain);
    }

    public void PilihLevel(string levelBerapa)
    {
        StartCoroutine(LoadLevelWithDelay("lvl_" + levelBerapa));
    }

    private IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(1f); // Atur delay sesuai kebutuhan
        SceneManager.LoadScene(levelName);
    }

    // Fungsi untuk menyelesaikan level
    public void CompleteLevel(int currentLevel)
    {
        // Menyimpan bahwa level saat ini sudah selesai
        PlayerPrefs.SetInt("LEVEL_COMPLETED_" + currentLevel, 1);

        // Mengupdate level terakhir yang dimainkan
        int nextLevel = currentLevel + 1;
        PlayerPrefs.SetInt("LEVEL", nextLevel);

        // Simpan perubahan di PlayerPrefs
        PlayerPrefs.Save();

        Debug.Log("Level " + currentLevel + " diselesaikan. Level berikutnya: " + nextLevel);
        CekLevel(); // Memperbarui status level
    }

    // Fungsi untuk mereset semua level dan skor
    public void ResetLevels()
    {
        StartCoroutine(ResetLevelsWithDelay());
    }

    private IEnumerator ResetLevelsWithDelay()
    {
        // Menghapus semua PlayerPrefs dan mengatur ulang level
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.SetInt("LEVEL", 1); // Mengatur ulang level ke level 1

        // Reset skor
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore(); // Memanggil fungsi reset skor
        }

        // Tambahkan delay sebelum mengupdate tampilan level
        yield return new WaitForSeconds(1f); // Atur delay sesuai kebutuhan
        CekLevel(); // Perbarui tampilan tombol level
    }
}
