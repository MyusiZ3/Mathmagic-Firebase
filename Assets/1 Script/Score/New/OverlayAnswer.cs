using UnityEngine;

public class OverlayAnswer : MonoBehaviour
{
    [Header("Overlay Panels")]
    public GameObject correctOverlay;  // Panel untuk jawaban benar
    public GameObject wrongOverlay;    // Panel untuk jawaban salah

    [Header("Audio Clips")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public void ShowCorrectOverlay()
    {
        HideAllOverlays(); // Pastikan overlay lain mati sebelum menampilkan yang benar
        correctOverlay.SetActive(true);
        PlaySound(correctSound);

        // Tambah skor otomatis ketika jawaban benar
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10);
            Debug.Log("Skor +10 ditambahkan secara otomatis lewat OverlayAnswer.");
        }
    }

    public void ShowWrongOverlay()
    {
        HideAllOverlays(); // Pastikan overlay lain mati sebelum menampilkan yang salah
        wrongOverlay.SetActive(true);
        PlaySound(wrongSound);
    }

    public void HideAllOverlays()
    {
        correctOverlay?.SetActive(false);
        wrongOverlay?.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
