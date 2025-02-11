using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    public Text nameText;       // Untuk Nama
    public Text usernameText;   // Untuk Username
    public Text scoreText;      // Untuk Skor
    public Image profileImage;  // Untuk Foto Profil

    public void SetData(string name, string username, int score, string profileImageName)
    {
        nameText.text = name;
        usernameText.text = "@" + username; // Tambahkan "@" agar lebih mirip username sosial media
        scoreText.text = score.ToString();

        // Load image dari Resources jika tersedia
        Sprite loadedSprite = Resources.Load<Sprite>("ProfileImages/" + profileImageName);
        if (loadedSprite != null)
        {
            profileImage.sprite = loadedSprite;
        }
        else
        {
            Debug.LogWarning($"Gambar profil '{profileImageName}' tidak ditemukan di Resources/ProfileImages!");
        }
    }
}
