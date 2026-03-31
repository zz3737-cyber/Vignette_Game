using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitCamera : MonoBehaviour
{
    public Camera targetCamera;
    public bool fillScreen = true; // true = 铺满并允许裁边，false = 完整显示但可能留边

    private SpriteRenderer sr;
    private float lastAspect = -1f;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        FitToCamera();
        SaveScreenState();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || targetCamera.aspect != lastAspect)
        {
            FitToCamera();
            SaveScreenState();
        }
    }

    void SaveScreenState()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastAspect = targetCamera != null ? targetCamera.aspect : -1f;
    }

    void FitToCamera()
    {
        if (targetCamera == null || sr == null || sr.sprite == null) return;

        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = camWidth / spriteSize.x;
        float scaleY = camHeight / spriteSize.y;

        float finalScale = fillScreen ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}