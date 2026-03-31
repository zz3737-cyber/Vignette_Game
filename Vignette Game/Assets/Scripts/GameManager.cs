using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Students")]
    public StudentController[] students; // 顺序：0=1号, 1=2号, 2=3号, 3=4号

    [Header("Start UI")]
    public GameObject startPanel;

    [Header("Single Mode Panels")]
    public GameObject singleWinPanel;
    public GameObject singleLosePanel;

    [Header("Two Player Result Panels")]
    public GameObject player1WinPanel;
    public GameObject player2WinPanel;

    [Header("State")]
    public bool gameEnded = false;
    public bool gameStarted = false;
    public GameMode currentMode = GameMode.None;

    [Header("End Timing")]
    public float gameOverPauseDelay = 0.8f;

    // 用来跨场景重载时记住“是不是要自动再开一把”
    private static bool autoStartAfterReload = false;
    private static GameMode reloadMode = GameMode.None;

    void Start()
    {
        HideAllEndPanels();

        // 如果是 Play Again 触发的重载，就自动进入刚才的模式
        if (autoStartAfterReload && reloadMode != GameMode.None)
        {
            currentMode = reloadMode;
            autoStartAfterReload = false;
            reloadMode = GameMode.None;

            if (startPanel != null) startPanel.SetActive(false);

            gameStarted = true;
            gameEnded = false;
            Time.timeScale = 1f;
        }
        else
        {
            currentMode = GameMode.None;
            gameStarted = false;
            gameEnded = false;

            if (startPanel != null) startPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    void HideAllEndPanels()
    {
        if (singleWinPanel != null) singleWinPanel.SetActive(false);
        if (singleLosePanel != null) singleLosePanel.SetActive(false);
        if (player1WinPanel != null) player1WinPanel.SetActive(false);
        if (player2WinPanel != null) player2WinPanel.SetActive(false);
    }

    public void StartSinglePlayerMode()
    {
        currentMode = GameMode.SinglePlayer;
        StartGame();
    }

    public void StartTwoPlayerMode()
    {
        currentMode = GameMode.TwoPlayer;
        StartGame();
    }

    void StartGame()
    {
        gameStarted = true;
        gameEnded = false;

        if (startPanel != null)
            startPanel.SetActive(false);

        HideAllEndPanels();
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
                s.GetCaught(true);
            else
                s.GetCaught(false);
        }

        StartCoroutine(GameOverSequence(caughtStudent));
    }

    IEnumerator GameOverSequence(StudentController caughtStudent)
    {
        yield return new WaitForSecondsRealtime(gameOverPauseDelay);

        if (currentMode == GameMode.SinglePlayer)
        {
            ShowSingleLosePanel();
        }
        else if (currentMode == GameMode.TwoPlayer)
        {
            ShowTwoPlayerCaughtResult(caughtStudent);
        }

        Time.timeScale = 0f;
    }

    public void CheckAllFinished()
    {
        if (gameEnded) return;

        if (currentMode == GameMode.SinglePlayer)
        {
            CheckSinglePlayerFinish();
        }
        else if (currentMode == GameMode.TwoPlayer)
        {
            CheckTwoPlayerFinish();
        }
    }

    void CheckSinglePlayerFinish()
    {
        foreach (StudentController s in students)
        {
            if (s != null && s.gameObject.activeSelf && !s.isFinished)
                return;
        }

        gameEnded = true;
        ShowSingleWinPanel();
        Time.timeScale = 0f;
    }

    void CheckTwoPlayerFinish()
    {
        bool player1Finished = IsPlayer1GroupFinished();
        bool player2Finished = IsPlayer2GroupFinished();

        if (player1Finished || player2Finished)
        {
            gameEnded = true;

            if (player1Finished && !player2Finished)
                ShowPlayer1WinPanel();
            else if (player2Finished && !player1Finished)
                ShowPlayer2WinPanel();
            else
                ShowPlayer1WinPanel(); // 同帧都完成时，先默认玩家1胜

            Time.timeScale = 0f;
        }
    }

    bool IsPlayer1GroupFinished()
    {
        return students.Length >= 2 &&
               students[0] != null && students[1] != null &&
               students[0].isFinished && students[1].isFinished;
    }

    bool IsPlayer2GroupFinished()
    {
        return students.Length >= 4 &&
               students[2] != null && students[3] != null &&
               students[2].isFinished && students[3].isFinished;
    }

    void ShowTwoPlayerCaughtResult(StudentController caughtStudent)
    {
        if (caughtStudent == null)
        {
            ShowPlayer1WinPanel();
            return;
        }

        int id = caughtStudent.studentID;

        // 1/2 被抓 -> 玩家2赢
        if (id == 1 || id == 2)
        {
            ShowPlayer2WinPanel();
        }
        // 3/4 被抓 -> 玩家1赢
        else if (id == 3 || id == 4)
        {
            ShowPlayer1WinPanel();
        }
    }

    void ShowSingleWinPanel()
    {
        HideAllEndPanels();
        if (singleWinPanel != null) singleWinPanel.SetActive(true);
    }

    void ShowSingleLosePanel()
    {
        HideAllEndPanels();
        if (singleLosePanel != null) singleLosePanel.SetActive(true);
    }

    void ShowPlayer1WinPanel()
    {
        HideAllEndPanels();
        if (player1WinPanel != null) player1WinPanel.SetActive(true);
    }

    void ShowPlayer2WinPanel()
    {
        HideAllEndPanels();
        if (player2WinPanel != null) player2WinPanel.SetActive(true);
    }

    // Play Again：重新开始当前模式
    public void PlayAgain()
    {
        Time.timeScale = 1f;

        autoStartAfterReload = true;
        reloadMode = currentMode;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Exit：回到开始页面
    public void ExitToStartMenu()
    {
        Time.timeScale = 1f;

        autoStartAfterReload = false;
        reloadMode = GameMode.None;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}