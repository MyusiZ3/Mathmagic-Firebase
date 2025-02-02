using UnityEngine;

public class DontDestroyMusic : MonoBehaviour
{
    private static DontDestroyMusic instance;

    void Awake()
    {
        // Pastikan hanya ada satu instance dari Game Object ini
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Jangan destroy Game Object ini saat berpindah scene
        }
        else
        {
            Destroy(gameObject); // Hancurkan duplicate jika ada
        }
    }
}