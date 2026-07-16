using UnityEngine;
using Firebase.Auth; // Untuk menangani Firebase Auth
using UnityEngine.SceneManagement;

public class LogoutController : MonoBehaviour
{
    public string loginSceneName; // Nama scene login, diatur melalui Inspector
    public GameObject logoutConfirmationUI; // Popup konfirmasi logout (opsional)

    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance; // Inisialisasi Firebase Auth
    }

    // Fungsi logout utama
    public void Logout()
    {
        // Logout dari Firebase
        auth.SignOut();

        // Hapus seluruh cache data pengguna lokal dan hancurkan singleton managers
        ScoreManager.ClearLocalUserData();

        // Navigasi ke scene login
        SceneManager.LoadScene(loginSceneName);
    }

    // Menampilkan popup konfirmasi logout
    public void ShowLogoutConfirmation()
    {
        if (logoutConfirmationUI != null)
        {
            logoutConfirmationUI.SetActive(true);
        }
    }

    // Menghandle aksi di popup konfirmasi
    public void ConfirmLogout(bool isConfirmed)
    {
        if (isConfirmed)
        {
            Logout(); // Lakukan logout jika pengguna menekan "Yes"
        }
        else
        {
            // Sembunyikan popup jika pengguna menekan "No"
            if (logoutConfirmationUI != null)
            {
                logoutConfirmationUI.SetActive(false);
            }
        }
    }
}
