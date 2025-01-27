using UnityEngine;

public enum GameState
{
    Playing,
    Paused,
    TimeOver,
    AnswerCorrect,
    AnswerWrong // Tambahan untuk jawaban salah
}

public class OverlayManager : MonoBehaviour
{
    public static OverlayManager Instance;

    [Header("Overlay Panels")]
    public GameObject pauseOverlay;
    public GameObject timeOverOverlay;
    public GameObject answerCorrectOverlay;
    public GameObject answerWrongOverlay; // Tambahan overlay salah

    private GameState currentState = GameState.Playing;

    void Awake()
    {
        // Implementasi Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameState(GameState newState)
    {
        Debug.Log("Setting Game State to: " + newState);
        // Sembunyikan semua overlay terlebih dahulu
        HideAllOverlays();

        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                if (pauseOverlay != null)
                {
                    pauseOverlay.SetActive(true);
                }
                Time.timeScale = 0f;
                break;

            case GameState.TimeOver:
                if (timeOverOverlay != null)
                {
                    timeOverOverlay.SetActive(true);
                }
                Time.timeScale = 0f;
                break;

            case GameState.AnswerCorrect:
                if (answerCorrectOverlay != null)
                {
                    answerCorrectOverlay.SetActive(true);
                }
                Time.timeScale = 0f;
                break;

            case GameState.AnswerWrong:
                if (answerWrongOverlay != null)
                {
                    answerWrongOverlay.SetActive(true);
                }
                Time.timeScale = 0f;
                break;
        }
    }

    private void HideAllOverlays()
    {
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (timeOverOverlay != null) timeOverOverlay.SetActive(false);
        if (answerCorrectOverlay != null) answerCorrectOverlay.SetActive(false);
        if (answerWrongOverlay != null) answerWrongOverlay.SetActive(false); // Sembunyikan overlay salah
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }
}
