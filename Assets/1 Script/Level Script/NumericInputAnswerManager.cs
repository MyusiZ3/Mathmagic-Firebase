using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NumericInputAnswerManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestionInputData
    {
        [Tooltip("Input field tempat pemain mengetik jawaban.")]
        public TMP_InputField answerInputField;

        [Tooltip("Jawaban angka yang benar untuk input ini.")]
        public int correctAnswer;
    }

    [Header("UI References")]
    [Tooltip("Daftar pertanyaan input dan jawaban benarnya (bisa 1, 2, atau lebih dalam satu level).")]
    public List<QuestionInputData> questions = new List<QuestionInputData>();

    [Tooltip("Tombol Check untuk memeriksa jawaban.")]
    public Button checkButton;

    [Header("Score Settings")]
    [Tooltip("Jumlah skor yang didapatkan jika jawaban benar.")]
    public int scoreReward = 10;

    [Tooltip("Jumlah skor yang dikurangi jika jawaban salah.")]
    public int scorePenalty = 5;

    private OverlayAnswer overlayAnswer;

    private void Awake()
    {
        overlayAnswer = FindFirstObjectByType<OverlayAnswer>();
    }

    private void Start()
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning($"[NumericInputAnswerManager] Daftar 'questions' masih kosong pada {gameObject.name}!");
            return;
        }

        // Konfigurasi semua input field dan daftarkan listener
        foreach (var question in questions)
        {
            if (question.answerInputField != null)
            {
                ConfigureInputFieldForNumbers(question.answerInputField);
                question.answerInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            }
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckAnswers);
            // Validasi awal tombol Check
            ValidateCheckButtonState();
        }

        // Fokuskan input field pertama secara otomatis
        if (questions[0].answerInputField != null)
        {
            questions[0].answerInputField.ActivateInputField();
        }
    }

    private void ConfigureInputFieldForNumbers(TMP_InputField inputField)
    {
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.keyboardType = TouchScreenKeyboardType.NumberPad;
        inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
    }

    private void OnInputFieldValueChanged(string value)
    {
        ValidateCheckButtonState();
    }

    /// <summary>
    /// Memeriksa apakah semua input field sudah terisi untuk mengaktifkan tombol Check.
    /// </summary>
    private void ValidateCheckButtonState()
    {
        if (checkButton == null) return;

        bool allFilled = true;
        foreach (var question in questions)
        {
            if (question.answerInputField == null || 
                string.IsNullOrEmpty(question.answerInputField.text) || 
                question.answerInputField.text == "-")
            {
                allFilled = false;
                break;
            }
        }

        checkButton.interactable = allFilled;
    }

    /// <summary>
    /// Memeriksa seluruh jawaban yang diinput oleh pemain.
    /// </summary>
    public void CheckAnswers()
    {
        if (questions == null || questions.Count == 0) return;

        bool allCorrect = true;

        foreach (var question in questions)
        {
            if (question.answerInputField == null) continue;

            if (int.TryParse(question.answerInputField.text, out int playerAnswer))
            {
                if (playerAnswer != question.correctAnswer)
                {
                    allCorrect = false;
                    // Kita bisa menambahkan feedback visual per input field di sini jika diinginkan
                    Debug.Log($"Jawaban salah pada input {question.answerInputField.name}. Terisi: {playerAnswer}, Seharusnya: {question.correctAnswer}");
                }
            }
            else
            {
                allCorrect = false;
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
            }
        }
        else
        {
            Debug.LogWarning("[NumericInputAnswerManager] OverlayAnswer tidak ditemukan di scene!");
        }
    }

    private void OnDestroy()
    {
        if (questions != null)
        {
            foreach (var question in questions)
            {
                if (question.answerInputField != null)
                {
                    question.answerInputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
                }
            }
        }
        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(CheckAnswers);
        }
    }
}
