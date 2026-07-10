using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Header("Audio Settings")]
    [Tooltip("AudioSource untuk memutar efek suara. Jika kosong, akan mencari otomatis.")]
    public AudioSource audioSource;

    [Tooltip("Suara saat pemain mengklik/memilih opsi.")]
    public AudioClip selectSound;

    private SelectionOption selectedOption = null;
    private OverlayAnswer overlayAnswer;
    
    // Caching warna dan skala asli dari Inspector
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();
    private Dictionary<Button, Vector3> originalScales = new Dictionary<Button, Vector3>();

    private void Awake()
    {
        overlayAnswer = FindFirstObjectByType<OverlayAnswer>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = FindFirstObjectByType<AudioSource>();
            }
        }
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
                // Simpan warna asli dan skala asli tombol dari Inspector
                originalColors[option.optionButton] = option.optionButton.image.color;
                originalScales[option.optionButton] = option.optionButton.transform.localScale;
                
                option.optionButton.onClick.AddListener(() => SelectOption(option));
            }
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckSelectedAnswer);
        }

        // Atur status awal tombol Check dan visualnya (menggunakan skala asli)
        ResetSelection();
    }

    private void SelectOption(SelectionOption targetOption)
    {
        PlaySound(selectSound);

        // Jika mengklik kembali tombol yang sudah terpilih, lakukan deselect (batal pilih)
        if (selectedOption == targetOption)
        {
            ResetSelection();
            return;
        }

        selectedOption = targetOption;

        // Perbarui warna dan skala semua tombol agar lebih responsif dan premium
        foreach (var option in options)
        {
            if (option.optionButton != null)
            {
                Vector3 baseScale = originalScales.ContainsKey(option.optionButton) 
                    ? originalScales[option.optionButton] 
                    : Vector3.one;

                if (option == selectedOption)
                {
                    // Tombol terpilih: warna penuh dan skala sedikit membesar (1.05x dari skala asli)
                    option.optionButton.image.color = originalColors[option.optionButton];
                    option.optionButton.transform.localScale = baseScale * 1.05f;
                }
                else
                {
                    // Tombol tidak terpilih: buat sedikit transparan (opacity 50%) dan skala normal asli
                    Color c = originalColors[option.optionButton];
                    c.a = 0.5f;
                    option.optionButton.image.color = c;
                    option.optionButton.transform.localScale = baseScale;
                }
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
                // Jika salah, reset seleksi agar tombol Check tidak bisa ditekan lagi sebelum memilih ulang
                ResetSelection();
            }
        }
        else
        {
            Debug.LogWarning("[SelectionAnswerManager] OverlayAnswer tidak ditemukan di scene!");
        }
    }

    /// <summary>
    /// Mengembalikan semua pilihan ke status awal (tidak ada terpilih).
    /// </summary>
    public void ResetSelection()
    {
        selectedOption = null;
        foreach (var option in options)
        {
            if (option.optionButton != null)
            {
                if (originalColors.ContainsKey(option.optionButton))
                {
                    option.optionButton.image.color = originalColors[option.optionButton];
                }
                if (originalScales.ContainsKey(option.optionButton))
                {
                    option.optionButton.transform.localScale = originalScales[option.optionButton];
                }
            }
        }
        ValidateCheckButtonState();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
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
