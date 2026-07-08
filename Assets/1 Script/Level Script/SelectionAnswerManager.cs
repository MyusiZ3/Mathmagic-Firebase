using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectionAnswerManager : MonoBehaviour
{
    [System.Serializable]
    public class SelectionOption
    {
        [Tooltip("Tombol pilihan jawaban.")]
        public Button optionButton;

        [Tooltip("Apakah pilihan ini adalah jawaban yang benar?")]
        public bool isCorrect;
    }

    [Header("UI References")]
    [Tooltip("Daftar pilihan jawaban yang bisa dipilih oleh pemain.")]
    public List<SelectionOption> options = new List<SelectionOption>();

    [Tooltip("Tombol Check untuk memeriksa jawaban.")]
    public Button checkButton;

    [Header("Score Settings")]
    [Tooltip("Jumlah skor yang didapatkan jika jawaban benar.")]
    public int scoreReward = 10;

    [Tooltip("Jumlah skor yang dikurangi jika jawaban salah.")]
    public int scorePenalty = 5;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Warna tombol saat tidak dipilih.")]
    public Color normalColor = Color.white;

    [Tooltip("Warna tombol saat dipilih.")]
    public Color selectedColor = new Color(0.8f, 0.9f, 1f, 1f); // HSL Sleek Blue/Teal soft

    private SelectionOption selectedOption = null;
    private OverlayAnswer overlayAnswer;

    private void Awake()
    {
        overlayAnswer = FindFirstObjectByType<OverlayAnswer>();
    }

    private void Start()
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning($"[SelectionAnswerManager] Daftar 'options' masih kosong pada {gameObject.name}!");
            return;
        }

        // Daftarkan listener klik ke setiap tombol pilihan
        foreach (var option in options)
        {
            if (option.optionButton != null)
            {
                // Reset warna awal ke normalColor
                option.optionButton.image.color = normalColor;
                
                option.optionButton.onClick.AddListener(() => SelectOption(option));
            }
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckSelectedAnswer);
            // Validasi awal tombol Check (harus pilih salah satu dulu)
            ValidateCheckButtonState();
        }
    }

    /// <summary>
    /// Memilih salah satu opsi dan mengubah visual tombolnya.
    /// </summary>
    private void SelectOption(SelectionOption targetOption)
    {
        selectedOption = targetOption;

        // Perbarui warna semua tombol agar hanya tombol terpilih yang berubah
        foreach (var option in options)
        {
            if (option.optionButton != null)
            {
                option.optionButton.image.color = (option == selectedOption) ? selectedColor : normalColor;
            }
        }

        ValidateCheckButtonState();
    }

    /// <summary>
    /// Memeriksa apakah tombol Check bisa diaktifkan (hanya aktif jika sudah memilih).
    /// </summary>
    private void ValidateCheckButtonState()
    {
        if (checkButton != null)
        {
            checkButton.interactable = (selectedOption != null);
        }
    }

    /// <summary>
    /// Memeriksa apakah opsi terpilih adalah jawaban yang benar.
    /// </summary>
    public void CheckSelectedAnswer()
    {
        if (selectedOption == null) return;

        if (overlayAnswer != null)
        {
            if (selectedOption.isCorrect)
            {
                overlayAnswer.ShowCorrectOverlay(scoreReward);
            }
            else
            {
                overlayAnswer.ShowWrongOverlay(scorePenalty);
            }
        }
        else
        {
            Debug.LogWarning("[SelectionAnswerManager] OverlayAnswer tidak ditemukan di scene!");
        }
    }

    private void OnDestroy()
    {
        if (options != null)
        {
            foreach (var option in options)
            {
                if (option.optionButton != null)
                {
                    option.optionButton.onClick.RemoveAllListeners();
                }
            }
        }
        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(CheckSelectedAnswer);
        }
    }
}
