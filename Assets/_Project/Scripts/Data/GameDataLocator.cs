using UnityEngine;

[DefaultExecutionOrder(-90)]
[DisallowMultipleComponent]
public class GameDataLocator : MonoBehaviour
{
    public static GameDataLocator Instance { get; private set; }

    [SerializeField] private GameDataRegistry registry;
    [SerializeField] private bool keepAcrossScenes = true;

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

    public bool TryGet<T>(DataKey key, out T value) where T : ScriptableObject
    {
        if (registry == null)
        {
            value = null;
            return false;
        }

        return registry.TryGet(key, out value);
    }

    public T Get<T>(DataKey key) where T : ScriptableObject
    {
        if (registry == null)
        {
            return null;
        }

        return registry.Get<T>(key);
    }
}
