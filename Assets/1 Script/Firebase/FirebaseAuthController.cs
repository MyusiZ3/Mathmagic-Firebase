using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Added for email validation
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
    private Coroutine fadeCoroutine;

    void Start()
    {
        feedbackText.gameObject.SetActive(false); // Ensure alert is inactive at start

        // Check Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled || task.Result != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase dependencies not available: {task.Exception}");
                ShowAlert("Firebase tidak dapat dijalankan!", errorColor);
                return;
            }

            EnsureFirebase();

            // Load the target scene if user is already authenticated
            if (auth != null && auth.CurrentUser != null)
            {
                PlayerPrefs.SetString("UserId", auth.CurrentUser.UserId);
                PlayerPrefs.Save();
                SceneManager.LoadScene(targetSceneName);
            }
        });
    }

    private bool EnsureFirebase()
    {
        if (auth == null)
        {
            try { auth = FirebaseAuth.DefaultInstance; }
            catch (Exception ex) { Debug.LogError($"[FirebaseAuth] Auth init error: {ex.Message}"); }
        }
        if (firestore == null)
        {
            try { firestore = FirebaseFirestore.DefaultInstance; }
            catch (Exception ex) { Debug.LogError($"[FirebaseAuth] Firestore init error: {ex.Message}"); }
        }
        return auth != null && firestore != null;
    }

    public void Register()
    {
        string name = nameInput.text.Trim();
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // Validate user input
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowAlert("Semua kolom belum diisi!", errorColor);
            return;
        }

        // Improved email validation
        if (!IsValidEmail(email))
        {
            ShowAlert("Email tidak valid!", errorColor);
            return;
        }

        // Password validation
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

        if (!EnsureFirebase())
        {
            ShowAlert("Firebase belum siap, periksa koneksi internet!", errorColor);
            return;
        }

        // Check if username already exists in Firestore 'users' collection
        ShowAlert("Memeriksa username...", successColor);
        try
        {
            firestore.Collection("users").WhereEqualTo("username", username).GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Error checking username: {GetExceptionMessage(task.Exception)}");
                    ShowAlert("Gagal memeriksa ketersediaan username!", errorColor);
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                if (snapshot != null && snapshot.Count > 0)
                {
                    ShowAlert("Username sudah digunakan!", errorColor);
                    return;
                }

                // Attempt to create a new user since username is unique
                ShowAlert("Mendaftarkan akun...", successColor);
                auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(regTask => {
                    if (regTask.IsFaulted || regTask.IsCanceled)
                    {
                        HandleRegistrationError(regTask.Exception);
                    }
                    else
                    {
                        FirebaseUser newUser = regTask.Result.User;
                        SaveUserData(newUser.UserId, name, username, email);
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception during username check: {ex.Message}");
            ShowAlert("Gagal memproses registrasi!", errorColor);
        }
    }

    // Improved email validation function
    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    void SaveUserData(string userId, string name, string username, string email)
    {
        if (!EnsureFirebase()) return;

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference docRef = firestore.Collection("users").Document(shortId);
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "name", name },
            { "username", username },
            { "email", email },
            { "score", 0 },
            { "age", 12 } // Initial age field value
        };

        // Save user data to Firestore
        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                ShowAlert("Gagal menyimpan data pengguna!", errorColor);
            }
            else
            {
                ShowAlert("Registrasi berhasil!", successColor);

                // Kosongkan input field setelah registrasi berhasil
                nameInput.text = "";
                usernameInput.text = "";
                emailInput.text = "";
                passwordInput.text = "";
                confirmPasswordInput.text = "";

                OpenLoginUI();
            }
        });
    }

    public void OpenLoginUI()
    {
        if (registrationUI != null) registrationUI.SetActive(false);
        if (loginUI != null) loginUI.SetActive(true);
    }

    public void OpenRegistrationUI()
    {
        if (loginUI != null) loginUI.SetActive(false);
        if (registrationUI != null) registrationUI.SetActive(true);
    }

    public void Login()
    {
        string usernameOrEmail = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        // Validate login input
        if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
        {
            ShowAlert("Username/Email dan password belum diisi!", errorColor);
            return;
        }

        if (!EnsureFirebase())
        {
            ShowAlert("Firebase belum siap, periksa koneksi internet!", errorColor);
            return;
        }

        // If it's a valid email, login directly
        if (IsValidEmail(usernameOrEmail))
        {
            PerformLogin(usernameOrEmail, password);
        }
        else
        {
            // It's a username, query Firestore to find the associated email
            ShowAlert("Mencari akun...", successColor);
            try
            {
                firestore.Collection("users").WhereEqualTo("username", usernameOrEmail).GetSnapshotAsync().ContinueWithOnMainThread(task => {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError($"Error finding username for login: {GetExceptionMessage(task.Exception)}");
                        ShowAlert("Gagal memverifikasi username!", errorColor);
                        return;
                    }

                    QuerySnapshot snapshot = task.Result;
                    if (snapshot == null || snapshot.Count == 0)
                    {
                        ShowAlert("Username tidak ditemukan!", errorColor);
                        return;
                    }

                    // Get the email from the first matching user document
                    string email = "";
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        if (document.ContainsField("email"))
                        {
                            email = document.GetValue<string>("email");
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(email))
                    {
                        ShowAlert("Email tidak ditemukan untuk username ini!", errorColor);
                        return;
                    }

                    PerformLogin(email, password);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during login username lookup: {ex.Message}");
                ShowAlert("Terjadi kesalahan sistem login!", errorColor);
            }
        }
    }

    private void PerformLogin(string email, string password)
    {
        if (!EnsureFirebase())
        {
            ShowAlert("Firebase belum siap!", errorColor);
            return;
        }

        ShowAlert("Menghubungkan...", successColor);
        try
        {
            // Attempt to sign in
            auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    HandleLoginError(task.Exception);
                }
                else
                {
                    FirebaseUser user = task.Result.User;
                    PlayerPrefs.SetString("UserId", user.UserId);
                    PlayerPrefs.Save();
                    SceneManager.LoadScene(targetSceneName);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception during PerformLogin: {ex.Message}");
            ShowAlert("Gagal menghubungkan!", errorColor);
        }
    }

    public void Logout()
    {
        if (auth != null) auth.SignOut();
        ScoreManager.ClearLocalUserData();
        SceneManager.LoadScene(loginSceneName);
    }

    void ShowAlert(string message, Color color)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);

        fadeCoroutine = StartCoroutine(FadeOutAlert());
    }

    IEnumerator FadeOutAlert()
    {
        yield return new WaitForSeconds(3);
        for (float t = 1; t >= 0; t -= Time.deltaTime)
        {
            feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, t);
            yield return null;
        }
        feedbackText.gameObject.SetActive(false);
    }

    private string GetExceptionMessage(Exception exception)
    {
        if (exception == null) return "";
        Exception baseEx = exception.GetBaseException();
        return baseEx != null ? baseEx.Message : exception.Message;
    }

    // Improved error handling for registration
    private void HandleRegistrationError(Exception exception)
    {
        string msg = GetExceptionMessage(exception).ToLower();
        if (msg.Contains("email address is already in use") || msg.Contains("already-exists"))
        {
            ShowAlert("Email sudah digunakan!", errorColor);
        }
        else
        {
            ShowAlert("Terjadi kesalahan registrasi. Coba lagi!", errorColor);
        }
    }

    // Improved error handling for login
    private void HandleLoginError(Exception exception)
    {
        string fullMsg = (GetExceptionMessage(exception) + " " + (exception != null ? exception.ToString() : "")).ToLower();
        Debug.LogWarning($"[FirebaseAuth] Login error details: {fullMsg}");

        if (fullMsg.Contains("network") || fullMsg.Contains("connect") || fullMsg.Contains("unreachable") || fullMsg.Contains("dns") || fullMsg.Contains("time out"))
        {
            ShowAlert("Tidak ada koneksi internet!", errorColor);
        }
        else
        {
            ShowAlert("Email/Username atau password salah!", errorColor);
        }
    }
}