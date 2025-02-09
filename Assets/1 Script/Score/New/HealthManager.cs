using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    public int maxHealth = 5;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText;
    public GameObject countdownPanel;
    
    [Header("Question Panel Settings")]
    public GameObject questionPanel;
    public Button[] answerButtons;
    
    [Header("Health Regeneration Settings")]
    public float timeUntilNextHealth = 300f; // Bisa diatur dari Inspector, default 5 menit (300 detik)
    
    private int currentHealth;
    private float countdownTimer = 0f;
    private bool isRegenerating = false;
    private DateTime lastSaveTime;

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
        UpdateHealthDisplay();
        ToggleQuestionPanel();
    }

    private void LoadHealthData()
    {
        currentHealth = PlayerPrefs.GetInt("CurrentHealth", maxHealth);
        countdownTimer = PlayerPrefs.GetFloat("CountdownTimer", timeUntilNextHealth);

        string lastSaveTimeString = PlayerPrefs.GetString("LastSaveTime", "");
        if (!string.IsNullOrEmpty(lastSaveTimeString))
        {
            lastSaveTime = DateTime.Parse(lastSaveTimeString);
            TimeSpan timePassed = DateTime.Now - lastSaveTime;
            float totalSecondsPassed = (float)timePassed.TotalSeconds;

            int healthToRegenerate = Mathf.FloorToInt(totalSecondsPassed / timeUntilNextHealth);
            currentHealth = Mathf.Min(maxHealth, currentHealth + healthToRegenerate);
            countdownTimer = timeUntilNextHealth - (totalSecondsPassed % timeUntilNextHealth);
        }

        if (currentHealth >= maxHealth)
        {
            countdownTimer = 0;
            isRegenerating = false;
        }
        else
        {
            isRegenerating = true;
        }

        PlayerPrefs.SetInt("CurrentHealth", currentHealth);
        PlayerPrefs.SetFloat("CountdownTimer", countdownTimer);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (isRegenerating)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0)
            {
                RegenerateOneHealth();
            }
            UpdateHealthTimerText();
            PlayerPrefs.SetFloat("CountdownTimer", countdownTimer);
            PlayerPrefs.Save();
        }
    }

    public void LoseHealth()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            PlayerPrefs.SetInt("CurrentHealth", currentHealth);
            PlayerPrefs.Save();
            UpdateHealthDisplay();

            if (currentHealth == 0)
            {
                countdownTimer = timeUntilNextHealth;
                ToggleQuestionPanel();
                countdownPanel.SetActive(true);
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
            PlayerPrefs.Save();

            if (currentHealth < maxHealth)
            {
                countdownTimer = timeUntilNextHealth;
                isRegenerating = true;
            }
            else
            {
                countdownTimer = 0;
                isRegenerating = false;
                countdownPanel.SetActive(false);
            }

            UpdateHealthDisplay();

            if (currentHealth > 0)
            {
                ToggleQuestionPanel();
            }
        }
    }

    private void UpdateHealthDisplay()
    {
        healthText.text = $"Nyawa: {currentHealth}";
        UpdateHealthTimerText();
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
            int minutes = Mathf.FloorToInt(countdownTimer / 60);
            int seconds = Mathf.FloorToInt(countdownTimer % 60);
            string timeFormatted = $"{minutes:D2}:{seconds:D2}";

            healthTimerText.text = $"1 nyawa akan pulih dalam {timeFormatted}";
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void ToggleQuestionPanel()
    {
        bool isActive = currentHealth > 0;
        
        if (questionPanel != null) 
        {
            questionPanel.SetActive(isActive);
        }

        foreach (Button btn in answerButtons)
        {
            btn.interactable = isActive;
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());
        PlayerPrefs.SetInt("CurrentHealth", currentHealth);
        PlayerPrefs.SetFloat("CountdownTimer", countdownTimer);
        PlayerPrefs.Save();
    }
}
