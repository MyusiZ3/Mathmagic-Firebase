using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class HealthManager : MonoBehaviour
{
    private static HealthManager instance;
    private static bool isQuitting = false;

    public static bool HasInstance => instance != null && !isQuitting;

    public static HealthManager Instance
    {
        get
        {
            if (isQuitting)
            {
                return null;
            }
            if (instance == null)
            {
                instance = FindFirstObjectByType<HealthManager>();
                if (instance == null && !isQuitting)
                {
                    GameObject go = new GameObject("HealthManager (Auto-Created)");
                    instance = go.AddComponent<HealthManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("HealthManager otomatis dibuat untuk keperluan playtesting.");
                }
            }
            return instance;
        }
    }

    [Header("Health Balance Settings")]
    [Tooltip("Jumlah maksimum HP (Nyawa) yang dimiliki pemain.")]
    public int maxHealth = 10;

    [Tooltip("Durasi waktu (dalam detik) untuk meregenerasi 1 HP (misal: 300 detik = 5 menit).")]
    public float timeUntilNextHealth = 300f; // 5 menit

    private int currentHealth = -1; // -1 menandakan sedang loading dari Firestore
    private float countdownTimer;
    private bool isRegenerating;
    private FirebaseFirestore firestore;
    private string userId;

    // Public getters untuk dibaca oleh UI (seperti HealthUIUpdater)
    public int CurrentHealth => currentHealth; 
    public int MaxHealth => maxHealth;
    public float CountdownTimer => countdownTimer;
    public bool IsRegenerating => isRegenerating;

    // Event C# jika ada komponen UI yang ingin merespon secara reaktif
    public event Action OnHealthUpdated;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticVariables()
    {
        instance = null;
        isQuitting = false;
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

        if (RemoteSettingsManager.Instance != null)
        {
            ApplyRemoteSettings();
            LoadHealthData();
            RemoteSettingsManager.Instance.OnSettingsLoaded += OnRemoteSettingsLoaded;
        }
        else
        {
            LoadHealthData();
        }
    }

    private ListenerRegistration userHealthListener;

    private void OnDestroy()
    {
        if (userHealthListener != null)
        {
            userHealthListener.Stop();
            userHealthListener = null;
        }
        if (instance == this)
        {
            instance = null;
            isQuitting = true;
        }
        if (RemoteSettingsManager.HasInstance)
        {
            RemoteSettingsManager.Instance.OnSettingsLoaded -= OnRemoteSettingsLoaded;
        }
    }

    private void OnRemoteSettingsLoaded()
    {
        ApplyRemoteSettings();
        if (currentHealth == -1)
        {
            LoadHealthData();
        }
    }

    private void ApplyRemoteSettings()
    {
        if (RemoteSettingsManager.Instance != null)
        {
            maxHealth = RemoteSettingsManager.Instance.maxHealth;
            timeUntilNextHealth = RemoteSettingsManager.Instance.healthCooldownSeconds;
            Debug.Log($"[HealthManager] Applied Remote Settings: maxHealth={maxHealth}, timeUntilNextHealth={timeUntilNextHealth}");
            
            if (currentHealth != -1)
            {
                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                    UpdateHealthData();
                }
                if (currentHealth < maxHealth)
                {
                    if (countdownTimer > timeUntilNextHealth)
                    {
                        countdownTimer = timeUntilNextHealth;
                    }
                }
                NotifyUI();
            }
        }
    }

    private void LoadHealthData()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("User ID belum ada di HealthManager. Menunggu autentikasi.");
            return;
        }

        if (userHealthListener != null)
        {
            userHealthListener.Stop();
            userHealthListener = null;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = firestore.Collection("users").Document(shortId);
        
        userHealthListener = userRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                bool needsUpdate = false;
                Dictionary<string, object> updates = new Dictionary<string, object>();

                // Load Hp
                if (snapshot.ContainsField("Hp"))
                {
                    int remoteHp = snapshot.GetValue<int>("Hp");
                    currentHealth = Mathf.Min(remoteHp, maxHealth);
                    if (remoteHp > maxHealth)
                    {
                        currentHealth = maxHealth;
                        updates["Hp"] = maxHealth;
                        needsUpdate = true;
                    }
                }
                else
                {
                    currentHealth = maxHealth;
                    updates["Hp"] = maxHealth;
                    needsUpdate = true;
                }

                // Load LastHpUpdateTime
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
                
                // Hitung regenerasi HP
                int healthToRegenerate = Mathf.FloorToInt((float)timePassed.TotalSeconds / timeUntilNextHealth);

                if (healthToRegenerate > 0 && currentHealth < maxHealth)
                {
                    int oldHealth = currentHealth;
                    currentHealth = Mathf.Min(maxHealth, currentHealth + healthToRegenerate);
                    
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

                NotifyUI();
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
            }
            else
            {
                countdownTimer = timeUntilNextHealth;
            }

            NotifyUI();
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

            NotifyUI();
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

    private void NotifyUI()
    {
        OnHealthUpdated?.Invoke();
    }
}
