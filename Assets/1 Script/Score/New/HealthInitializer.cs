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
        CheckAndInitializeHealth();
    }

    private void CheckAndInitializeHealth()
    {
        DocumentReference userRef = firestore.Collection("users").Document(userId);
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
