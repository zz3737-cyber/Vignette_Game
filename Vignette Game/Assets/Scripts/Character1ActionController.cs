using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class MainWriterActionAnimator : MonoBehaviour
{
    public enum AnimState
    {
        Writing,
        SelfCheat,
        Helping
    }

    public enum InputKey
    {
        A,
        S,
        K,
        L
    }

    [Header("Input")]
    public InputKey selfKey = InputKey.A;
    public InputKey partnerKey = InputKey.S;

    [Header("Frames")]
    public Sprite[] writingFrames;
    public Sprite[] selfCheatFrames;
    public Sprite[] helpingFrames;

    [Header("End State Sprites")]
    public Sprite celebrateSprite;
    public Sprite sadSprite;

    [Header("Scale")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 celebrateScale = Vector3.one;
    public Vector3 sadScale = Vector3.one;

    [Header("Frame Rates")]
    public float writingFrameRate = 4f;
    public float actionFrameRate = 10f;

    [Header("References")]
    public StudentController studentController;

    private SpriteRenderer sr;
    private AnimState activeState = AnimState.Writing;
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isReversing = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (studentController == null)
            studentController = GetComponent<StudentController>();
    }

    void Start()
    {
        transform.localScale = normalScale;
        SetFirstWritingFrame();
    }

    void Update()
    {
        if (sr == null) return;

        // 只有真正进入结局表情时才锁住
        if (studentController != null)
        {
            if (studentController.showSad)
            {
                if (sadSprite != null) sr.sprite = sadSprite;
                transform.localScale = sadScale;
                return;
            }

            if (studentController.showCelebrate)
            {
                if (celebrateSprite != null) sr.sprite = celebrateSprite;
                transform.localScale = celebrateScale;
                return;
            }
        }

        transform.localScale = normalScale;

        bool selfPressed = IsKeyPressed(selfKey);
        bool partnerPressed = IsKeyPressed(partnerKey);

        AnimState targetState = AnimState.Writing;

        if (selfPressed && partnerPressed)
            targetState = AnimState.Helping;
        else if (selfPressed)
            targetState = AnimState.SelfCheat;
        else
            targetState = AnimState.Writing;

        UpdateState(targetState);
        UpdateAnimationPlayback();
    }

    bool IsKeyPressed(InputKey key)
    {
        if (Keyboard.current == null) return false;

        switch (key)
        {
            case InputKey.A: return Keyboard.current.aKey.isPressed;
            case InputKey.S: return Keyboard.current.sKey.isPressed;
            case InputKey.K: return Keyboard.current.kKey.isPressed;
            case InputKey.L: return Keyboard.current.lKey.isPressed;
            default: return false;
        }
    }

    void UpdateState(AnimState targetState)
    {
        if (targetState == AnimState.Writing)
        {
            isReversing = activeState != AnimState.Writing;
            return;
        }

        if (targetState != activeState)
        {
            activeState = targetState;
            currentFrame = 0;
            timer = 0f;
            isReversing = false;
            ApplyCurrentFrame();
            return;
        }

        isReversing = false;
    }

    void UpdateAnimationPlayback()
    {
        Sprite[] frames = GetCurrentFrames();
        if (frames == null || frames.Length == 0) return;

        float fps = activeState == AnimState.Writing ? writingFrameRate : actionFrameRate;
        float frameDuration = 1f / fps;

        timer += Time.deltaTime;
        if (timer < frameDuration) return;
        timer = 0f;

        if (activeState == AnimState.Writing)
        {
            currentFrame = (currentFrame + 1) % frames.Length;
            ApplyCurrentFrame();
            return;
        }

        if (!isReversing)
        {
            if (currentFrame < frames.Length - 1)
                currentFrame++;

            ApplyCurrentFrame();
        }
        else
        {
            if (currentFrame > 0)
            {
                currentFrame--;
                ApplyCurrentFrame();
            }
            else
            {
                activeState = AnimState.Writing;
                currentFrame = 0;
                timer = 0f;
                isReversing = false;
                ApplyCurrentFrame();
            }
        }
    }

    Sprite[] GetCurrentFrames()
    {
        switch (activeState)
        {
            case AnimState.Writing: return writingFrames;
            case AnimState.SelfCheat: return selfCheatFrames;
            case AnimState.Helping: return helpingFrames;
            default: return null;
        }
    }

    void ApplyCurrentFrame()
    {
        Sprite[] frames = GetCurrentFrames();
        if (frames == null || frames.Length == 0) return;

        currentFrame = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
        sr.sprite = frames[currentFrame];
    }

    void SetFirstWritingFrame()
    {
        activeState = AnimState.Writing;
        currentFrame = 0;
        timer = 0f;
        isReversing = false;
        ApplyCurrentFrame();
    }
}