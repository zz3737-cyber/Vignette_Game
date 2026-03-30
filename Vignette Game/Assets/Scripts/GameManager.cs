using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Students")]
    public StudentController[] students;

    [Header("UI")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("State")]
    public bool gameEnded = false;
    public bool gameStarted = false;

    [Header("End Timing")]
    public float gameOverPauseDelay = 0.8f;

    void Start()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        gameStarted = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void TriggerGameOver(StudentController caughtStudent)
    {
        if (gameEnded) return;

        gameEnded = true;

        foreach (StudentController s in students)
        {
            if (s == null) continue;

            if (s == caughtStudent)
                s.GetCaught(true);   // 被抓到的那个慢慢变红
            else
                s.GetCaught(false);  // 其他人结束，但不变红
        }

        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        yield return new WaitForSecondsRealtime(gameOverPauseDelay);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CheckAllFinished()
    {
        if (gameEnded) return;

        foreach (StudentController s in students)
        {
            if (s != null && !s.isFinished)
                return;
        }

        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}