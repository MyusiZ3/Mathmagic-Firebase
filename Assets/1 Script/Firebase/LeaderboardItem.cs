using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text usernameText;
    public TMP_Text scoreText;
    public Image profileImage;
    public Image backgroundImage;  // Referensi untuk background image

    public void SetData(int rank, string name, string username, int score, Sprite profileSprite, Color backgroundColor)
    {
        if (rankText != null)
            rankText.text = "#" + rank.ToString();

        if (nameText != null)
            nameText.text = name;

        if (usernameText != null)
            usernameText.text = "@" + username;

        if (scoreText != null)
            scoreText.text = score.ToString();

        if (profileImage != null && profileSprite != null)
        {
            profileImage.sprite = profileSprite;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;  // Mengubah warna background sesuai peringkat
        }
    }
}
