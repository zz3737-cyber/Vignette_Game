using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class StudentController : MonoBehaviour
{
    [Header("Basic Info")]
    public int studentID; // 1,2,3,4
    public StudentAction currentAction = StudentAction.Idle;

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

    [Header("State")]
    public bool isCaught = false;
    public bool isFinished = false;

    [Header("Caught Color")]
    public float caughtColorFadeDuration = 0.8f;

    [Header("Sprites")]
    public Sprite normalSprite;   // 平时
    public Sprite cheatingSprite; // 只要按键就切这个

    private GameManager gameManager;
    private SpriteRenderer sr;
    private Coroutine fadeCoroutine;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        sr = GetComponent<SpriteRenderer>();
        if (sr != null && normalSprite != null)
        {
            sr.sprite = normalSprite;
        }

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
        UpdateVisualStyle();
        CheckFinished();
    }

    void HandleInputAndProgress()
    {
        bool myKeyPressed = IsPressingOwnKey();
        currentAction = StudentAction.Idle;

        if (isMainWriter)
        {
            if (myKeyPressed)
            {
                currentAction = StudentAction.Writing;
                progress += writeSpeed * Time.deltaTime;
            }
        }

        if (isCopyStudent)
        {
            if (myKeyPressed && partner != null && partner.IsPressingOwnKey())
            {
                currentAction = StudentAction.Copying;
                progress += copySpeed * Time.deltaTime;
            }
        }

        progress = Mathf.Clamp(progress, 0, maxProgress);
    }

    public bool IsPressingOwnKey()
    {
        if (Keyboard.current == null) return false;

        switch (studentID)
        {
            case 1:
                return Keyboard.current.digit1Key.isPressed || Keyboard.current.numpad1Key.isPressed;
            case 2:
                return Keyboard.current.digit2Key.isPressed || Keyboard.current.numpad2Key.isPressed;
            case 3:
                return Keyboard.current.digit3Key.isPressed || Keyboard.current.numpad3Key.isPressed;
            case 4:
                return Keyboard.current.digit4Key.isPressed || Keyboard.current.numpad4Key.isPressed;
            default:
                return false;
        }
    }

    void UpdateUI()
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }

    void UpdateVisualStyle()
    {
        if (sr == null) return;
        if (isCaught) return;

        bool myKeyPressed = IsPressingOwnKey();

        if (myKeyPressed)
        {
            if (cheatingSprite != null)
                sr.sprite = cheatingSprite;
        }
        else
        {
            if (normalSprite != null)
                sr.sprite = normalSprite;
        }
    }

    void CheckFinished()
    {
        if (progress >= maxProgress)
        {
            isFinished = true;
            currentAction = StudentAction.Finished;

            if (sr != null && normalSprite != null)
                sr.sprite = normalSprite;

            gameManager.CheckAllFinished();
        }
    }

    public bool IsCheatingVisible()
    {
        return currentAction == StudentAction.Writing || currentAction == StudentAction.Copying;
    }

    public void GetCaught(bool turnRed)
    {
        if (isCaught) return;

        isCaught = true;
        currentAction = StudentAction.Caught;

        if (turnRed && sr != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeToRed());
        }
    }

    IEnumerator FadeToRed()
    {
        float t = 0f;
        Color startColor = sr.color;
        Color targetColor = Color.red;

        while (t < caughtColorFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / caughtColorFadeDuration);
            sr.color = Color.Lerp(startColor, targetColor, normalized);
            yield return null;
        }

        sr.color = targetColor;
    }
}