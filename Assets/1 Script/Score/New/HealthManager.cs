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
        LoadHealthData();
    }

    private void LoadHealthData()
    {
        DocumentReference userRef = firestore.Collection("users").Document(userId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                currentHealth = task.Result.GetValue<int>("Hp");
                long lastUpdateTimestamp = task.Result.GetValue<long>("LastHpUpdateTime");
                DateTime lastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(lastUpdateTimestamp).UtcDateTime;

                TimeSpan timePassed = DateTime.UtcNow - lastUpdateTime;
                int healthToRegenerate = Mathf.FloorToInt((float)timePassed.TotalSeconds / timeUntilNextHealth);

                currentHealth = Mathf.Min(maxHealth, currentHealth + healthToRegenerate);
                countdownTimer = timeUntilNextHealth - (float)(timePassed.TotalSeconds % timeUntilNextHealth);

                if (currentHealth >= maxHealth)
                {
                    countdownTimer = 0;
                    isRegenerating = false;
                }
                else
                {
                    isRegenerating = true;
                }

                UpdateHealthDisplay();
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
                countdownPanel.SetActive(true);
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
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        DocumentReference userRef = firestore.Collection("users").Document(userId);
        userRef.UpdateAsync(new Dictionary<string, object>
        {
            { "Hp", currentHealth },
            { "LastHpUpdateTime", currentTimestamp }
        });
    }

    private void UpdateHealthDisplay()
    {
        healthText.text = $"HP: {currentHealth}";
        UpdateHealthTimerText();
    }

    private void UpdateHealthTimerText()
    {
        if (currentHealth == maxHealth)
        {
            healthTimerText.text = "";
            countdownPanel.SetActive(false);
        }
        else
        {
            int minutes = Mathf.FloorToInt(countdownTimer / 60);
            int seconds = Mathf.FloorToInt(countdownTimer % 60);
            healthTimerText.text = $"HP +1 dalam {minutes:D2}:{seconds:D2}";
        }
    }
}
