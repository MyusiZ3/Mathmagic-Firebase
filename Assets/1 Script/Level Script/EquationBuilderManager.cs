using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EquationBuilderManager : MonoBehaviour
{
    public enum SlotType
    {
        Any,
        NumberOnly,     // Hanya menerima angka
        OperatorOnly    // Hanya menerima operator (+, -, x, =, dll)
    }

    [System.Serializable]
    public class SlotData
    {
        [Tooltip("Tombol slot di atas.")]
        public Button slotButton;

        [Tooltip("Jenis input yang diperbolehkan masuk ke slot ini.")]
        public SlotType slotType = SlotType.Any;
    }

    [System.Serializable]
    public class CorrectSequence
    {
        [Tooltip("Kemungkinan susunan teks jawaban yang benar dari kiri ke kanan (misal: 4, +, 5).")]
        public List<string> sequence = new List<string>();
    }

    [Header("Slots Setup (Top)")]
    [Tooltip("Daftar slot kosong di atas beserta aturannya.")]
    public List<SlotData> slots = new List<SlotData>();

    [Header("Options Setup (Bottom)")]
    [Tooltip("Daftar tombol pilihan (kartu angka/simbol) di bagian bawah.")]
    public List<Button> optionButtons = new List<Button>();

    [Header("Answer Verification")]
    [Tooltip("Daftar semua kemungkinan urutan jawaban yang dianggap benar (misal: 4+5 atau 5+4).")]
    public List<CorrectSequence> correctSequences = new List<CorrectSequence>();

    [Header("UI References")]
    [Tooltip("Tombol Check untuk memeriksa persamaan.")]
    public Button checkButton;

    [Header("Score Settings")]
    [Tooltip("Jumlah skor yang ditambahkan saat jawaban benar.")]
    public int scoreReward = 15;

    [Tooltip("Jumlah skor yang dikurangi saat jawaban salah.")]
    public int scorePenalty = 5;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Warna slot ketika kosong.")]
    public Color emptySlotColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

    [Tooltip("Warna slot ketika terisi.")]
    public Color filledSlotColor = Color.white;

    [Header("Audio Settings")]
    [Tooltip("AudioSource untuk memutar efek suara. Jika kosong, akan mencari otomatis.")]
    public AudioSource audioSource;

    [Tooltip("Suara saat memindahkan kartu (ke atas atau ke bawah).")]
    public AudioClip cardMoveSound;

    // Menyimpan tombol opsi bawah mana yang sedang mengisi slot atas ke-i
    private Button[] slotAssignments;
    private OverlayAnswer overlayAnswer;

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
        slotAssignments = new Button[slots.Count];

        // Inisialisasi slot atas
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i; // Salin variabel untuk closure
            if (slots[i].slotButton != null)
            {
                slots[i].slotButton.onClick.AddListener(() => OnSlotClicked(index));
                
                // Set teks awal slot menjadi kosong
                var textComp = slots[i].slotButton.GetComponentInChildren<TMP_Text>();
                if (textComp != null) textComp.text = "";
                
                slots[i].slotButton.image.color = emptySlotColor;
            }
        }

        // Inisialisasi opsi bawah
        foreach (var option in optionButtons)
        {
            if (option != null)
            {
                option.onClick.AddListener(() => OnOptionClicked(option));
            }
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckEquation);
        }

        // Perbarui tombol check di awal
        ValidateCheckButtonState();
    }

    /// <summary>
    /// Ketika tombol opsi di bawah diklik.
    /// </summary>
    private void OnOptionClicked(Button clickedOption)
    {
        var optionText = clickedOption.GetComponentInChildren<TMP_Text>();
        if (optionText == null) return;

        string val = optionText.text;
        bool isOperator = IsOperatorString(val);

        // Cari slot kosong pertama di atas yang tipenya sesuai
        int matchingSlotIndex = FindFirstMatchingEmptySlot(isOperator);

        if (matchingSlotIndex != -1)
        {
            PlaySound(cardMoveSound);

            // Pasang ke slot tersebut
            slotAssignments[matchingSlotIndex] = clickedOption;

            // Salin teks opsi bawah ke slot atas
            var slotText = slots[matchingSlotIndex].slotButton.GetComponentInChildren<TMP_Text>();
            if (slotText != null)
            {
                slotText.text = val;
            }

            // Ubah visual slot atas menjadi terisi
            slots[matchingSlotIndex].slotButton.image.color = filledSlotColor;

            // Sembunyikan tombol opsi bawah yang sudah dipakai
            clickedOption.gameObject.SetActive(false);

            ValidateCheckButtonState();
        }
    }

    /// <summary>
    /// Ketika slot di atas diklik (mengembalikan kartu ke bawah).
    /// </summary>
    private void OnSlotClicked(int slotIndex)
    {
        // Pastikan slot tersebut memiliki kartu yang terisi
        if (slotAssignments[slotIndex] != null)
        {
            PlaySound(cardMoveSound);

            // Aktifkan kembali tombol opsi bawah
            slotAssignments[slotIndex].gameObject.SetActive(true);

            // Kosongkan slot atas
            var slotText = slots[slotIndex].slotButton.GetComponentInChildren<TMP_Text>();
            if (slotText != null)
            {
                slotText.text = "";
            }

            slots[slotIndex].slotButton.image.color = emptySlotColor;

            // Hapus referensi penugasan
            slotAssignments[slotIndex] = null;

            ValidateCheckButtonState();
        }
    }

    /// <summary>
    /// Mengecek apakah sebuah string adalah operator matematika.
    /// </summary>
    private bool IsOperatorString(string text)
    {
        text = text.Trim();
        return text == "+" || text == "-" || text == "x" || text == "*" || text == "/" || text == "=" || text == ":" || text == "÷";
    }

    /// <summary>
    /// Mencari indeks slot kosong yang sesuai dengan jenis kartu.
    /// </summary>
    private int FindFirstMatchingEmptySlot(bool isOperator)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slotAssignments[i] == null)
            {
                SlotType type = slots[i].slotType;
                if (type == SlotType.Any) return i;
                if (isOperator && type == SlotType.OperatorOnly) return i;
                if (!isOperator && type == SlotType.NumberOnly) return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Tombol Check hanya aktif jika semua slot atas sudah terisi.
    /// </summary>
    private void ValidateCheckButtonState()
    {
        if (checkButton != null)
        {
            bool allFilled = true;
            for (int i = 0; i < slotAssignments.Length; i++)
            {
                if (slotAssignments[i] == null)
                {
                    allFilled = false;
                    break;
                }
            }
            checkButton.interactable = allFilled;
        }
    }

    /// <summary>
    /// Memeriksa apakah susunan kartu di slot atas sudah sesuai dengan salah satu correctSequences.
    /// </summary>
    public void CheckEquation()
    {
        // Ambil urutan teks saat ini di slot atas
        List<string> currentSequence = new List<string>();
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotButton == null) continue;
            var slotText = slots[i].slotButton.GetComponentInChildren<TMP_Text>();
            currentSequence.Add((slotText != null) ? slotText.text.Trim() : "");
        }

        bool matchFound = false;

        // Bandingkan dengan setiap kemungkinan kunci jawaban yang benar
        foreach (var correctSeq in correctSequences)
        {
            if (correctSeq.sequence.Count != currentSequence.Count) continue;

            bool isMatch = true;
            for (int i = 0; i < currentSequence.Count; i++)
            {
                if (currentSequence[i] != correctSeq.sequence[i])
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                matchFound = true;
                break;
            }
        }

        if (overlayAnswer != null)
        {
            if (matchFound)
            {
                overlayAnswer.ShowCorrectOverlay(scoreReward);
            }
            else
            {
                overlayAnswer.ShowWrongOverlay(scorePenalty);
                // Jika salah, kembalikan semua kartu ke bawah agar pemain bisa mencoba kembali
                ResetAllSlots();
            }
        }
        else
        {
            Debug.LogWarning("[EquationBuilderManager] OverlayAnswer tidak ditemukan di scene!");
        }
    }

    /// <summary>
    /// Mengembalikan semua kartu di slot atas kembali ke pilihan bawah.
    /// </summary>
    public void ResetAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slotAssignments[i] != null)
            {
                slotAssignments[i].gameObject.SetActive(true);
                slotAssignments[i] = null;
            }

            if (slots[i].slotButton != null)
            {
                var slotText = slots[i].slotButton.GetComponentInChildren<TMP_Text>();
                if (slotText != null) slotText.text = "";
                slots[i].slotButton.image.color = emptySlotColor;
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
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot != null && slot.slotButton != null) slot.slotButton.onClick.RemoveAllListeners();
            }
        }
        if (optionButtons != null)
        {
            foreach (var option in optionButtons)
            {
                if (option != null) option.onClick.RemoveAllListeners();
            }
        }
        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(CheckEquation);
        }
    }
}
