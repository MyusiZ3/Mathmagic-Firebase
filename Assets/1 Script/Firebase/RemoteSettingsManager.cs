using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class RemoteSettingsManager : MonoBehaviour
{
    private static RemoteSettingsManager instance;
    private static bool isQuitting = false;

    public static bool HasInstance => instance != null && !isQuitting;

    public static RemoteSettingsManager Instance
    {
        get
        {
            if (isQuitting)
            {
                return null;
            }
            if (instance == null)
            {
                instance = FindFirstObjectByType<RemoteSettingsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("RemoteSettingsManager (Auto-Created)");
                    instance = go.AddComponent<RemoteSettingsManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("RemoteSettingsManager created automatically.");
                }
            }
            return instance;
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



    [Header("Default Local Settings")]
    public int defaultMaxHealth = 10;
    public float defaultHealthCooldownSeconds = 300f;
    public float defaultQuestionTimerSeconds = 60f;
    public int defaultMainLevelReward = 100;
    public int defaultBonusLevelReward = 250;
    public int defaultScoreToUnlockA = 30;
    public int defaultScoreToUnlockB = 80;
    public int defaultScoreToUnlockC = 150;
    public int defaultScoreToUnlockD = 200;

    [Header("Current Remote Settings")]
    public int maxHealth;
    public float healthCooldownSeconds;
    public float questionTimerSeconds;
    public int mainLevelReward;
    public int bonusLevelReward;
    public int scoreToUnlockA;
    public int scoreToUnlockB;
    public int scoreToUnlockC;
    public int scoreToUnlockD;

    public bool IsLoaded { get; private set; } = false;
    public event Action OnSettingsLoaded;

    private ListenerRegistration settingsListener;
    private FirebaseFirestore db;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaults();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                FetchRemoteSettings();
            }
            else
            {
                Debug.LogError($"[RemoteSettingsManager] Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void InitializeDefaults()
    {
        maxHealth = defaultMaxHealth;
        healthCooldownSeconds = defaultHealthCooldownSeconds;
        questionTimerSeconds = defaultQuestionTimerSeconds;
        mainLevelReward = defaultMainLevelReward;
        bonusLevelReward = defaultBonusLevelReward;
        scoreToUnlockA = defaultScoreToUnlockA;
        scoreToUnlockB = defaultScoreToUnlockB;
        scoreToUnlockC = defaultScoreToUnlockC;
        scoreToUnlockD = defaultScoreToUnlockD;
    }

    private int SafeGetInt(DocumentSnapshot snapshot, string field, int defaultValue)
    {
        if (snapshot.ContainsField(field))
        {
            try
            {
                object val = snapshot.GetValue<object>(field);
                return Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RemoteSettings] Error parsing field {field}: {ex.Message}");
            }
        }
        return defaultValue;
    }

    private float SafeGetFloat(DocumentSnapshot snapshot, string field, float defaultValue)
    {
        if (snapshot.ContainsField(field))
        {
            try
            {
                object val = snapshot.GetValue<object>(field);
                return Convert.ToSingle(val);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RemoteSettings] Error parsing field {field}: {ex.Message}");
            }
        }
        return defaultValue;
    }

    public void FetchRemoteSettings()
    {
        if (settingsListener != null)
        {
            settingsListener.Stop();
            settingsListener = null;
        }

        DocumentReference docRef = db.Collection("settings").Document("global");
        settingsListener = docRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                maxHealth = SafeGetInt(snapshot, "max_health", defaultMaxHealth);
                healthCooldownSeconds = SafeGetFloat(snapshot, "health_cooldown_seconds", defaultHealthCooldownSeconds);
                questionTimerSeconds = SafeGetFloat(snapshot, "question_timer_seconds", defaultQuestionTimerSeconds);
                mainLevelReward = SafeGetInt(snapshot, "main_level_score_reward", defaultMainLevelReward);
                bonusLevelReward = SafeGetInt(snapshot, "bonus_level_score_reward", defaultBonusLevelReward);
                scoreToUnlockA = SafeGetInt(snapshot, "achievement_threshold_a", defaultScoreToUnlockA);
                scoreToUnlockB = SafeGetInt(snapshot, "achievement_threshold_b", defaultScoreToUnlockB);
                scoreToUnlockC = SafeGetInt(snapshot, "achievement_threshold_c", defaultScoreToUnlockC);
                scoreToUnlockD = SafeGetInt(snapshot, "achievement_threshold_d", defaultScoreToUnlockD);

                IsLoaded = true;
                Debug.Log($"[RemoteSettings] Settings loaded/updated in real-time: max_health={maxHealth}, health_cooldown={healthCooldownSeconds}, timer={questionTimerSeconds}, main_reward={mainLevelReward}, bonus_reward={bonusLevelReward}, achA={scoreToUnlockA}, achB={scoreToUnlockB}, achC={scoreToUnlockC}, achD={scoreToUnlockD}");
                
                // Let systems like HealthManager/Timer know we updated
                if (OnSettingsLoaded != null)
                {
                    Delegate[] invocationList = OnSettingsLoaded.GetInvocationList();
                    Debug.Log($"[RemoteSettings] Invoking OnSettingsLoaded with {invocationList.Length} listeners.");
                    foreach (var del in invocationList)
                    {
                        Debug.Log($"[RemoteSettings] Listener: {del.Method.DeclaringType.Name}.{del.Method.Name} on target {del.Target}");
                    }
                }
                else
                {
                    Debug.Log("[RemoteSettings] OnSettingsLoaded has NO listeners.");
                }
                OnSettingsLoaded?.Invoke();
            }
            else
            {
                Debug.LogWarning("[RemoteSettings] Global settings document doesn't exist, using defaults.");
            }
        });

        settingsListener.ListenerTask.ContinueWithOnMainThread(listenerTask =>
        {
            if (listenerTask.IsFaulted)
            {
                Debug.LogError($"[RemoteSettings] Listen failed: {listenerTask.Exception}");
            }
        });
    }

    private void OnDestroy()
    {
        if (settingsListener != null)
        {
            settingsListener.Stop();
            settingsListener = null;
        }

        if (instance == this)
        {
            instance = null;
            isQuitting = true;
        }
    }
}

