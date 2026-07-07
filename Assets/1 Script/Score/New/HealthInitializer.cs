using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class HealthInitializer : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private string userId;

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
        CheckAndInitializeHealth();
    }

    private void CheckAndInitializeHealth()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID null atau kosong di HealthInitializer.");
            return;
        }

        string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);
        DocumentReference userRef = firestore.Collection("users").Document(shortId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Dictionary<string, object> updates = new Dictionary<string, object>();
                bool needsUpdate = false;

                if (!task.Result.ContainsField("Hp"))
                {
                    updates["Hp"] = 10;
                    needsUpdate = true;
                }

                if (!task.Result.ContainsField("LastHpUpdateTime"))
                {
                    updates["LastHpUpdateTime"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    userRef.UpdateAsync(updates);
                }
            }
        });
    }
}
