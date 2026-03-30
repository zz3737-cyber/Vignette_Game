using UnityEngine;

public class UIFollowWorld : MonoBehaviour
{
    public Transform target;              // 要跟随的人物
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f); // 在人物头顶上方一点
    public Camera worldCamera;            // 主摄像机
    public RectTransform uiRectTransform; // 这个UI本身
    public Canvas canvas;                 // 所属Canvas

    void LateUpdate()
    {
        if (target == null || worldCamera == null || uiRectTransform == null || canvas == null)
            return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
        {
            uiRectTransform.gameObject.SetActive(false);
            return;
        }
        else
        {
            uiRectTransform.gameObject.SetActive(true);
        }

        Vector2 anchoredPos;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera,
            out anchoredPos))
        {
            uiRectTransform.anchoredPosition = anchoredPos;
        }
    }
}