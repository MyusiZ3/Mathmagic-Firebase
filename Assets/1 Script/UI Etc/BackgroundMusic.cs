using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // Cek apakah sudah ada instance yang sama, jika iya, hancurkan yang baru
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // Simpan instance dan pastikan objek ini tidak hancur saat pindah scene
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
