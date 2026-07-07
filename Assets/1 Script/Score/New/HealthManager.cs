using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using System;
using System.Collections.Generic;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    public int maxHealth = 10;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText;
    public GameObject countdownPanel;

    public float timeUntilNextHealth = 300f; // 5 menit

    private int currentHealth;
    private float countdownTimer;
    private bool isRegenerating;
    private FirebaseFirestore firestore;
    private string userId;

    // Tambahkan public getter agar currentHealth bisa diakses
    public int CurrentHealth => currentHealth; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("UserId");
        if (string.IsNullOrEmpty(userId) && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            PlayerPrefs.SetString("UserId", userId);
            PlayerPrefs.Save();
        }
        LoadHealthData();
    }

    private void LoadHealthData()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID null atau kosong di HealthManager. LoadHealthData dibatalkan.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = firestore.Collection("users").Document(shortId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                bool needsUpdate = false;
                Dictionary<string, object> updates = new Dictionary<string, object>();

                // Load Hp dengan aman
                if (snapshot.ContainsField("Hp"))
                {
                    currentHealth = snapshot.GetValue<int>("Hp");
                }
                else
                {
                    currentHealth = maxHealth;
                    updates["Hp"] = maxHealth;
                    needsUpdate = true;
                }

                // Load LastHpUpdateTime dengan aman
                long lastUpdateTimestamp;
                if (snapshot.ContainsField("LastHpUpdateTime"))
                {
                    lastUpdateTimestamp = snapshot.GetValue<long>("LastHpUpdateTime");
                }
                else
                {
                    lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    updates["LastHpUpdateTime"] = lastUpdateTimestamp;
                    needsUpdate = true;
                }

                // Jika ada data HP yang belum diinisialisasi di Firestore, simpan sekarang
                if (needsUpdate)
                {
                    userRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log("Health fields initialized in Firestore for user.");
                        }
                    });
                }

                DateTime lastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(lastUpdateTimestamp).UtcDateTime;
                TimeSpan timePassed = DateTime.UtcNow - lastUpdateTime;
                
                // Hitung regenerasi HP jika HP kurang dari maksimal
                int healthToRegenerate = Mathf.FloorToInt((float)timePassed.TotalSeconds / timeUntilNextHealth);

                if (healthToRegenerate > 0 && currentHealth < maxHealth)
                {
                    int oldHealth = currentHealth;
                    currentHealth = Mathf.Min(maxHealth, currentHealth + healthToRegenerate);
                    
                    // Update timestamp berdasarkan berapa banyak HP yang teregenerasi
                    long newTimestamp = lastUpdateTimestamp + (healthToRegenerate * (long)timeUntilNextHealth);
                    long currentSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (newTimestamp > currentSeconds || currentHealth >= maxHealth)
                    {
                        newTimestamp = currentSeconds;
                    }
                    
                    userRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "Hp", currentHealth },
                        { "LastHpUpdateTime", newTimestamp }
                    });
                    
                    Debug.Log($"HP teregenerasi secara otomatis: {oldHealth} -> {currentHealth}");
                }

                // Hitung sisa waktu hitung mundur untuk HP berikutnya
                if (currentHealth >= maxHealth)
                {
                    countdownTimer = 0;
                    isRegenerating = false;
                }
                else
                {
                    countdownTimer = timeUntilNextHealth - (float)(timePassed.TotalSeconds % timeUntilNextHealth);
                    isRegenerating = true;
                }

                UpdateHealthDisplay();
            }
            else
            {
                Debug.LogWarning("Dokumen pengguna tidak ditemukan untuk HealthManager di Firestore.");
            }
        });
    }

    private void Update()
    {
        if (isRegenerating && currentHealth < maxHealth)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0)
            {
                RegenerateOneHealth();
            }
            UpdateHealthTimerText();
        }
    }

    public void LoseHealth()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            UpdateHealthData();

            if (currentHealth == 0)
            {
                countdownTimer = timeUntilNextHealth;
                isRegenerating = true;
                if (countdownPanel != null)
                {
                    countdownPanel.SetActive(true);
                }
            }
            else
            {
                countdownTimer = timeUntilNextHealth;
            }

            UpdateHealthDisplay();
        }
    }

    private void RegenerateOneHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHealthData();

            if (currentHealth < maxHealth)
            {
                countdownTimer = timeUntilNextHealth;
                isRegenerating = true;
            }
            else
            {
                countdownTimer = 0;
                isRegenerating = false;
            }

            UpdateHealthDisplay();
        }
    }

    private void UpdateHealthData()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID null atau kosong di HealthManager. UpdateHealthData dibatalkan.");
            return;
        }

        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = firestore.Collection("users").Document(shortId);
        userRef.UpdateAsync(new Dictionary<string, object>
        {
            { "Hp", currentHealth },
            { "LastHpUpdateTime", currentTimestamp }
        });
    }

    private void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}";
        }
        UpdateHealthTimerText();

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(currentHealth == 0);
        }
    }

    private void UpdateHealthTimerText()
    {
        if (currentHealth == maxHealth)
        {
            if (healthTimerText != null)
            {
                healthTimerText.text = "";
            }
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(false);
            }
        }
        else
        {
            int minutes = Mathf.FloorToInt(countdownTimer / 60);
            int seconds = Mathf.FloorToInt(countdownTimer % 60);
            if (healthTimerText != null)
            {
                healthTimerText.text = $"HP +1 dalam {minutes:D2}:{seconds:D2}";
            }
        }
    }
}
