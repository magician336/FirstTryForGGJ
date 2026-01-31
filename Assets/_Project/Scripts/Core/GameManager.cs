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
    public event Action OnPlayerDead;
    public event Action OnPlayerRespawn;

    private string _nextSpawnTagOverride = null;
    private Vector3 currentRespawnPoint;

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
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        if (loadStartingSceneOnStart && !string.IsNullOrWhiteSpace(startingSceneName))
        {
            if (SceneManager.Instance != null)
            {
                SceneManager.Instance.LoadScene(startingSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(startingSceneName);
            }
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

    public void SetNextSpawnTag(string tag)
    {
        _nextSpawnTagOverride = tag;
    }

    public void SetRespawnPoint(Vector3 point)
    {
        currentRespawnPoint = point;
    }

    public void HandlePlayerDeath(HealthController health)
    {
        if (IsPaused) return; // Prevent double death if multiple sources hit same frame

        Debug.Log("Player Died!");
        OnPlayerDead?.Invoke();
        StartCoroutine(RespawnRoutine(health));
    }

    private System.Collections.IEnumerator RespawnRoutine(HealthController health)
    {
        // 1. Wait for death animation
        yield return new WaitForSeconds(1.5f);

        // 2. Respawn logic
        if (Player != null)
        {
            Player.Teleport(currentRespawnPoint);

            // Reset health
            if (health != null) health.ResetHealth();

            // Trigger revive state in PlayerController
            Player.Revive();

            OnPlayerRespawn?.Invoke();
        }
    }

    private void SpawnOrFindPlayer()
    {
        var existing = FindObjectOfType<PlayerController>();
        if (existing != null)
        {
            RegisterPlayer(existing);
            // Initialize respawn point to start position
            currentRespawnPoint = existing.transform.position;
            return;
        }

        if (playerPrefab == null)
        {
            return;
        }
        string currentTag = !string.IsNullOrEmpty(_nextSpawnTagOverride) ? _nextSpawnTagOverride : playerSpawnTag;

        Transform spawnPoint = null;
        var taggedNodes = GameObject.FindGameObjectsWithTag(currentTag);
        if (taggedNodes.Length > 0)
        {
            spawnPoint = taggedNodes[0].transform;
        }

        // 清除覆盖，以便下次使用默认值或重新设置
        _nextSpawnTagOverride = null;

        var instance = Instantiate(playerPrefab, spawnPoint != null ? spawnPoint.position : Vector3.zero, Quaternion.identity);
        RegisterPlayer(instance);
        // Initialize respawn point to spawn position
        currentRespawnPoint = instance.transform.position;
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
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.ReloadCurrent();
        }
        else
        {
            var idx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }

    public void LoadLevelByName(string sceneName)
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadScene(sceneName);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}