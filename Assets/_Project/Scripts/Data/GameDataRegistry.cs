using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/Game Data Registry", fileName = "GameDataRegistry")]
public class GameDataRegistry : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public DataKey key;
        public ScriptableObject data;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<DataKey, ScriptableObject> cache;

    public IReadOnlyList<Entry> Entries => entries;

    public bool TryGet<T>(DataKey key, out T value) where T : ScriptableObject
    {
        EnsureCache();
        if (key == null)
        {
            value = null;
            return false;
        }

        if (cache.TryGetValue(key, out var data) && data is T typed)
        {
            value = typed;
            return true;
        }

        value = null;
        return false;
    }

    public T Get<T>(DataKey key) where T : ScriptableObject
    {
        return TryGet<T>(key, out var value) ? value : null;
    }

    public void RebuildCache()
    {
        cache = null;
        EnsureCache();
    }

    private void EnsureCache()
    {
        if (cache != null)
        {
            return;
        }

        cache = new Dictionary<DataKey, ScriptableObject>();
        foreach (var entry in entries)
        {
            if (entry == null || entry.key == null || entry.data == null)
            {
                continue;
            }

            if (!cache.ContainsKey(entry.key))
            {
                cache.Add(entry.key, entry.data);
            }
        }
    }
}
