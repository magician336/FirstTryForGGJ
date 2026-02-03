using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Gameplay";
    [SerializeField] private string settingsScene = "Settings";

    [Header("Options")]
    [SerializeField] private bool keepAcrossScenes = true;

    [Header("GameManager")]
    [SerializeField] private GameManager gameManager;

    public event Action<string> SceneLoadStarted;
    public event Action<string> SceneLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (keepAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadMainMenu() => LoadScene(mainMenuScene);
    public void LoadGameplay() => LoadScene(gameplayScene);
    public void LoadSettings() => LoadScene(settingsScene);

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrent() => LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

    public void LoadScene(SceneType type)
    {
        switch (type)
        {
            case SceneType.MainMenu:
                LoadMainMenu();
                break;
            case SceneType.Gameplay:
                LoadGameplay();
                break;
            case SceneType.Settings:
                LoadSettings();
                break;
        }
    }
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene name is empty. LoadScene aborted.");
            return;
        }

        gameManager.PlayerSettingsReset();

        SceneLoadStarted?.Invoke(sceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        SceneLoaded?.Invoke(sceneName);
    }
}

public enum SceneType
{
    MainMenu,
    LevelSelect,
    Gameplay,
    Settings
}
