using UnityEngine;
using UnityEngine.UI;

public class OverlayNavigator : MonoBehaviour
{
    public GameObject[] overlays;  // Array untuk menyimpan overlay
    public Button nextButton;      // Tombol Next
    public Button previousButton;  // Tombol Previous

    private int currentIndex = 0;  // Menyimpan indeks overlay yang aktif saat ini

    void Start()
    {
        // Tampilkan overlay pertama, sembunyikan lainnya
        UpdateOverlay();

        // Tambahkan listener untuk tombol
        nextButton.onClick.AddListener(ShowNextOverlay);
        previousButton.onClick.AddListener(ShowPreviousOverlay);
    }

    // Fungsi untuk menampilkan overlay selanjutnya
    public void ShowNextOverlay()
    {
        if (currentIndex < overlays.Length - 1)
        {
            currentIndex++;  // Pindah ke overlay berikutnya
            UpdateOverlay();
        }
    }

    // Fungsi untuk menampilkan overlay sebelumnya
    public void ShowPreviousOverlay()
    {
        if (currentIndex > 0)
        {
            currentIndex--;  // Pindah ke overlay sebelumnya
            UpdateOverlay();
        }
    }

    // Fungsi untuk mengupdate tampilan overlay
    void UpdateOverlay()
    {
        // Loop melalui semua overlay
        for (int i = 0; i < overlays.Length; i++)
        {
            if (i == currentIndex)
            {
                overlays[i].SetActive(true);  // Aktifkan overlay saat ini
            }
            else
            {
                overlays[i].SetActive(false);  // Sembunyikan overlay lain
            }
        }

        // Nonaktifkan tombol jika di batas (pertama atau terakhir)
        previousButton.interactable = currentIndex > 0;
        nextButton.interactable = currentIndex < overlays.Length - 1;
    }
}
