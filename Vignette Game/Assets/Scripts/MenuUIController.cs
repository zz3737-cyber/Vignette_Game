using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject howToPlayPanel;

    // 不要在 Start/Awake 里强行设置首页状态
    // 交给 GameManager 控制

    public void OpenHowToPlay()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    public void BackToStart()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
    }
}