using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MatchingPairsManager : MonoBehaviour
{
    [System.Serializable]
    public class MatchPair
    {
        [Tooltip("Tombol di kolom kiri (Pertanyaan/Soal).")]
        public Button leftButton;

        [Tooltip("Tombol di kolom kanan (Jawaban yang cocok).")]
        public Button rightButton;
    }

    [Header("Pairs Configuration")]
    [Tooltip("Daftar pasangan tombol yang benar (Kiri cocok dengan Kanan).")]
    public List<MatchPair> pairs = new List<MatchPair>();

    [Header("UI References")]
    [Tooltip("Tombol Check untuk memeriksa semua pasangan.")]
    public Button checkButton;

    [Header("Score Settings")]
    [Tooltip("Skor reward saat semua pasangan benar.")]
    public int scoreReward = 15;

    [Tooltip("Skor penalti jika ada pasangan yang salah.")]
    public int scorePenalty = 5;

    [Header("Visual Feedback Colors")]
    [Tooltip("Warna tombol saat tidak terpilih / normal.")]
    public Color normalColor = Color.white;

    [Tooltip("Warna tombol saat sedang dipilih (sebelum dipasangkan).")]
    public Color selectedColor = new Color(1f, 0.9f, 0.5f, 1f); // Kuning soft

    [Tooltip("Warna untuk masing-masing pasangan yang berhasil terhubung (misal: Biru, Ungu, Hijau, dll).")]
    public List<Color> connectionColors = new List<Color>()
    {
        new Color(0.3f, 0.7f, 1f, 1f), // Teal/Blue soft
        new Color(0.9f, 0.5f, 0.8f, 1f), // Pink/Purple soft
        new Color(1f, 0.6f, 0.4f, 1f), // Orange soft
        new Color(0.4f, 0.8f, 0.5f, 1f)  // Green soft
    };

    [Header("Audio Settings")]
    [Tooltip("AudioSource untuk memutar efek suara klik. Jika kosong, akan mencari otomatis.")]
    public AudioSource audioSource;

    [Tooltip("Suara saat tombol pilihan diklik.")]
    public AudioClip clickSound;

    [Tooltip("Suara saat sepasang tombol berhasil dihubungkan.")]
    public AudioClip pairConnectedSound;

    // State pencocokan aktif
    private Button selectedLeft = null;
    private Button selectedRight = null;

    // Menyimpan hubungan pencocokan pemain: Key = LeftButton, Value = RightButton
    private Dictionary<Button, Button> playerConnections = new Dictionary<Button, Button>();
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
        if (pairs == null || pairs.Count == 0)
        {
            Debug.LogWarning($"[MatchingPairsManager] Daftar 'pairs' kosong pada {gameObject.name}!");
            return;
        }

        // Daftarkan listener klik untuk semua tombol di kolom kiri & kanan
        foreach (var pair in pairs)
        {
            if (pair.leftButton != null)
            {
                pair.leftButton.onClick.AddListener(() => OnLeftButtonClicked(pair.leftButton));
                pair.leftButton.image.color = normalColor;
            }
            if (pair.rightButton != null)
            {
                pair.rightButton.onClick.AddListener(() => OnRightButtonClicked(pair.rightButton));
                pair.rightButton.image.color = normalColor;
            }
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckAnswers);
        }

        // Acak urutan baris secara otomatis
        ShuffleRowPositions();

        // Perbarui visual awal
        UpdateVisuals();
        ValidateCheckButtonState();
    }

    /// <summary>
    /// Mengacak urutan baris tombol kiri dan kanan secara independen.
    /// </summary>
    private void ShuffleRowPositions()
    {
        List<Transform> leftTransforms = new List<Transform>();
        List<Transform> rightTransforms = new List<Transform>();

        foreach (var pair in pairs)
        {
            if (pair.leftButton != null && !leftTransforms.Contains(pair.leftButton.transform))
            {
                leftTransforms.Add(pair.leftButton.transform);
            }
            if (pair.rightButton != null && !rightTransforms.Contains(pair.rightButton.transform))
            {
                rightTransforms.Add(pair.rightButton.transform);
            }
        }

        ShuffleSiblingIndices(leftTransforms);
        ShuffleSiblingIndices(rightTransforms);
    }

    private void ShuffleSiblingIndices(List<Transform> transforms)
    {
        if (transforms.Count <= 1) return;

        // Fisher-Yates Shuffle
        for (int i = 0; i < transforms.Count; i++)
        {
            Transform temp = transforms[i];
            int randomIndex = Random.Range(i, transforms.Count);
            transforms[i] = transforms[randomIndex];
            transforms[randomIndex] = temp;
        }

        // Terapkan urutan acak dengan mengirim ke akhir daftar sibling
        foreach (var t in transforms)
        {
            t.SetAsLastSibling();
        }
    }

    private void OnLeftButtonClicked(Button button)
    {
        PlaySound(clickSound);

        // Jika tombol kiri ini sudah terhubung sebelumnya, hapus hubungannya
        if (playerConnections.ContainsKey(button))
        {
            playerConnections.Remove(button);
        }

        // Toggle seleksi
        if (selectedLeft == button)
        {
            selectedLeft = null;
        }
        else
        {
            selectedLeft = button;
        }

        UpdateVisuals();
        ValidateCheckButtonState();

        // Jika kiri dan kanan sudah terpilih, buat hubungannya
        if (selectedLeft != null && selectedRight != null)
        {
            LinkSelectedPair();
        }
    }

    private void OnRightButtonClicked(Button button)
    {
        PlaySound(clickSound);

        // Jika tombol kanan ini sudah terhubung ke tombol kiri mana pun, hapus hubungannya
        Button keyToRemove = null;
        foreach (var kvp in playerConnections)
        {
            if (kvp.Value == button)
            {
                keyToRemove = kvp.Key;
                break;
            }
        }
        if (keyToRemove != null)
        {
            playerConnections.Remove(keyToRemove);
        }

        // Toggle seleksi
        if (selectedRight == button)
        {
            selectedRight = null;
        }
        else
        {
            selectedRight = button;
        }

        UpdateVisuals();
        ValidateCheckButtonState();

        // Jika kiri dan kanan sudah terpilih, buat hubungannya
        if (selectedLeft != null && selectedRight != null)
        {
            LinkSelectedPair();
        }
    }

    private void LinkSelectedPair()
    {
        if (selectedLeft == null || selectedRight == null) return;

        // Simpan hubungan baru
        playerConnections[selectedLeft] = selectedRight;

        PlaySound(pairConnectedSound);

        // Reset seleksi aktif
        selectedLeft = null;
        selectedRight = null;

        UpdateVisuals();
        ValidateCheckButtonState();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.PlaySFX(audioSource, clip);
        }
    }

    /// <summary>
    /// Memperbarui warna visual tombol berdasarkan status seleksi dan pencocokan.
    /// </summary>
    private void UpdateVisuals()
    {
        // Reset semua ke warna normal terlebih dahulu
        foreach (var pair in pairs)
        {
            if (pair.leftButton != null) pair.leftButton.image.color = normalColor;
            if (pair.rightButton != null) pair.rightButton.image.color = normalColor;
        }

        // Beri warna khusus untuk setiap pasangan yang telah terhubung
        int colorIndex = 0;
        foreach (var kvp in playerConnections)
        {
            Color pairColor = connectionColors[colorIndex % connectionColors.Count];
            if (kvp.Key != null) kvp.Key.image.color = pairColor;
            if (kvp.Value != null) kvp.Value.image.color = pairColor;
            colorIndex++;
        }

        // Beri highlight untuk tombol yang baru diklik/sedang dipilih (tapi belum dipasangkan)
        if (selectedLeft != null)
        {
            selectedLeft.image.color = selectedColor;
        }
        if (selectedRight != null)
        {
            selectedRight.image.color = selectedColor;
        }
    }

    /// <summary>
    /// Tombol Check hanya aktif jika semua pasangan sudah dihubungkan oleh pemain.
    /// </summary>
    private void ValidateCheckButtonState()
    {
        if (checkButton != null)
        {
            // Aktif jika jumlah koneksi yang dibuat pemain sama dengan jumlah pasangan soal
            checkButton.interactable = (playerConnections.Count == pairs.Count);
        }
    }

    /// <summary>
    /// Memeriksa kebenaran semua pasangan yang dibuat pemain.
    /// </summary>
    public void CheckAnswers()
    {
        if (playerConnections.Count < pairs.Count) return;

        bool allCorrect = true;

        foreach (var correctPair in pairs)
        {
            if (correctPair.leftButton == null || correctPair.rightButton == null) continue;

            // Cari apakah pemain menghubungkan tombol kiri ini ke tombol kanan yang benar
            if (playerConnections.TryGetValue(correctPair.leftButton, out Button playerSelectedRight))
            {
                if (playerSelectedRight != correctPair.rightButton)
                {
                    allCorrect = false;
                    break;
                }
            }
            else
            {
                allCorrect = false;
                break;
            }
        }

        if (overlayAnswer != null)
        {
            if (allCorrect)
            {
                overlayAnswer.ShowCorrectOverlay(scoreReward);
            }
            else
            {
                overlayAnswer.ShowWrongOverlay(scorePenalty);
                // Jika salah, bersihkan semua koneksi agar pemain bisa mencoba kembali
                ResetConnections();
            }
        }
        else
        {
            Debug.LogWarning("[MatchingPairsManager] OverlayAnswer tidak ditemukan di scene!");
        }
    }

    /// <summary>
    /// Mereset semua pencocokan yang telah dibuat pemain.
    /// </summary>
    public void ResetConnections()
    {
        playerConnections.Clear();
        selectedLeft = null;
        selectedRight = null;
        UpdateVisuals();
        ValidateCheckButtonState();
    }

    private void OnDestroy()
    {
        if (pairs != null)
        {
            foreach (var pair in pairs)
            {
                if (pair.leftButton != null) pair.leftButton.onClick.RemoveAllListeners();
                if (pair.rightButton != null) pair.rightButton.onClick.RemoveAllListeners();
            }
        }
        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(CheckAnswers);
        }
    }
}
