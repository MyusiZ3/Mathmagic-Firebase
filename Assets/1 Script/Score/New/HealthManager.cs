using UnityEngine;
using TMPro;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 5;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText; // UI untuk menampilkan waktu regenerasi
    public GameObject gameOverPanel;
    
    private int currentHealth;
    private float timeUntilNextHealth = 300f; // 5 menit (300 detik)
    private float countdownTimer = 0f; // Timer untuk nyawa berikutnya
    private bool isRegenerating = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthText();
        UpdateHealthTimerText();
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
        }
    }

    public void LoseHealth()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            UpdateHealthText();

            if (currentHealth == 0)
            {
                gameOverPanel.SetActive(true);
                countdownTimer = maxHealth * timeUntilNextHealth;
                isRegenerating = true;
            }
            else
            {
                countdownTimer = timeUntilNextHealth;
                isRegenerating = true;
            }
        }
    }

    private void RegenerateOneHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            countdownTimer = currentHealth < maxHealth ? timeUntilNextHealth : 0;
            isRegenerating = currentHealth < maxHealth;
            UpdateHealthText();
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = "Nyawa: " + currentHealth;
    }

    private void UpdateHealthTimerText()
    {
        if (currentHealth == maxHealth)
        {
            healthTimerText.text = "";
        }
        else
        {
            int minutes = Mathf.FloorToInt(countdownTimer / 60);
            int seconds = Mathf.FloorToInt(countdownTimer % 60);
            string timeFormatted = string.Format("{0:D2}:{1:D2}", minutes, seconds);

            if (currentHealth == 0)
            {
                int totalTime = maxHealth * 5; // Total pemulihan semua nyawa dalam menit
                healthTimerText.text = $"{maxHealth} nyawa akan dipulihkan dalam {totalTime} menit";
            }
            else
            {
                healthTimerText.text = $"1 nyawa akan dipulihkan dalam {timeFormatted}";
            }
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
