using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIUpdater : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthTimerText;
    public GameObject countdownPanel;

    [Header("HP Sprite References")]
    public Image hpImageDisplay;
    public Sprite spriteFull;
    public Sprite sprite75;
    public Sprite sprite50;
    public Sprite sprite25;
    public Sprite sprite0;

    [Header("Heartbeat Effect Settings")]
    [Tooltip("Target RectTransform untuk efek berdetak (heartbeat) pada overlay countdown.")]
    public RectTransform heartbeatTarget;
    [Tooltip("Apakah efek berdetak aktif.")]
    public bool enableHeartbeat = true;
    [Tooltip("Seberapa cepat detakan jantung (frekuensi).")]
    public float beatSpeed = 4f;
    [Tooltip("Seberapa besar perubahan skala saat berdetak.")]
    public float beatScaleMultiplier = 0.15f;

    private Vector3 originalHeartbeatScale = Vector3.one;

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
        if (heartbeatTarget != null)
        {
            originalHeartbeatScale = heartbeatTarget.localScale;
        }

        // Terapkan outline hitam secara dinamis agar teks HP selalu terlihat jelas di latar belakang apa pun
        if (healthText != null)
        {
            healthText.outlineWidth = 0.2f;
            healthText.outlineColor = Color.black;
            
            if (healthText.fontMaterial != null)
            {
                healthText.fontMaterial.EnableKeyword("OUTLINE_ON");
                healthText.fontMaterial.SetColor(TMPro.ShaderUtilities.ID_OutlineColor, Color.black);
                healthText.fontMaterial.SetFloat(TMPro.ShaderUtilities.ID_OutlineWidth, 0.2f);
            }
        }

        UpdateUI();
    }

    private void Update()
    {
        // Selalu perbarui UI setiap frame agar timer hitung mundur terus berjalan lancar di layar
        UpdateUI();

        // Efek detak jantung (heartbeat) pada target overlay countdown
        if (enableHeartbeat && heartbeatTarget != null && countdownPanel != null && countdownPanel.activeInHierarchy)
        {
            float time = Time.time * beatSpeed;
            float t = time % (2 * Mathf.PI); 
            float scaleOffset = 0f;

            if (t < 0.5f)
            {
                scaleOffset = Mathf.Sin(t * Mathf.PI / 0.5f) * beatScaleMultiplier;
            }
            else if (t < 0.8f)
            {
                scaleOffset = 0f;
            }
            else if (t < 1.3f)
            {
                scaleOffset = Mathf.Sin((t - 0.8f) * Mathf.PI / 0.5f) * (beatScaleMultiplier * 0.5f);
            }
            else
            {
                scaleOffset = 0f;
            }

            heartbeatTarget.localScale = originalHeartbeatScale * (1f + scaleOffset);
        }
        else if (heartbeatTarget != null && heartbeatTarget.localScale != originalHeartbeatScale)
        {
            // Reset ke skala asli jika panel countdown dinonaktifkan
            heartbeatTarget.localScale = originalHeartbeatScale;
        }
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

        // Update HP Sprite berdasarkan persentase sisa nyawa
        if (hpImageDisplay != null)
        {
            if (currentHealth >= maxHealth)
            {
                hpImageDisplay.sprite = spriteFull;
            }
            else if (currentHealth <= 0)
            {
                hpImageDisplay.sprite = sprite0;
            }
            else
            {
                float percent = (float)currentHealth / maxHealth;
                if (percent >= 0.75f)
                {
                    hpImageDisplay.sprite = sprite75;
                }
                else if (percent >= 0.50f)
                {
                    hpImageDisplay.sprite = sprite50;
                }
                else if (percent >= 0.25f)
                {
                    hpImageDisplay.sprite = sprite25;
                }
                else
                {
                    // Sisa HP sangat sedikit (di bawah 25% tapi belum 0) tetap menampilkan sprite 25%
                    hpImageDisplay.sprite = sprite25;
                }
            }
        }

        // 1. Update text HP
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}";
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
