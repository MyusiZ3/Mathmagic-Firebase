using UnityEngine;
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

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    [Header("Default Local Settings")]
    public int defaultMaxHealth = 10;
    public float defaultHealthCooldownSeconds = 300f;
    public float defaultQuestionTimerSeconds = 60f;
    public int defaultMainLevelReward = 100;
    public int defaultBonusLevelReward = 250;

    [Header("Current Remote Settings")]
    public int maxHealth;
    public float healthCooldownSeconds;
    public float questionTimerSeconds;
    public int mainLevelReward;
    public int bonusLevelReward;

    public bool IsLoaded { get; private set; } = false;
    public event Action OnSettingsLoaded;

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
        db = FirebaseFirestore.DefaultInstance;
        FetchRemoteSettings();
    }

    private void InitializeDefaults()
    {
        maxHealth = defaultMaxHealth;
        healthCooldownSeconds = defaultHealthCooldownSeconds;
        questionTimerSeconds = defaultQuestionTimerSeconds;
        mainLevelReward = defaultMainLevelReward;
        bonusLevelReward = defaultBonusLevelReward;
    }

    public void FetchRemoteSettings()
    {
        DocumentReference docRef = db.Collection("settings").Document("global");
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.ContainsField("max_health"))
                    maxHealth = snapshot.GetValue<int>("max_health");
                if (snapshot.ContainsField("health_cooldown_seconds"))
                    healthCooldownSeconds = snapshot.GetValue<float>("health_cooldown_seconds");
                if (snapshot.ContainsField("question_timer_seconds"))
                    questionTimerSeconds = snapshot.GetValue<float>("question_timer_seconds");
                if (snapshot.ContainsField("main_level_score_reward"))
                    mainLevelReward = snapshot.GetValue<int>("main_level_score_reward");
                if (snapshot.ContainsField("bonus_level_score_reward"))
                    bonusLevelReward = snapshot.GetValue<int>("bonus_level_score_reward");

                IsLoaded = true;
                Debug.Log($"[RemoteSettings] Settings loaded: max_health={maxHealth}, health_cooldown={healthCooldownSeconds}, timer={questionTimerSeconds}, main_reward={mainLevelReward}, bonus_reward={bonusLevelReward}");
                
                // Let systems like HealthManager/Timer know we updated
                OnSettingsLoaded?.Invoke();
            }
            else
            {
                Debug.LogWarning("[RemoteSettings] Failed to fetch settings or document doesn't exist, using defaults.");
            }
        });
    }
}
