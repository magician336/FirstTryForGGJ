# 数据文件夹

该文件夹通过 ScriptableObject 集中管理数据访问，确保系统解耦且具备可扩展性。

## 核心概念
- `DataKey`：用于引用数据的唯一键资产，无需硬编码字符串。
- `GameDataRegistry`：注册表资产，用于将键映射到 ScriptableObject 数据资产。
- `GameDataLocator`：场景单例类，对外提供 `Get<T>(DataKey)` / `TryGet<T>` 数据访问方法。

## 典型用法
1. 为每个数据条目创建一个 `DataKey` 资产。
2. 创建数据类 ScriptableObject（例如：设置、曲线、表格）。
3. 将这些数据资产添加到 `GameDataRegistry` 资产中。
4. 在启动场景（bootstrap scene）中添加 `GameDataLocator`，并为其分配对应的注册表。

## 示例
```csharp
public class ExampleConsumer : MonoBehaviour
{
    [SerializeField] private DataKey playerSettingsKey;

    private void Start()
    {
        var settings = GameDataLocator.Instance.Get<PlayerSettings>(playerSettingsKey);
        if (settings != null)
        {
            // 使用设置数据
        }
    }
}
```

### 术语说明
- **ScriptableObject**：Unity 特有的可编写脚本对象，此处保留英文术语（行业通用做法）。
- **解耦（decoupled）**：降低模块间的依赖，提升代码可维护性。
- **单例（singleton）**：保证场景中仅有一个实例的设计模式。
- **启动场景（bootstrap scene）**：游戏初始化时最先加载的场景，负责全局数据/服务的初始化。
