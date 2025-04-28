# UGUI 详细架构与原理

## 1. 详细架构图

```mermaid
graph TD
    subgraph Canvas System
        A[Canvas] --> B[CanvasScaler]
        A --> C[CanvasRenderer]
        A --> D[GraphicRaycaster]
        
        B --> E[UI Scale Mode]
        B --> F[Reference Resolution]
        B --> G[Screen Match Mode]
        
        C --> H[UI Mesh]
        C --> I[UI Material]
        C --> J[Batch System]
    end
    
    subgraph Layout System
        K[Layout System] --> L[Layout Group]
        L --> M[Horizontal Layout]
        L --> N[Vertical Layout]
        L --> O[Grid Layout]
        
        P[Layout Element] --> Q[Min Size]
        P --> R[Preferred Size]
        P --> S[Flexible Size]
    end
    
    subgraph Rendering System
        T[Graphic] --> U[Image]
        T --> V[Text]
        T --> W[RawImage]
        
        X[CanvasRenderer] --> Y[Mesh Generation]
        X --> Z[Material Management]
        X --> AA[Batch Management]
    end
    
    subgraph Event System
        AB[Event System] --> AC[Input Module]
        AC --> AD[Standalone Input]
        AC --> AE[Touch Input]
        
        AF[GraphicRaycaster] --> AG[Raycasters]
        AF --> AH[Event Handlers]
    end
```

## 2. 重建（Rebuild）详细时序图

```mermaid
sequenceDiagram
    participant UI as UI Component
    participant Layout as Layout System
    participant Graphic as Graphic
    participant Canvas as Canvas
    participant Renderer as CanvasRenderer
    
    Note over UI,Renderer: 布局重建触发条件
    UI->>Layout: 位置/大小改变
    UI->>Layout: 子物体添加/删除
    UI->>Layout: 父物体布局改变
    UI->>Layout: Layout Group 属性改变
    
    Note over UI,Renderer: 渲染重建触发条件
    UI->>Graphic: 颜色改变
    UI->>Graphic: 材质改变
    UI->>Graphic: 纹理改变
    UI->>Graphic: 遮罩改变
    
    Graphic->>Graphic: SetVerticesDirty()
    Graphic->>Graphic: SetMaterialDirty()
    
    Graphic->>Canvas: MarkCanvasForRebuild()
    
    Canvas->>Layout: RebuildLayout()
    Layout->>Layout: CalculateLayout()
    Layout->>Layout: SetLayout()
    
    Canvas->>Renderer: Rebuild()
    Renderer->>Renderer: UpdateGeometry()
    Renderer->>Renderer: UpdateMaterial()
    
    Renderer->>Renderer: SetVertices()
    Renderer->>Renderer: SetMaterial()
```

## 3. 合批（Batching）详细流程图

```mermaid
graph TD
    subgraph 合批条件检查
        A[UI Element] --> B{材质检查}
        B -->|相同材质| C{纹理检查}
        B -->|不同材质| D[新批次]
        
        C -->|相同纹理| E{渲染队列检查}
        C -->|不同纹理| D
        
        E -->|相同队列| F{重叠检查}
        E -->|不同队列| D
        
        F -->|无重叠| G{深度检查}
        F -->|有重叠| D
        
        G -->|深度连续| H{Canvas Group检查}
        G -->|深度不连续| D
        
        H -->|相同Group| I[添加到批次]
        H -->|不同Group| D
    end
    
    subgraph 合批优化
        I --> J[更新批次]
        D --> K[创建新批次]
        
        L[批次管理] --> M[合并顶点]
        L --> N[合并材质]
        L --> O[更新批次]
    end
```

## 4. 影响重建的因素

### 4.1 布局重建触发条件
1. **位置/大小改变**：
   - RectTransform 的 position 改变
   - RectTransform 的 sizeDelta 改变
   - RectTransform 的 anchor 改变
   - RectTransform 的 pivot 改变

2. **层级结构改变**：
   - 子物体添加或删除
   - 父物体改变
   - 兄弟物体顺序改变

3. **布局组件属性改变**：
   - Layout Group 的 padding 改变
   - Layout Group 的 spacing 改变
   - Layout Group 的 alignment 改变
   - Layout Element 的 preferred size 改变

### 4.2 渲染重建触发条件
1. **视觉属性改变**：
   - 颜色（Color）改变
   - 透明度（Alpha）改变
   - 材质（Material）改变
   - 纹理（Texture）改变
   - 遮罩（Mask）改变

2. **几何属性改变**：
   - 形状改变
   - 边框改变
   - 圆角改变
   - 阴影改变

## 5. 影响合批的因素

### 5.1 合批条件
1. **材质条件**：
   - 相同材质
   - 相同着色器
   - 相同渲染队列

2. **纹理条件**：
   - 相同纹理
   - 相同纹理设置
   - 相同纹理过滤模式

3. **空间条件**：
   - 无重叠
   - 深度连续
   - 相同 Canvas Group

### 5.2 合批打断因素
1. **材质相关**：
   - 不同材质
   - 不同着色器
   - 不同渲染队列

2. **纹理相关**：
   - 不同纹理
   - 不同纹理设置
   - 不同纹理过滤模式

3. **空间相关**：
   - UI 元素重叠
   - 深度不连续
   - 不同 Canvas Group

4. **其他因素**：
   - 遮罩使用
   - 特殊效果（如阴影）
   - 动态更新频繁 