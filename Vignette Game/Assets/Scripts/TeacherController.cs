using UnityEngine;

public class TeacherController : MonoBehaviour
{
    [Header("Movement")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float moveSpeed = 2f;

    [Header("Detection Box")]
    public StudentController[] students;
    public Vector2 detectBoxSize = new Vector2(3f, 2f);
    public float detectOffsetX = 1.8f;
    public float detectOffsetY = -2.0f;

    private Vector3 targetPos;
    private GameManager gameManager;
    private bool facingRight = true;

    private SpriteRenderer sr;
    private Vector3 originalScale;

    void Start()
    {
        targetPos = rightPoint.position;
        gameManager = FindObjectOfType<GameManager>();

        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (gameManager == null || gameManager.gameEnded) return;

        Patrol();
        UpdateFacingVisual();
        DetectStudents();
    }

    void Patrol()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (targetPos.x > transform.position.x)
            facingRight = true;
        else if (targetPos.x < transform.position.x)
            facingRight = false;

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (targetPos == rightPoint.position)
            {
                targetPos = leftPoint.position;
                facingRight = false;
            }
            else
            {
                targetPos = rightPoint.position;
                facingRight = true;
            }
        }
    }

    void UpdateFacingVisual()
    {
        if (sr == null) return;

        Vector3 scale = originalScale;

        if (facingRight)
            scale.x = Mathf.Abs(originalScale.x);
        else
            scale.x = -Mathf.Abs(originalScale.x);

        transform.localScale = scale;
    }

    void DetectStudents()
    {
        Vector2 boxCenter = GetDetectBoxCenter();

        foreach (StudentController student in students)
        {
            if (student == null || student.isCaught || student.isFinished) continue;
            if (!student.IsCheatingVisible()) continue;

            Vector2 studentPos = student.transform.position;

            bool insideX = Mathf.Abs(studentPos.x - boxCenter.x) <= detectBoxSize.x * 0.5f;
            bool insideY = Mathf.Abs(studentPos.y - boxCenter.y) <= detectBoxSize.y * 0.5f;

            if (insideX && insideY)
            {
                Debug.Log("Caught student " + student.studentID);
                gameManager.TriggerGameOver(student);
                return;
            }
        }
    }

    Vector2 GetDetectBoxCenter()
    {
        float offsetX = facingRight ? detectOffsetX : -detectOffsetX;
        return new Vector2(transform.position.x + offsetX, transform.position.y + detectOffsetY);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float offsetX = facingRight ? detectOffsetX : -detectOffsetX;
        Vector3 boxCenter = new Vector3(
            transform.position.x + offsetX,
            transform.position.y + detectOffsetY,
            0f
        );

        Gizmos.DrawWireCube(boxCenter, new Vector3(detectBoxSize.x, detectBoxSize.y, 0f));
    }
}