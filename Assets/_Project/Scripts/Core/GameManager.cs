using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("启动与关卡")]
    [SerializeField] private bool loadStartingSceneOnStart = false;
    [SerializeField] private string startingSceneName = "";

    [Header("玩家生成")]
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private string playerSpawnTag = "PlayerSpawn";
    [SerializeField] private bool autoSpawnPlayer = true;

    public bool IsPaused { get; private set; }
    public int Score { get; private set; }
    public PlayerController Player { get; private set; }

    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action<Scene> OnLevelLoaded;
    public event Action<PlayerController> OnPlayerSpawned;
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        if (loadStartingSceneOnStart && !string.IsNullOrWhiteSpace(startingSceneName))
        {
            SceneManager.LoadScene(startingSceneName);
            return;
        }

        StartGame();
    }

    public void StartGame()
    {
        OnGameStarted?.Invoke();
        if (autoSpawnPlayer)
        {
            SpawnOrFindPlayer();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupLevel(scene);
        OnLevelLoaded?.Invoke(scene);
    }

    private void SetupLevel(Scene scene)
    {
        Score = Score;
        if (autoSpawnPlayer)
        {
            SpawnOrFindPlayer();
        }
    }

    public void RegisterPlayer(PlayerController controller)
    {
        Player = controller;
        OnPlayerSpawned?.Invoke(Player);
    }

    private void SpawnOrFindPlayer()
    {
        var existing = FindObjectOfType<PlayerController>();
        if (existing != null)
        {
            RegisterPlayer(existing);
            return;
        }

        if (playerPrefab == null)
        {
            return;
        }

        Transform spawnPoint = null;
        var tagged = GameObject.FindGameObjectWithTag(playerSpawnTag);
        if (tagged != null)
        {
            spawnPoint = tagged.transform;
        }

        var instance = Instantiate(playerPrefab, spawnPoint != null ? spawnPoint.position : Vector3.zero, Quaternion.identity);
        RegisterPlayer(instance);
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    public void RestartLevel()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    public void LoadNextLevel()
    {
        var idx = SceneManager.GetActiveScene().buildIndex + 1;
        if (idx < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(idx);
        }
    }

    public void LoadLevelByName(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}