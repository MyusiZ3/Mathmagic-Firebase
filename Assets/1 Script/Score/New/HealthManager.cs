using UnityEngine;
using TMPro;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    public int maxHealth = 5;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText;
    public GameObject gameOverPanel;
    
    private int currentHealth;
    private float timeUntilNextHealth = 300f; // 5 menit (300 detik)
    private float countdownTimer = 0f;
    private bool isRegenerating = false;

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
        currentHealth = PlayerPrefs.GetInt("CurrentHealth", maxHealth);
        countdownTimer = PlayerPrefs.GetFloat("CountdownTimer", 0);
        isRegenerating = currentHealth < maxHealth;
        
        UpdateHealthText();
        UpdateHealthTimerText();
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
        PlayerPrefs.Save();
    }
}