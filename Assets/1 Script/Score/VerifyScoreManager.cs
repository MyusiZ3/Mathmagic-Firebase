using UnityEngine;

public class VerifyScoreManager : MonoBehaviour
{
    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            Debug.Log("ScoreManager.Instance is available.");
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is NOT available!");
        }
    }
}
