# Cursor 规则文档

## 1. 规则概述

Cursor 规则是一组用于控制 UI 元素交互行为的规则，主要包括鼠标指针样式、交互状态和动画效果等。通过合理设置 Cursor 规则，可以提升用户体验和界面交互的流畅度。

## 2. 基本规则

### 2.1 指针样式规则

```csharp
// 设置默认指针样式
Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);

// 设置自定义指针样式
Cursor.SetCursor(customTexture, hotSpot, CursorMode.ForceSoftware);
```

**规则说明**：
- `defaultTexture`：默认指针纹理
- `hotSpot`：指针热点位置（通常是纹理的中心点）
- `CursorMode`：指针模式
  - `Auto`：自动选择硬件或软件指针
  - `ForceSoftware`：强制使用软件指针
  - `ForceHardware`：强制使用硬件指针

### 2.2 指针可见性规则

```csharp
// 显示/隐藏指针
Cursor.visible = true/false;

// 锁定指针到屏幕中心
Cursor.lockState = CursorLockMode.Locked;
```

**规则说明**：
- `visible`：控制指针是否可见
- `lockState`：控制指针的锁定状态
  - `None`：不锁定
  - `Locked`：锁定到屏幕中心
  - `Confined`：限制在游戏窗口内

## 3. 交互规则

### 3.1 按钮交互规则

```csharp
public class ButtonCursorRule : MonoBehaviour
{
    [SerializeField] private Texture2D hoverTexture;
    [SerializeField] private Texture2D clickTexture;
    private Texture2D defaultTexture;

    private void Start()
    {
        defaultTexture = Cursor.GetCursor();
    }

    public void OnPointerEnter()
    {
        Cursor.SetCursor(hoverTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerExit()
    {
        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerDown()
    {
        Cursor.SetCursor(clickTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerUp()
    {
        Cursor.SetCursor(hoverTexture, Vector2.zero, CursorMode.Auto);
    }
}
```

**规则说明**：
- 鼠标悬停时改变指针样式
- 点击时改变指针样式
- 离开时恢复默认样式

### 3.2 拖拽交互规则

```csharp
public class DragCursorRule : MonoBehaviour
{
    [SerializeField] private Texture2D dragTexture;
    private Texture2D defaultTexture;
    private bool isDragging;

    private void Start()
    {
        defaultTexture = Cursor.GetCursor();
    }

    public void OnBeginDrag()
    {
        isDragging = true;
        Cursor.SetCursor(dragTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnEndDrag()
    {
        isDragging = false;
        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }
}
```

**规则说明**：
- 开始拖拽时改变指针样式
- 结束拖拽时恢复默认样式
- 拖拽过程中保持指针样式

## 4. 状态规则

### 4.1 游戏状态规则

```csharp
public class GameStateCursorRule : MonoBehaviour
{
    [SerializeField] private Texture2D gameOverTexture;
    [SerializeField] private Texture2D pauseTexture;
    private Texture2D defaultTexture;

    private void Start()
    {
        defaultTexture = Cursor.GetCursor();
    }

    public void OnGameOver()
    {
        Cursor.visible = true;
        Cursor.SetCursor(gameOverTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnPause()
    {
        Cursor.visible = true;
        Cursor.SetCursor(pauseTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnResume()
    {
        Cursor.visible = false;
        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }
}
```

**规则说明**：
- 游戏结束时显示特定指针
- 暂停时显示特定指针
- 恢复时隐藏指针

### 4.2 加载状态规则

```csharp
public class LoadingCursorRule : MonoBehaviour
{
    [SerializeField] private Texture2D loadingTexture;
    private Texture2D defaultTexture;

    private void Start()
    {
        defaultTexture = Cursor.GetCursor();
    }

    public void OnLoadingStart()
    {
        Cursor.SetCursor(loadingTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnLoadingComplete()
    {
        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }
}
```

**规则说明**：
- 加载开始时显示加载指针
- 加载完成时恢复默认指针

## 5. 最佳实践

### 5.1 性能优化

1. **纹理管理**
   - 使用适当的纹理大小
   - 预加载所有指针纹理
   - 使用纹理图集

2. **状态切换**
   - 避免频繁切换指针样式
   - 使用状态机管理指针状态
   - 缓存常用指针样式

### 5.2 用户体验

1. **视觉反馈**
   - 提供清晰的视觉反馈
   - 使用合适的指针大小
   - 保持指针样式的一致性

2. **交互设计**
   - 根据交互类型选择合适的指针样式
   - 提供适当的悬停效果
   - 考虑不同设备的兼容性

### 5.3 代码组织

1. **模块化**
   - 将指针规则封装为独立组件
   - 使用接口定义标准行为
   - 实现可复用的指针管理器

2. **配置管理**
   - 使用 ScriptableObject 管理指针配置
   - 支持运行时动态切换
   - 提供编辑器工具

## 6. 示例代码

### 6.1 指针管理器

```csharp
public class CursorManager : MonoBehaviour
{
    [System.Serializable]
    public class CursorState
    {
        public string stateName;
        public Texture2D cursorTexture;
        public Vector2 hotSpot;
    }

    [SerializeField] private List<CursorState> cursorStates;
    private Dictionary<string, CursorState> stateMap;
    private CursorState currentState;

    private void Awake()
    {
        InitializeStateMap();
    }

    private void InitializeStateMap()
    {
        stateMap = new Dictionary<string, CursorState>();
        foreach (var state in cursorStates)
        {
            stateMap[state.stateName] = state;
        }
    }

    public void SetCursorState(string stateName)
    {
        if (stateMap.TryGetValue(stateName, out var state))
        {
            currentState = state;
            Cursor.SetCursor(state.cursorTexture, state.hotSpot, CursorMode.Auto);
        }
    }

    public void ResetCursor()
    {
        if (cursorStates.Count > 0)
        {
            SetCursorState(cursorStates[0].stateName);
        }
    }
}
```

### 6.2 指针配置

```csharp
[CreateAssetMenu(fileName = "CursorConfig", menuName = "UI/Cursor Config")]
public class CursorConfig : ScriptableObject
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D clickCursor;
    [SerializeField] private Texture2D dragCursor;
    [SerializeField] private Texture2D loadingCursor;

    public Texture2D DefaultCursor => defaultCursor;
    public Texture2D HoverCursor => hoverCursor;
    public Texture2D ClickCursor => clickCursor;
    public Texture2D DragCursor => dragCursor;
    public Texture2D LoadingCursor => loadingCursor;
}
```

## 7. 注意事项

1. **平台兼容性**
   - 考虑不同平台的指针行为差异
   - 测试不同分辨率和DPI设置
   - 处理触摸设备的特殊情况

2. **性能考虑**
   - 避免频繁切换指针样式
   - 合理管理内存使用
   - 优化纹理加载

3. **用户体验**
   - 保持指针样式的一致性
   - 提供清晰的视觉反馈
   - 考虑可访问性需求 