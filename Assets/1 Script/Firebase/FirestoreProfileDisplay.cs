using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using TMPro;

public class FirestoreProfileDisplay : MonoBehaviour
{
    [Header("Firestore Config")]
    public string userId; // User ID akan diisi otomatis setelah login.

    [Header("UI Elements")]
    public TMP_InputField nameInputField;
    public TMP_InputField usernameInputField;
    public TMP_InputField ageInputField;
    public TMP_InputField emailInputField; // Input field untuk email (tidak bisa diedit)

    public TMP_Text scoreText; // Untuk menampilkan skor
    public TMP_Text greetingText; // Untuk greeting di Main Menu
    public TMP_Text buttonText; // Text pada tombol edit/simpan

    public TMP_Text usernameDisplayText; // TMP_Text untuk menampilkan username di halaman lain
    public TMP_Text emailDisplayText; // TMP_Text untuk menampilkan email user di halaman lain

    public GameObject editSaveButton;

    private FirebaseFirestore firestore;
    private FirebaseAuth auth;
    private bool isEditing = false; // Menandakan apakah dalam mode edit
    private int cachedScore = 0; // Cache untuk menyimpan skor sementara

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        // Cek jika user sudah login, ambil user ID
        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
        }
        else
        {
            Debug.LogError("Tidak ada pengguna yang login.");
            return;
        }

        SetInputFieldsInteractable(false); // Nonaktifkan input field saat awal
        LoadUserData();
        UpdateGreeting(); // Update greeting di awal
    }

    void LoadUserData()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak tersedia!");
            return;
        }

        firestore.Collection("users").Document(userId).Listen(snapshot =>
        {
            if (snapshot == null || !snapshot.Exists)
            {
                Debug.LogError("Dokumen pengguna tidak ditemukan.");
                return;
            }

            // Mengisi data dari Firestore ke InputField dan UI Text
            if (snapshot.TryGetValue("name", out string name))
            {
                nameInputField.text = name;
                UpdateGreeting(); // Perbarui greeting jika nama berubah
            }
            else
            {
                Debug.LogWarning("Nama tidak ditemukan di Firestore.");
            }

            if (snapshot.TryGetValue("username", out string username))
            {
                usernameInputField.text = username;
                if (usernameDisplayText != null)
                {
                    usernameDisplayText.text = username; // Update username display
                }
            }
            else
            {
                Debug.LogWarning("Username tidak ditemukan di Firestore.");
            }

            if (snapshot.TryGetValue("score", out int score))
            {
                cachedScore = score; // Cache score agar tidak berubah saat edit profile
                scoreText.text = $"{score}";
            }
            else
            {
                Debug.LogWarning("Skor tidak ditemukan di Firestore.");
            }

            if (snapshot.TryGetValue("age", out int age))
            {
                ageInputField.text = age.ToString();
            }
            else
            {
                Debug.LogWarning("Usia tidak ditemukan di Firestore.");
            }

            if (snapshot.TryGetValue("email", out string email))
            {
                emailInputField.text = email;
                emailInputField.interactable = false; // Membuat email tidak bisa diedit

                if (emailDisplayText != null)
                {
                    emailDisplayText.text = email; // Update email display
                }
            }
            else
            {
                Debug.LogWarning("Email tidak ditemukan di Firestore.");
            }

            Debug.Log("Data pengguna berhasil dimuat dan diupdate secara realtime.");
        });
    }

    public void ToggleEditSave()
    {
        if (isEditing)
        {
            // Simpan data ke Firestore
            SaveUserData();
            SetInputFieldsInteractable(false);
            buttonText.text = "Edit";
        }
        else
        {
            // Aktifkan mode edit
            SetInputFieldsInteractable(true);
            buttonText.text = "Simpan";
        }

        isEditing = !isEditing;
    }

    void SaveUserData()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID tidak tersedia!");
            return;
        }

        // Data yang akan diperbarui ke Firestore
        Dictionary<string, object> updatedData = new Dictionary<string, object>
        {
            { "name", nameInputField.text },
            { "username", usernameInputField.text },
            { "age", int.Parse(ageInputField.text) },
            { "score", cachedScore } // Pastikan skor tidak berubah
        };

        firestore.Collection("users").Document(userId).UpdateAsync(updatedData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Gagal memperbarui data pengguna.");
            }
            else
            {
                Debug.Log("Data pengguna berhasil diperbarui.");
            }
        });
    }

    void SetInputFieldsInteractable(bool interactable)
    {
        // Mengatur apakah input field bisa di-edit
        nameInputField.interactable = interactable;
        usernameInputField.interactable = interactable;
        ageInputField.interactable = interactable;

        // Email input field tetap tidak bisa diedit
        emailInputField.interactable = false; 
    }

    void UpdateGreeting()
    {
        string greeting;

        // Mendapatkan waktu saat ini
        int hour = DateTime.Now.Hour;

        // Menentukan greeting berdasarkan waktu
        if (hour >= 4 && hour < 12)
            greeting = "Selamat Pagi";
        else if (hour >= 12 && hour < 15)
            greeting = "Selamat Siang";
        else if (hour >= 15 && hour < 18)
            greeting = "Selamat Sore";
        else
            greeting = "Selamat Malam";

        // Nama dipotong jika lebih dari 8 karakter
        string displayName = string.IsNullOrEmpty(nameInputField.text) ? "User" : LimitCharacters(nameInputField.text);

        // Mengatur teks greeting
        greetingText.text = $"{greeting}, {displayName}!";
    }

    string LimitCharacters(string input)
    {
        // Membatasi panjang karakter maksimal 8 dan menambahkan "..." jika lebih
        return input.Length > 8 ? input.Substring(0, 8) + "..." : input;
    }
}
