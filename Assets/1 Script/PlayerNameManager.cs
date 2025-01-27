using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerNameManager : MonoBehaviour
{
    // Referensi untuk TMP_InputField, tempat pengguna mengetikkan nama
    public TMP_InputField NamaInput;

    // Referensi untuk teks yang menampilkan nama pemain
    public TextMeshProUGUI PlayerNameText;

    // Tombol ikon pensil untuk mengaktifkan pengeditan nama
    public Button EditButton;

    // Nama default pemain
    private string defaultName = "Player1";

    // Kunci untuk menyimpan nama pemain di PlayerPrefs
    private string playerNameKey = "PlayerName";

    void Start()
    {
        // Muat nama pemain dari PlayerPrefs jika sudah ada, atau gunakan defaultName jika belum ada
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            // Jika ada nama yang tersimpan, gunakan nama tersebut
            string savedName = PlayerPrefs.GetString(playerNameKey);
            NamaInput.text = savedName;
            PlayerNameText.text = savedName;
        }
        else
        {
            // Jika tidak ada nama yang tersimpan, gunakan nama default
            NamaInput.text = defaultName;
            PlayerNameText.text = defaultName;
        }

        // Nonaktifkan InputField pada awalnya, sehingga tidak bisa diubah sebelum ikon pensil diklik
        NamaInput.interactable = false;

        // Set listener untuk tombol pensil agar mengaktifkan InputField
        EditButton.onClick.AddListener(ActivateNameInput);

        // Set listener untuk InputField agar menyimpan nama saat pengguna selesai mengetik
        NamaInput.onEndEdit.AddListener(SubmitName);
    }

    // Fungsi untuk mengaktifkan InputField ketika tombol pensil diklik
    public void ActivateNameInput()
    {
        NamaInput.interactable = true;  // Aktifkan InputField
        NamaInput.ActivateInputField();  // Fokus ke InputField untuk langsung bisa mengetik
    }

    // Fungsi untuk menyimpan nama dan menonaktifkan InputField setelah selesai mengetik
    public void SubmitName(string inputText)
    {
        if (!string.IsNullOrEmpty(inputText))  // Hanya simpan jika input tidak kosong
        {
            PlayerNameText.text = inputText;  // Perbarui teks dengan nama yang diinput
            PlayerPrefs.SetString(playerNameKey, inputText);  // Simpan nama ke PlayerPrefs
            PlayerPrefs.Save();  // Simpan perubahan ke disk
        }

        NamaInput.interactable = false;  // Nonaktifkan InputField setelah selesai mengetik
    }
}
