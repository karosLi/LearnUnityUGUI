# UGUI 系统关系图

## 1. 事件系统详细架构

```mermaid
graph TD
    subgraph 事件系统架构
        A[EventSystem] --> B[InputModule]
        B --> C[StandaloneInputModule]
        B --> D[TouchInputModule]
        
        E[GraphicRaycaster] --> F[Raycasters]
        F --> G[PhysicsRaycaster]
        F --> H[Physics2DRaycaster]
        
        I[EventHandlers] --> J[IPointerClickHandler]
        I --> K[IPointerDownHandler]
        I --> L[IPointerUpHandler]
        I --> M[IDragHandler]
        I --> N[IScrollHandler]
        
        O[EventData] --> P[PointerEventData]
        O --> Q[BaseEventData]
    end
    
    subgraph 事件流程
        R[Input] --> S[EventSystem.ProcessEvents]
        S --> T[InputModule.Process]
        T --> U[Raycast]
        U --> V[ExecuteEvents]
        V --> W[EventHandlers]
    end
```

## 2. 布局系统详细架构

```mermaid
graph TD
    subgraph 布局系统架构
        A[LayoutSystem] --> B[LayoutGroup]
        B --> C[HorizontalLayoutGroup]
        B --> D[VerticalLayoutGroup]
        B --> E[GridLayoutGroup]
        
        F[LayoutElement] --> G[MinSize]
        F --> H[PreferredSize]
        F --> I[FlexibleSize]
        
        J[LayoutRebuilder] --> K[CalculateLayout]
        J --> L[SetLayout]
        
        M[LayoutUtility] --> N[GetMinSize]
        M --> O[GetPreferredSize]
        M --> P[GetFlexibleSize]
    end
    
    subgraph 布局计算流程
        Q[LayoutChange] --> R[MarkLayoutForRebuild]
        R --> S[LayoutRebuilder.Rebuild]
        S --> T[CalculateLayout]
        T --> U[SetLayout]
    end
```

## 3. 渲染循环系统

```mermaid
sequenceDiagram
    participant Unity as Unity Engine
    participant Canvas as Canvas
    participant Graphic as Graphic
    participant Renderer as CanvasRenderer
    participant GPU as GPU
    
    Unity->>Canvas: LateUpdate
    Canvas->>Canvas: UpdateCanvas
    
    Note over Canvas,GPU: 布局阶段
    Canvas->>Graphic: RebuildLayout
    Graphic->>Graphic: CalculateLayout
    Graphic->>Graphic: SetLayout
    
    Note over Canvas,GPU: 渲染阶段
    Canvas->>Renderer: Rebuild
    Renderer->>Renderer: UpdateGeometry
    Renderer->>Renderer: UpdateMaterial
    
    Note over Canvas,GPU: 合批阶段
    Renderer->>Renderer: BatchElements
    Renderer->>Renderer: UpdateBatches
    
    Note over Canvas,GPU: 提交阶段
    Renderer->>GPU: SubmitBatches
    GPU->>GPU: Render
```

## 4. 模块调用关系

```mermaid
graph TD
    subgraph 核心模块
        A[Canvas] --> B[CanvasScaler]
        A --> C[CanvasRenderer]
        A --> D[GraphicRaycaster]
    end
    
    subgraph 渲染模块
        C --> E[Graphic]
        E --> F[Image]
        E --> G[Text]
        E --> H[RawImage]
        
        I[CanvasRenderer] --> J[MeshGeneration]
        I --> K[MaterialManagement]
        I --> L[BatchManagement]
    end
    
    subgraph 布局模块
        M[LayoutSystem] --> N[LayoutGroup]
        N --> O[HorizontalLayout]
        N --> P[VerticalLayout]
        N --> Q[GridLayout]
        
        R[LayoutElement] --> S[MinSize]
        R --> T[PreferredSize]
        R --> U[FlexibleSize]
    end
    
    subgraph 事件模块
        V[EventSystem] --> W[InputModule]
        W --> X[StandaloneInput]
        W --> Y[TouchInput]
        
        Z[GraphicRaycaster] --> AA[Raycasters]
        Z --> AB[EventHandlers]
    end
    
    subgraph 模块间调用
        A --> M
        A --> V
        C --> I
        E --> M
        V --> Z
    end
```

## 5. 系统交互时序

```mermaid
sequenceDiagram
    participant Unity as Unity Engine
    participant Canvas as Canvas
    participant Layout as LayoutSystem
    participant Event as EventSystem
    participant Render as RenderSystem
    
    Unity->>Canvas: LateUpdate
    
    Note over Canvas,Render: 布局阶段
    Canvas->>Layout: RebuildLayout
    Layout->>Layout: CalculateLayout
    Layout->>Layout: SetLayout
    
    Note over Canvas,Render: 事件阶段
    Event->>Event: ProcessEvents
    Event->>Event: Raycast
    Event->>Event: ExecuteEvents
    
    Note over Canvas,Render: 渲染阶段
    Canvas->>Render: Rebuild
    Render->>Render: UpdateGeometry
    Render->>Render: UpdateMaterial
    Render->>Render: BatchElements
    Render->>Render: SubmitBatches
```

## 6. 关键系统说明

### 6.1 事件系统
- **输入处理**：处理鼠标、键盘、触摸等输入
- **射线检测**：检测 UI 元素是否被点击
- **事件分发**：将事件分发给对应的 UI 元素
- **事件处理**：执行 UI 元素的响应函数

### 6.2 布局系统
- **布局计算**：计算 UI 元素的位置和大小
- **布局更新**：更新 UI 元素的布局
- **布局优化**：优化布局计算性能
- **布局约束**：处理布局约束条件

### 6.3 渲染循环
- **布局阶段**：计算 UI 元素的位置和大小
- **渲染阶段**：生成 UI 元素的网格和材质
- **合批阶段**：合并相同材质的 UI 元素
- **提交阶段**：将渲染数据提交给 GPU

### 6.4 模块调用
- **Canvas**：UI 系统的根节点
- **Graphic**：UI 元素的基类
- **Layout**：处理 UI 元素的布局
- **Event**：处理 UI 元素的交互
- **Renderer**：处理 UI 元素的渲染 