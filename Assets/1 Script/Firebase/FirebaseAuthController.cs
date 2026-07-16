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

    void Start()
    {
        // Check Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result != DependencyStatus.Available)
            {
                ShowAlert("Firebase tidak dapat dijalankan!", errorColor);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            firestore = FirebaseFirestore.DefaultInstance;

            // Load the target scene if user is already authenticated
            if (auth.CurrentUser != null)
            {
                PlayerPrefs.SetString("UserId", auth.CurrentUser.UserId);
                PlayerPrefs.Save();
                SceneManager.LoadScene(targetSceneName);
            }
        });

        feedbackText.gameObject.SetActive(false); // Ensure alert is inactive at start
    }

    public void Register()
    {
        string name = nameInput.text;
        string username = usernameInput.text;
        string email = emailInput.text;
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

        // Attempt to create a new user
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                HandleRegistrationError(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                SaveUserData(newUser.UserId, name, username, email);
            }
        });
    }

    // Improved email validation function
    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    void SaveUserData(string userId, string name, string username, string email)
    {
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
            if (task.IsCompleted)
            {
                ShowAlert("Registrasi berhasil!", successColor);

                // Kosongkan input field setelah registrasi berhasil
                nameInput.text = "";
                usernameInput.text = "";
                emailInput.text = "";
                passwordInput.text = "";
                confirmPasswordInput.text = "";

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

        // Validate login input
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowAlert("Email dan password belum diisi!", errorColor);
            return;
        }

        // Attempt to sign in
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                HandleLoginError(task.Exception);
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
        ScoreManager.ClearLocalUserData();
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

    // Improved error handling for registration
    // Improved error handling for registration
    private void HandleRegistrationError(Exception exception)
    {
        if (exception.Message.Contains("email address is already in use"))
        {
            ShowAlert("Email sudah digunakan!", errorColor);
        }
        else
        {
            ShowAlert("Terjadi kesalahan. Coba lagi!", errorColor);
        }
    }

    // Improved error handling for login
    private void HandleLoginError(Exception exception)
    {
        if (exception.Message.Contains("password is invalid"))
        {
            ShowAlert("Email atau password salah!", errorColor);
        }
        else if (exception.Message.Contains("network error"))
        {
            ShowAlert("Tidak ada koneksi internet!", errorColor);
        }
        else
        {
            ShowAlert("Terjadi kesalahan. Coba lagi nanti!", errorColor);
        }
    }
}