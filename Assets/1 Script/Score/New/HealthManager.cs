using UnityEngine;
using TMPro;
using System;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    public int maxHealth = 5;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText;
    public GameObject gameOverPanel;
    public GameObject countdownPanel; // Panel untuk menampilkan countdown saat HP habis

    private int currentHealth;
    private float timeUntilNextHealth = 300f; // 5 menit (300 detik)
    private float countdownTimer = 0f;
    private bool isRegenerating = false;
    private DateTime lastSaveTime; // Waktu terakhir aplikasi ditutup

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
        LoadHealthData();
        UpdateHealthText();
        UpdateHealthTimerText();
    }

    private void LoadHealthData()
    {
        currentHealth = PlayerPrefs.GetInt("CurrentHealth", maxHealth);
        countdownTimer = PlayerPrefs.GetFloat("CountdownTimer", 0);

        // Hitung waktu yang terlewat sejak aplikasi terakhir ditutup
        string lastSaveTimeString = PlayerPrefs.GetString("LastSaveTime", "");
        if (!string.IsNullOrEmpty(lastSaveTimeString))
        {
            lastSaveTime = DateTime.Parse(lastSaveTimeString);
            TimeSpan timePassed = DateTime.Now - lastSaveTime;
            countdownTimer -= (float)timePassed.TotalSeconds;
            if (countdownTimer < 0) countdownTimer = 0;
        }

        isRegenerating = currentHealth < maxHealth;
    }

    private void Update()
    {
        if (isRegenerating)
        {
            countdownTimer -= Time.deltaTime;
            PlayerPrefs.SetFloat("CountdownTimer", countdownTimer);

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
            PlayerPrefs.SetInt("CurrentHealth", currentHealth);
            UpdateHealthText();

            if (currentHealth == 0)
            {
                gameOverPanel.SetActive(true);
                countdownPanel.SetActive(true); // Tampilkan panel countdown
                countdownTimer = timeUntilNextHealth;
            }
            else
            {
                countdownTimer = timeUntilNextHealth;
            }

            isRegenerating = true;
        }
    }

    private void RegenerateOneHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            PlayerPrefs.SetInt("CurrentHealth", currentHealth);

            countdownTimer = currentHealth < maxHealth ? timeUntilNextHealth : 0;
            isRegenerating = currentHealth < maxHealth;

            UpdateHealthText();

            if (currentHealth > 0)
            {
                countdownPanel.SetActive(false); // Sembunyikan panel countdown
                gameOverPanel.SetActive(false); // Sembunyikan panel game over
            }
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = $"Nyawa: {currentHealth}";
    }

    private void UpdateHealthTimerText()
    {
        if (currentHealth == maxHealth)
        {
            healthTimerText.text = "";
            PlayerPrefs.DeleteKey("CountdownTimer");
        }
        else
        {
            int missingHealth = maxHealth - currentHealth;
            int totalMinutes = missingHealth * 5;

            int minutes = Mathf.FloorToInt(countdownTimer / 60);
            int seconds = Mathf.FloorToInt(countdownTimer % 60);
            string timeFormatted = $"{minutes:D2}:{seconds:D2}";

            healthTimerText.text = currentHealth == 0 ?
                $"{maxHealth} nyawa akan pulih dalam {totalMinutes} menit" :
                $"1 nyawa akan pulih dalam {timeFormatted}";
        }
    }

    public int GetCurrentHealth() => currentHealth;

    public bool HasEnoughHealth() => currentHealth > 0;

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
}