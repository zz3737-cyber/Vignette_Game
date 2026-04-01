using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StudentController : MonoBehaviour
{
    [Header("Basic Info")]
    public int studentID; // 1,2,3,4
    public StudentAction currentAction = StudentAction.Writing;

    [Header("Progress")]
    public float progress = 0f;
    public float maxProgress = 100f;
    public float writeSpeed = 10f;
    public float copySpeed = 14f;

    [Header("UI")]
    public Slider progressBar;

    [Header("Role")]
    public bool isMainWriter = false;   // 1 和 4
    public bool isCopyStudent = false;  // 2 和 3
    public StudentController partner;   // 2对应1，3对应4

    [Header("Logic State")]
    public bool isCaught = false;
    public bool isFinished = false;

    [Header("End Expression State")]
    public bool showCelebrate = false;
    public bool showSad = false;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (progressBar != null)
        {
            progressBar.maxValue = maxProgress;
            progressBar.value = progress;
        }
    }

    void Update()
    {
        if (gameManager == null || gameManager.gameEnded) return;
        if (isCaught || isFinished) return;

        HandleInputAndProgress();
        UpdateUI();
        CheckFinished();
    }

    void HandleInputAndProgress()
    {
        bool myKeyPressed = IsPressingOwnKey();
        bool partnerKeyPressed = partner != null && partner.IsPressingOwnKey();

        currentAction = StudentAction.Writing;

        // 1 和 4
        if (isMainWriter)
        {
            // 同时按：帮助，不涨进度
            if (myKeyPressed && partnerKeyPressed)
            {
                currentAction = StudentAction.Helping;
            }
            // 单独按：写字，涨进度
            else if (myKeyPressed)
            {
                currentAction = StudentAction.Writing;
                progress += writeSpeed * Time.deltaTime;
            }
            else
            {
                currentAction = StudentAction.Writing;
            }
        }

        // 2 和 3
        if (isCopyStudent)
        {
            // 同时按：抄，涨进度
            if (myKeyPressed && partnerKeyPressed)
            {
                currentAction = StudentAction.Copying;
                progress += copySpeed * Time.deltaTime;
            }
            // 单独按：抄动作，但不涨进度
            else if (myKeyPressed)
            {
                currentAction = StudentAction.Copying;
            }
            else
            {
                currentAction = StudentAction.Writing;
            }
        }

        progress = Mathf.Clamp(progress, 0, maxProgress);
    }

    public bool IsPressingOwnKey()
    {
        if (Keyboard.current == null) return false;

        switch (studentID)
        {
            case 1: return Keyboard.current.aKey.isPressed;
            case 2: return Keyboard.current.sKey.isPressed;
            case 3: return Keyboard.current.kKey.isPressed;
            case 4: return Keyboard.current.lKey.isPressed;
            default: return false;
        }
    }

    void UpdateUI()
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }

    void CheckFinished()
    {
        if (progress >= maxProgress)
        {
            isFinished = true;
            currentAction = StudentAction.Finished;
            gameManager.CheckAllFinished();
        }
    }

    public void GetCaught()
    {
        if (isCaught) return;

        isCaught = true;
        currentAction = StudentAction.Caught;
    }

    public void ShowCelebrate()
    {
        showCelebrate = true;
        showSad = false;
    }

    public void ShowSad()
    {
        showSad = true;
        showCelebrate = false;
    }

    public void ClearEndExpression()
    {
        showCelebrate = false;
        showSad = false;
    }
}