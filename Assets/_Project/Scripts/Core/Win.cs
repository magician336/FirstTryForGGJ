using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    public static Win Instance { get; private set; }

    [Header("UI 引用")]
    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        Instance = this;
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 触发胜利逻辑
    /// </summary>
    public void Victory()
    {
        Debug.Log("<color=gold>【游戏胜利】玩家获得了终极面具 FinalMask！</color>");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        //   

    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Time.timeScale = 1f;

        // 假设主菜单场景名为 "MainMenu"
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevelByName("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void BackToMenu()
    {
    }
}