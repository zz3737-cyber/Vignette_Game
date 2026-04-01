using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class TeacherController : MonoBehaviour
{
    [Header("Patrol")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float moveSpeed = 2f;
    public float reachThreshold = 0.05f;

    [Header("Detection")]
    public StudentController[] students;
    public Vector2 detectBoxSize = new Vector2(3f, 2f);
    public float detectOffsetX = 1.8f;
    public float detectOffsetY = -2.0f;

    [Header("Detection Visual")]
    public Transform detectVisual;
    public Vector3 detectVisualRightLocalPos = new Vector3(1.8f, -2f, 0f);
    public Vector3 detectVisualLeftLocalPos = new Vector3(-1.8f, -2f, 0f);
    public bool flipDetectVisual = true;

    [Header("Turn Back")]
    public float minTurnInterval = 2f;
    public float maxTurnInterval = 5f;
    public float turnChance = 0.35f;
    public float stopDuration = 1f;
    public float turnBackDuration = 1f;

    [Header("Caught Sequence")]
    public Vector2 caughtOffset = new Vector2(0f, 2.0f);
    public float caughtPoseDuration = 1f;

    [Header("Sprites")]
    public Sprite patrolSprite;
    public Sprite turnBackSprite;
    public Sprite caughtSprite;

    private GameManager gameManager;
    private SpriteRenderer sr;

    private bool movingRight = true;
    private bool detectFacingRight = true;
    private bool isPaused = false;
    private bool isCaughtSequence = false;

    private Coroutine turnRoutine;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null && patrolSprite != null)
            sr.sprite = patrolSprite;

        detectFacingRight = movingRight;
        UpdateSpriteFacing();
        UpdateDetectVisual();

        turnRoutine = StartCoroutine(RandomTurnLoop());
    }

    void Update()
    {
        if (gameManager == null) return;

        // 每帧都更新，这样你运行时调位置也会立刻生效
        UpdateDetectVisual();

        if (gameManager.gameEnded) return;
        if (isPaused) return;
        if (isCaughtSequence) return;

        Patrol();
        DetectStudents();
    }

    void UpdateSpriteFacing()
    {
        if (sr == null) return;

        // 如果方向反了，就改成 sr.flipX = movingRight;
        sr.flipX = !movingRight;
    }

    void UpdateDetectVisual()
    {
        if (detectVisual == null) return;

        // 直接用左右两套独立位置
        detectVisual.localPosition = detectFacingRight
            ? detectVisualRightLocalPos
            : detectVisualLeftLocalPos;

        if (flipDetectVisual)
        {
            Vector3 scale = detectVisual.localScale;
            scale.x = detectFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            detectVisual.localScale = scale;
        }
    }

    void Patrol()
    {
        if (leftPoint == null || rightPoint == null) return;

        Transform target = movingRight ? rightPoint : leftPoint;
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        UpdateSpriteFacing();

        if (Vector3.Distance(transform.position, target.position) <= reachThreshold)
        {
            movingRight = !movingRight;
            detectFacingRight = movingRight;

            UpdateSpriteFacing();
            UpdateDetectVisual();
        }
    }

    IEnumerator RandomTurnLoop()
    {
        while (true)
        {
            float wait = Random.Range(minTurnInterval, maxTurnInterval);
            yield return new WaitForSeconds(wait);

            if (gameManager == null || gameManager.gameEnded) continue;
            if (isPaused || isCaughtSequence) continue;

            if (Random.value <= turnChance)
            {
                yield return StartCoroutine(TurnBackRoutine());
            }
        }
    }

    IEnumerator TurnBackRoutine()
    {
        isPaused = true;

        if (sr != null && patrolSprite != null)
            sr.sprite = patrolSprite;

        UpdateSpriteFacing();
        UpdateDetectVisual();

        yield return new WaitForSeconds(stopDuration);

        movingRight = !movingRight;
        detectFacingRight = movingRight;

        UpdateSpriteFacing();
        UpdateDetectVisual();

        if (sr != null && turnBackSprite != null)
            sr.sprite = turnBackSprite;

        yield return new WaitForSeconds(turnBackDuration);

        if (sr != null && patrolSprite != null)
            sr.sprite = patrolSprite;

        UpdateSpriteFacing();
        UpdateDetectVisual();

        isPaused = false;
    }

    void DetectStudents()
    {
        Vector2 boxCenter = GetDetectBoxCenter();

        foreach (StudentController student in students)
        {
            if (student == null || student.isCaught || student.isFinished) continue;

            Vector2 studentPos = student.transform.position;

            bool insideX = Mathf.Abs(studentPos.x - boxCenter.x) <= detectBoxSize.x * 0.5f;
            bool insideY = Mathf.Abs(studentPos.y - boxCenter.y) <= detectBoxSize.y * 0.5f;

            if (insideX && insideY && student.IsPressingOwnKey())
            {
                gameManager.TriggerGameOver(student);
                return;
            }
        }
    }

    Vector2 GetDetectBoxCenter()
    {
        float offsetX = detectFacingRight ? detectOffsetX : -detectOffsetX;
        return new Vector2(transform.position.x + offsetX, transform.position.y + detectOffsetY);
    }

    public IEnumerator PlayCaughtSequence(StudentController caughtStudent)
    {
        if (caughtStudent == null) yield break;

        isCaughtSequence = true;
        isPaused = true;

        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }

        Vector3 targetPos = caughtStudent.transform.position + (Vector3)caughtOffset;
        transform.position = targetPos;

        movingRight = caughtStudent.transform.position.x > transform.position.x;
        detectFacingRight = movingRight;

        UpdateSpriteFacing();
        UpdateDetectVisual();

        if (sr != null && caughtSprite != null)
            sr.sprite = caughtSprite;

        yield return new WaitForSecondsRealtime(caughtPoseDuration);
    }

    public void ResetTeacher()
    {
        isPaused = false;
        isCaughtSequence = false;
        movingRight = true;
        detectFacingRight = movingRight;

        if (sr != null && patrolSprite != null)
            sr.sprite = patrolSprite;

        UpdateSpriteFacing();
        UpdateDetectVisual();

        if (turnRoutine == null)
            turnRoutine = StartCoroutine(RandomTurnLoop());
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float offsetX = detectFacingRight ? detectOffsetX : -detectOffsetX;
        Vector3 boxCenter = new Vector3(
            transform.position.x + offsetX,
            transform.position.y + detectOffsetY,
            0f
        );

        Gizmos.DrawWireCube(boxCenter, new Vector3(detectBoxSize.x, detectBoxSize.y, 0f));
    }
}