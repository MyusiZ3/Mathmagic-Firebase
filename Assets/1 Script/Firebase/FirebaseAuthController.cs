using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FirebaseAuthController : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string targetSceneName = "SceneA";
    public string loginSceneName = "LoginScene";

    [Header("UI Elements")]
    public InputField nameInput, usernameInput, emailInput, passwordInput, confirmPasswordInput;
    public InputField loginEmailInput, loginPasswordInput;
    public Text feedbackText;
    public GameObject registrationUI, loginUI;

    [Header("Feedback Colors")]
    public Color successColor = Color.green;
    public Color errorColor = Color.red;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result != DependencyStatus.Available)
            {
                ShowAlert("Firebase tidak dapat dijalankan!", errorColor);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            firestore = FirebaseFirestore.DefaultInstance;

            if (auth.CurrentUser != null)
            {
                SceneManager.LoadScene(targetSceneName);
            }
        });

        feedbackText.gameObject.SetActive(false); // Pastikan alert tidak aktif saat awal
    }

    public void Register()
    {
        string name = nameInput.text;
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowAlert("Semua kolom belum diisi!", errorColor);
            return;
        }

        if (!email.EndsWith("@gmail.com"))
        {
            ShowAlert("Gunakan @gmail.com!", errorColor);
            return;
        }

        if (password.Length <= 6)
        {
            ShowAlert("Password kurang dari 6 karakter!", errorColor);
            return;
        }

        if (password != confirmPassword)
        {
            ShowAlert("Password tidak cocok!", errorColor);
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                if (task.Exception.InnerException.Message.Contains("email address is already in use"))
                {
                    ShowAlert("Email sudah digunakan!", errorColor);
                }
                else
                {
                    ShowAlert("Terjadi kesalahan. Coba lagi!", errorColor);
                }
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                SaveUserData(newUser.UserId, name, username, email);
            }
        });
    }

    void SaveUserData(string userId, string name, string username, string email)
    {
        DocumentReference docRef = firestore.Collection("users").Document(userId);
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "name", name },
            { "username", username },
            { "email", email },
            { "score", 0 },
            { "age", 12 } // Field umur dengan nilai awal 12
        };

        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                ShowAlert("Registrasi berhasil!", successColor);
                registrationUI.SetActive(false);
                loginUI.SetActive(true);
            }
            else
            {
                ShowAlert("Gagal menyimpan data pengguna!", errorColor);
            }
        });
    }

    public void Login()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowAlert("Email dan password belum diisi!", errorColor);
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                if (task.Exception.InnerException.Message.Contains("password is invalid"))
                {
                    ShowAlert("Email atau password salah!", errorColor);
                }
                else if (task.Exception.InnerException.Message.Contains("network error"))
                {
                    ShowAlert("Tidak ada koneksi internet!", errorColor);
                }
                else
                {
                    ShowAlert("Terjadi kesalahan. Coba lagi nanti!", errorColor);
                }
            }
            else
            {
                FirebaseUser user = task.Result.User;
                PlayerPrefs.SetString("UserId", user.UserId);
                SceneManager.LoadScene(targetSceneName);
            }
        });
    }

    public void Logout()
    {
        auth.SignOut();
        PlayerPrefs.DeleteKey("UserId");
        SceneManager.LoadScene(loginSceneName);
    }

    void ShowAlert(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);

        StartCoroutine(FadeOutAlert());
    }

    IEnumerator FadeOutAlert()
    {
        yield return new WaitForSeconds(2);
        for (float t = 1; t >= 0; t -= Time.deltaTime)
        {
            feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, t);
            yield return null;
        }
        feedbackText.gameObject.SetActive(false);
    }
}
