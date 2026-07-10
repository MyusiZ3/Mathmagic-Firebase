using UnityEngine;
using TMPro;

public class HealthUIUpdater : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText;
    public GameObject countdownPanel;

    private void OnEnable()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthUpdated += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (HealthManager.HasInstance)
        {
            var mgr = HealthManager.Instance;
            if (mgr != null)
            {
                mgr.OnHealthUpdated -= UpdateUI;
            }
        }
    }

    private void Start()
    {
        if (countdownPanel == null)
        {
            Debug.LogWarning($"[HealthUIUpdater] 'countdownPanel' belum di-assign di Inspector pada GameObject: {gameObject.name}. Tolong seret panel countdown Anda ke slot ini!");
        }
        UpdateUI();
    }

    private void Update()
    {
        // Selalu perbarui UI setiap frame agar timer hitung mundur terus berjalan lancar di layar
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (HealthManager.Instance == null) return;

        int currentHealth = HealthManager.Instance.CurrentHealth;
        int maxHealth = HealthManager.Instance.MaxHealth;
        float countdownTimer = HealthManager.Instance.CountdownTimer;

        // Jika HP masih bernilai -1, artinya data sedang dimuat dari Firestore.
        // Kita sembunyikan panel cooldown dan lewati update agar tidak berkedip (flicker).
        if (currentHealth == -1)
        {
            if (countdownPanel != null) countdownPanel.SetActive(false);
            return;
        }

        // 1. Update text HP
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}";
        }

        // 2. Aktifkan countdown panel jika HP = 0 (game over/menunggu)
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(currentHealth == 0);
        }

        // 3. Update text timer hitung mundur regenerasi
        if (healthTimerText != null)
        {
            if (currentHealth >= maxHealth)
            {
                healthTimerText.text = "";
            }
            else
            {
                int minutes = Mathf.FloorToInt(countdownTimer / 60);
                int seconds = Mathf.FloorToInt(countdownTimer % 60);
                healthTimerText.text = $"HP +1 dalam {minutes:D2}:{seconds:D2}";
            }
        }
    }

    // Fungsi jembatan agar bisa dipanggil dari event OnClick tombol di Unity Inspector
    public void DecreaseHealth()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.LoseHealth();
        }
    }
}
