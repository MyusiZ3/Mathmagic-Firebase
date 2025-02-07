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
    public GameObject countdownPanel; // Panel "HP habis, tunggu X menit"
    
    [Header("Question Panel Settings")]
    public GameObject questionPanel; // Panel pertanyaan dan tombol jawaban
    public Button[] answerButtons; // Tombol True/False
    
    private int currentHealth;
    private float timeUntilNextHealth = 300f; // 5 menit (300 detik)
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
        countdownTimer = PlayerPrefs.GetFloat("CountdownTimer", 0);

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
                ToggleQuestionPanel(); // Aktifkan panel pertanyaan jika HP pulih
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
            UpdateHealthDisplay();

            if (currentHealth == 0)
            {
                countdownTimer = timeUntilNextHealth;
                ToggleQuestionPanel(); // Nonaktifkan panel pertanyaan
                countdownPanel.SetActive(true);
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

            UpdateHealthDisplay();

            if (currentHealth > 0)
            {
                countdownPanel.SetActive(false);
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

    // Aktifkan/nonaktifkan panel pertanyaan dan tombol jawaban
    private void ToggleQuestionPanel()
    {
        bool isActive = currentHealth > 0;
        
        // Matikan panel pertanyaan
        if (questionPanel != null) 
        {
            questionPanel.SetActive(isActive);
        }

        // Matikan tombol jawaban
        foreach (Button btn in answerButtons)
        {
            btn.interactable = isActive;
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
}