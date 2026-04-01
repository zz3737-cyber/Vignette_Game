using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Students")]
    public StudentController[] students; // 0=1号, 1=2号, 2=3号, 3=4号

    [Header("Teacher")]
    public TeacherController teacherController;

    [Header("Start UI")]
    public GameObject startPanel;

    [Header("Progress Bars")]
    public GameObject progressBarsGroup;

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

    private static bool autoStartAfterReload = false;
    private static GameMode reloadMode = GameMode.None;

    void Start()
    {
        HideAllEndPanels();

        if (autoStartAfterReload && reloadMode != GameMode.None)
        {
            currentMode = reloadMode;
            autoStartAfterReload = false;
            reloadMode = GameMode.None;

            if (startPanel != null) startPanel.SetActive(false);

            gameStarted = true;
            gameEnded = false;
            ClearAllEndExpressions();
            ShowProgressBars();

            if (teacherController != null)
                teacherController.ResetTeacher();

            Time.timeScale = 1f;
        }
        else
        {
            currentMode = GameMode.None;
            gameStarted = false;
            gameEnded = false;

            if (startPanel != null) startPanel.SetActive(true);

            ClearAllEndExpressions();
            HideProgressBars();

            if (teacherController != null)
                teacherController.ResetTeacher();

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

    void ClearAllEndExpressions()
    {
        if (students == null) return;

        foreach (var s in students)
        {
            if (s != null) s.ClearEndExpression();
        }
    }

    void HideProgressBars()
    {
        if (progressBarsGroup != null)
            progressBarsGroup.SetActive(false);
    }

    void ShowProgressBars()
    {
        if (progressBarsGroup != null)
            progressBarsGroup.SetActive(true);
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
        ClearAllEndExpressions();
        ShowProgressBars();

        if (teacherController != null)
            teacherController.ResetTeacher();

        Time.timeScale = 1f;
    }

    public void TriggerGameOver(StudentController caughtStudent)
    {
        if (gameEnded) return;

        gameEnded = true;

        if (currentMode == GameMode.SinglePlayer)
        {
            foreach (var s in students)
            {
                if (s != null) s.GetCaught();
            }
        }
        else if (currentMode == GameMode.TwoPlayer)
        {
            ApplyTwoPlayerCaughtLogic(caughtStudent);
        }

        StartCoroutine(GameOverSequence(caughtStudent));
    }

    void ApplyTwoPlayerCaughtLogic(StudentController caughtStudent)
    {
        if (caughtStudent == null) return;

        int id = caughtStudent.studentID;

        // 1/2组被抓 -> 只记录1/2为caught
        if (id == 1 || id == 2)
        {
            if (students.Length > 0 && students[0] != null) students[0].GetCaught();
            if (students.Length > 1 && students[1] != null) students[1].GetCaught();
        }
        // 3/4组被抓 -> 只记录3/4为caught
        else if (id == 3 || id == 4)
        {
            if (students.Length > 2 && students[2] != null) students[2].GetCaught();
            if (students.Length > 3 && students[3] != null) students[3].GetCaught();
        }
    }

    IEnumerator GameOverSequence(StudentController caughtStudent)
    {
        // 先让老师闪现到被抓学生前面，演抓到人的动作
        if (teacherController != null && caughtStudent != null)
        {
            yield return StartCoroutine(teacherController.PlayCaughtSequence(caughtStudent));
        }
        else
        {
            yield return new WaitForSecondsRealtime(gameOverPauseDelay);
        }

        ApplyEndExpressionsAfterResult();
        HideProgressBars();

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
        foreach (var s in students)
        {
            if (s != null && s.gameObject.activeSelf && !s.isFinished)
                return;
        }

        gameEnded = true;
        StartCoroutine(SingleWinSequence());
    }

    IEnumerator SingleWinSequence()
    {
        yield return new WaitForSecondsRealtime(gameOverPauseDelay);

        foreach (var s in students)
        {
            if (s != null) s.ShowCelebrate();
        }

        HideProgressBars();
        ShowSingleWinPanel();
        Time.timeScale = 0f;
    }

    void CheckTwoPlayerFinish()
    {
        bool player1Finished = IsPlayer1GroupFinished();
        bool player2Finished = IsPlayer2GroupFinished();

        if (!player1Finished && !player2Finished) return;

        gameEnded = true;
        StartCoroutine(TwoPlayerFinishSequence(player1Finished, player2Finished));
    }

    IEnumerator TwoPlayerFinishSequence(bool player1Finished, bool player2Finished)
    {
        yield return new WaitForSecondsRealtime(gameOverPauseDelay);

        if (player1Finished && !player2Finished)
        {
            students[0]?.ShowCelebrate();
            students[1]?.ShowCelebrate();
            students[2]?.ShowSad();
            students[3]?.ShowSad();
            HideProgressBars();
            ShowPlayer1WinPanel();
        }
        else if (player2Finished && !player1Finished)
        {
            students[2]?.ShowCelebrate();
            students[3]?.ShowCelebrate();
            students[0]?.ShowSad();
            students[1]?.ShowSad();
            HideProgressBars();
            ShowPlayer2WinPanel();
        }
        else
        {
            students[0]?.ShowCelebrate();
            students[1]?.ShowCelebrate();
            students[2]?.ShowCelebrate();
            students[3]?.ShowCelebrate();
            HideProgressBars();
            ShowPlayer1WinPanel();
        }

        Time.timeScale = 0f;
    }

    void ApplyEndExpressionsAfterResult()
    {
        if (currentMode == GameMode.SinglePlayer)
        {
            foreach (var s in students)
            {
                if (s != null) s.ShowSad();
            }
            return;
        }

        if (currentMode == GameMode.TwoPlayer)
        {
            bool leftLost =
                (students.Length > 0 && students[0] != null && students[0].isCaught) ||
                (students.Length > 1 && students[1] != null && students[1].isCaught);

            bool rightLost =
                (students.Length > 2 && students[2] != null && students[2].isCaught) ||
                (students.Length > 3 && students[3] != null && students[3].isCaught);

            if (leftLost)
            {
                students[0]?.ShowSad();
                students[1]?.ShowSad();
                students[2]?.ShowCelebrate();
                students[3]?.ShowCelebrate();
            }
            else if (rightLost)
            {
                students[2]?.ShowSad();
                students[3]?.ShowSad();
                students[0]?.ShowCelebrate();
                students[1]?.ShowCelebrate();
            }
        }
    }

    bool IsPlayer1GroupFinished()
    {
        return students.Length >= 2 &&
               students[0] != null && students[1] != null &&
               students[0].progress >= students[0].maxProgress &&
               students[1].progress >= students[1].maxProgress;
    }

    bool IsPlayer2GroupFinished()
    {
        return students.Length >= 4 &&
               students[2] != null && students[3] != null &&
               students[2].progress >= students[2].maxProgress &&
               students[3].progress >= students[3].maxProgress;
    }

    void ShowTwoPlayerCaughtResult(StudentController caughtStudent)
    {
        if (caughtStudent == null)
        {
            ShowPlayer1WinPanel();
            return;
        }

        int id = caughtStudent.studentID;

        if (id == 1 || id == 2)
            ShowPlayer2WinPanel();
        else if (id == 3 || id == 4)
            ShowPlayer1WinPanel();
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

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        autoStartAfterReload = true;
        reloadMode = currentMode;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToStartMenu()
    {
        Time.timeScale = 1f;
        autoStartAfterReload = false;
        reloadMode = GameMode.None;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}