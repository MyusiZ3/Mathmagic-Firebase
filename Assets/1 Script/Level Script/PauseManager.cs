using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseOverlay; // Drag & drop overlay di sini di Unity Editor

    private bool isPaused = false;

    void Start()
    {
        // Hide pause overlay saat game mulai
        pauseOverlay.SetActive(false);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pause game, menghentikan waktu
            Time.timeScale = 0f;
            pauseOverlay.SetActive(true); // Menampilkan overlay
        }
        else
        {
            // Resume game, melanjutkan waktu
            Time.timeScale = 1f;
            pauseOverlay.SetActive(false); // Menyembunyikan overlay
        }
    }
}
