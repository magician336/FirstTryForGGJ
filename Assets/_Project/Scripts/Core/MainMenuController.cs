using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Scene Configuration")]
    [SerializeField] private string firstLevelName = "beforereset";

    public void StartGame()
    {
        Debug.Log("StartGame Clicked.");
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadScene(firstLevelName);
            Debug.Log("SceneManager.Instance.LoadScene(firstLevelName);");

        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelName);
        }
    }

    public void OpenOptions()
    {
        Debug.Log("Options Clicked.");
        optionsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        Debug.Log("Credits Clicked.");
        creditsPanel.SetActive(true);
    }

    public void OpenMain()
    {
        Debug.Log("Credits Clicked.");
        mainPanel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void OpenOption(GameObject optionPanel)
    {
        optionPanel.SetActive(true);
    }

    public void QuitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitToDesktop();
        }
        else
        {
            Application.Quit();
        }
    }
}
