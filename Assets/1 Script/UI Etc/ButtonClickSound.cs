using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [Tooltip("Efek suara spesifik saat tombol ini diklik. Jika kosong, akan menggunakan default click sound dari AudioManager jika diset.")]
    public AudioClip clickSound;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        if (clickSound != null)
        {
            AudioManager.PlaySFX(clickSound);
        }
        else
        {
            AudioManager.PlayDefaultClickSound();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlaySound);
        }
    }
}
