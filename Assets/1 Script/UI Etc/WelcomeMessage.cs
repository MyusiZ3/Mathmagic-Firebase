using UnityEngine;
using UnityEngine.UI;

public class WelcomeMessage : MonoBehaviour
{
    public GameObject welcomeMessageOverlay;  // Overlay untuk welcome message
    public GameObject nextOverlay;  // Overlay berikutnya yang akan dibuka setelah welcome message
    public Button continueButton;

    private string welcomeKey = "HasSeenWelcome";  // Kunci untuk menyimpan status welcome message

    void Start()
    {
        // Cek apakah welcome message sudah pernah dilihat
        if (!PlayerPrefs.HasKey(welcomeKey) || PlayerPrefs.GetInt(welcomeKey) == 0)
        {
            // Tampilkan welcome message jika belum pernah dilihat
            welcomeMessageOverlay.SetActive(true);
            nextOverlay.SetActive(false);  // Sembunyikan overlay berikutnya sementara
            continueButton.onClick.AddListener(HideWelcomeMessage);
        }
        else
        {
            // Jika sudah dilihat, langsung sembunyikan welcome message dan tampilkan overlay berikutnya
            welcomeMessageOverlay.SetActive(false);
            nextOverlay.SetActive(true);
        }
    }

    public void HideWelcomeMessage()
    {
        welcomeMessageOverlay.SetActive(false);  // Sembunyikan welcome message overlay

        // Simpan status bahwa welcome message sudah dilihat
        PlayerPrefs.SetInt(welcomeKey, 1);
        PlayerPrefs.Save();

        // Tampilkan overlay berikutnya
        nextOverlay.SetActive(true);
    }
}
