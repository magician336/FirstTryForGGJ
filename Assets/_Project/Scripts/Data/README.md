# Data Folder

This folder centralizes data access using ScriptableObjects to keep systems decoupled and extensible.

## Core Concepts
- `DataKey`: unique key assets for referencing data without hard-coded strings.
- `GameDataRegistry`: registry asset that maps keys to ScriptableObject data assets.
- `GameDataLocator`: scene singleton that exposes `Get<T>(DataKey)` / `TryGet<T>` access.

## Typical Usage
1. Create a `DataKey` asset for each data entry.
2. Create data ScriptableObjects (e.g., settings, curves, tables).
3. Add them into a `GameDataRegistry` asset.
4. Add a `GameDataLocator` in your bootstrap scene and assign the registry.

## Example
```csharp
public class ExampleConsumer : MonoBehaviour
{
    [SerializeField] private DataKey playerSettingsKey;

    private void Start()
    {
        var settings = GameDataLocator.Instance.Get<PlayerSettings>(playerSettingsKey);
        if (settings != null)
        {
            // use settings
        }
    }
}
```
