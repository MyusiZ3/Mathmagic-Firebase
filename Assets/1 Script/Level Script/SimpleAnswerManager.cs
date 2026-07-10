using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleAnswerManager : MonoBehaviour
{
    [System.Serializable]
    public class SimpleOption
    {
        [Tooltip("Tombol pilihan jawaban (A, B, atau C).")]
        public Button optionButton;

        [Tooltip("Apakah pilihan ini adalah jawaban yang benar?")]
        public bool isCorrect;
    }

    [Header("Options Configuration")]
    [Tooltip("Daftar tombol pilihan jawaban dan status kebenarannya.")]
    public List<SimpleOption> options = new List<SimpleOption>();

    [Header("Score Settings")]
    [Tooltip("Jumlah skor yang ditambahkan saat jawaban benar.")]
    public int scoreReward = 10;

    [Tooltip("Jumlah skor yang dikurangi saat jawaban salah.")]
    public int scorePenalty = 5;

    private OverlayAnswer overlayAnswer;

    private void Awake()
    {
        overlayAnswer = FindFirstObjectByType<OverlayAnswer>();
    }

    private void Start()
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning($"[SimpleAnswerManager] Daftar 'options' masih kosong pada {gameObject.name}!");
            return;
        }

        // Daftarkan listener secara otomatis ke setiap tombol
        foreach (var option in options)
        {
            if (option.optionButton != null)
            {
                option.optionButton.onClick.AddListener(() => CheckAnswer(option.isCorrect));
            }
        }
    }

    public void CheckAnswer(bool isCorrect)
    {
        if (overlayAnswer != null)
        {
            if (isCorrect)
            {
                overlayAnswer.ShowCorrectOverlay(scoreReward);
            }
            else
            {
                overlayAnswer.ShowWrongOverlay(scorePenalty);

                // Cari AnswerShuffler untuk mengacak posisi tombol setelah salah
                AnswerShuffler shuffler = GetComponentInChildren<AnswerShuffler>();
                if (shuffler == null) shuffler = GetComponentInParent<AnswerShuffler>();
                if (shuffler != null)
                {
                    shuffler.Shuffle();
                }
            }
        }
        else
        {
            Debug.LogWarning("[SimpleAnswerManager] OverlayAnswer tidak ditemukan di scene!");
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
    }
}
