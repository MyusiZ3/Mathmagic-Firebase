using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class BonusLevelButton : MonoBehaviour
{
    [Tooltip("ID unik level bonus ini (harus sama dengan nama scene level bonus, misal: lvl_2_bonus).")]
    public string bonusLevelId;

    [Tooltip("Main level minimal yang harus terbuka/aktif agar level bonus ini bisa dimainkan.")]
    public int requiresMainLevelUnlocked = 2;

    [Header("Visual States (Optional GameObject)")]
    [Tooltip("Tampilan visual ketika level bonus terkunci.")]
    public GameObject lockedVisual;

    [Tooltip("Tampilan visual ketika level bonus terbuka dan siap dimainkan (misal kado utuh).")]
    public GameObject activeVisual;

    [Tooltip("Tampilan visual ketika level bonus sudah selesai dimainkan (misal kado terbuka/centang).")]
    public GameObject completedVisual;

    [Header("Color States (Optional Color)")]
    [Tooltip("Jika di-centang, warna Image tombol akan berubah sesuai state.")]
    public bool useCustomColors = true;

    [Tooltip("Warna tombol ketika level bonus masih terkunci.")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Tooltip("Warna tombol ketika level bonus aktif/siap dimainkan.")]
    public Color activeColor = Color.white;

    [Tooltip("Warna tombol setelah level bonus selesai dimainkan.")]
    public Color completedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(PlayBonusLevel);
        }
    }

    /// <summary>
    /// Memperbarui interactable button, warna Image, dan visibilitas state visual berdasarkan data level terbaru.
    /// </summary>
    public void RefreshState(int currentMainLevel, HashSet<string> completedBonusLevels)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button == null)
        {
            Debug.LogError($"[BonusLevelButton] Button component tidak ditemukan di GameObject {gameObject.name}!");
            return;
        }

        bool isCompleted = completedBonusLevels != null && completedBonusLevels.Contains(bonusLevelId);
        
        bool isUnlocked = false;
        if (LevelManager.Instance != null)
        {
            isUnlocked = LevelManager.Instance.IsMainLevelUnlocked(requiresMainLevelUnlocked);
        }
        else
        {
            isUnlocked = currentMainLevel >= requiresMainLevelUnlocked;
        }

        Debug.Log($"[BonusLevelButton] RefreshState: {bonusLevelId} | isCompleted={isCompleted} | isUnlocked={isUnlocked} (requires={requiresMainLevelUnlocked}, currentMainLevel={currentMainLevel})");

        // Terapkan warna
        if (useCustomColors && button.image != null)
        {
            if (isCompleted)
            {
                button.image.color = completedColor;
            }
            else if (isUnlocked)
            {
                button.image.color = activeColor;
            }
            else
            {
                button.image.color = lockedColor;
            }
        }

        // Terapkan GameObject Visual
        if (isCompleted)
        {
            // Level bonus hanya bisa diselesaikan sekali, jadi nonaktifkan tombol
            button.interactable = false;

            if (lockedVisual != null) lockedVisual.SetActive(false);
            if (activeVisual != null) activeVisual.SetActive(false);
            if (completedVisual != null) completedVisual.SetActive(true);
        }
        else if (isUnlocked)
        {
            // Terbuka dan belum selesai
            button.interactable = true;

            if (lockedVisual != null) lockedVisual.SetActive(false);
            if (activeVisual != null) activeVisual.SetActive(true);
            if (completedVisual != null) completedVisual.SetActive(false);
        }
        else
        {
            // Terkunci
            button.interactable = false;

            if (lockedVisual != null) lockedVisual.SetActive(true);
            if (activeVisual != null) activeVisual.SetActive(false);
            if (completedVisual != null) completedVisual.SetActive(false);
        }
    }

    private void PlayBonusLevel()
    {
        // Gunakan efek transisi memudar atau loading delay jika ada di LevelManager
        if (LevelManager.Instance != null)
        {
            StartCoroutine(LoadBonusLevelWithDelay());
        }
        else
        {
            SceneManager.LoadScene(bonusLevelId);
        }
    }

    private System.Collections.IEnumerator LoadBonusLevelWithDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(bonusLevelId);
    }
}
