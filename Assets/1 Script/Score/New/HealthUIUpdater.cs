using UnityEngine;
using TMPro;

public class HealthUIUpdater : MonoBehaviour
{
    public TextMeshProUGUI healthText;

    private void Update()
    {
        if (HealthManager.Instance != null)
        {
            healthText.text = $"HP: {HealthManager.Instance.CurrentHealth}"; // Ubah akses
        }
    }
}
