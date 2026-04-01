using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundAutoFit : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Fit")]
    public bool fillScreen = true;     // true = 铺满并允许裁边
    public float extraScale = 1.05f;   // 额外再放大一点，避免边露出来

    [Header("Offset")]
    public Vector2 worldOffset = Vector2.zero; // 用来手动微调背景位置

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        FitNow();
    }

    void LateUpdate()
    {
        FitNow();
    }

    void FitNow()
    {
        if (targetCamera == null || sr == null || sr.sprite == null) return;

        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = camWidth / spriteSize.x;
        float scaleY = camHeight / spriteSize.y;

        float finalScale = fillScreen ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);
        finalScale *= extraScale;

        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        transform.position = new Vector3(
            targetCamera.transform.position.x + worldOffset.x,
            targetCamera.transform.position.y + worldOffset.y,
            0f
        );
    }
}