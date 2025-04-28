# UGUI 架构与原理

## 1. 整体架构图

```mermaid
graph TD
    A[Canvas] --> B[CanvasScaler]
    A --> C[CanvasRenderer]
    A --> D[GraphicRaycaster]
    
    B --> E[UI Scale Mode]
    B --> F[Reference Resolution]
    
    C --> G[UI Mesh]
    C --> H[UI Material]
    
    D --> I[Event System]
    D --> J[Raycasters]
    
    K[UI Components] --> L[Graphic]
    L --> M[Image]
    L --> N[Text]
    L --> O[RawImage]
    
    P[Layout System] --> Q[Layout Group]
    Q --> R[Horizontal Layout]
    Q --> S[Vertical Layout]
    Q --> T[Grid Layout]
    
    U[Event System] --> V[Input Module]
    V --> W[Standalone Input]
    V --> X[Touch Input]
```

## 2. 重建（Rebuild）时序图

```mermaid
sequenceDiagram
    participant UI as UI Component
    participant Graphic as Graphic
    participant Canvas as Canvas
    participant Renderer as CanvasRenderer
    
    UI->>Graphic: SetVerticesDirty()
    UI->>Graphic: SetMaterialDirty()
    
    Graphic->>Canvas: MarkCanvasForRebuild()
    
    Canvas->>Renderer: Rebuild()
    Renderer->>Renderer: UpdateGeometry()
    Renderer->>Renderer: UpdateMaterial()
    
    Renderer->>Renderer: SetVertices()
    Renderer->>Renderer: SetMaterial()
```

## 3. 合批（Batching）流程图

```mermaid
graph TD
    A[UI Element] --> B{Check Batching Conditions}
    B -->|Same Material| C{Check Texture}
    B -->|Different Material| D[New Batch]
    
    C -->|Same Texture| E{Check Render Queue}
    C -->|Different Texture| D
    
    E -->|Same Queue| F{Check Overlap}
    E -->|Different Queue| D
    
    F -->|No Overlap| G[Add to Batch]
    F -->|Has Overlap| D
    
    G --> H[Update Batch]
    D --> I[Create New Batch]
```

## 4. 事件系统时序图

```mermaid
sequenceDiagram
    participant Input as Input System
    participant Event as Event System
    participant Raycaster as GraphicRaycaster
    participant UI as UI Component
    
    Input->>Event: Process Input
    Event->>Raycaster: Raycast
    
    Raycaster->>UI: Check Hit
    UI->>UI: OnPointerDown
    UI->>UI: OnPointerUp
    UI->>UI: OnClick
```

## 5. 关键组件关系

```mermaid
graph TD
    A[Canvas] --> B[CanvasRenderer]
    A --> C[GraphicRaycaster]
    
    B --> D[UI Mesh]
    B --> E[UI Material]
    
    C --> F[Event System]
    
    G[UI Component] --> H[Graphic]
    H --> I[Image]
    H --> J[Text]
    H --> K[RawImage]
    
    L[Layout] --> M[Layout Group]
    M --> N[Horizontal]
    M --> O[Vertical]
    M --> P[Grid]
```

## 6. 性能优化关键点

1. **重建优化**：
   - 避免频繁修改 UI 属性
   - 使用对象池
   - 合理使用 Layout
   - 减少嵌套层级

2. **合批优化**：
   - 相同材质和纹理
   - 避免重叠
   - 合理组织层级
   - 使用 Canvas Group

3. **事件优化**：
   - 减少事件监听器
   - 及时移除事件监听
   - 使用事件池
   - 避免频繁触发事件 