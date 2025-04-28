# UGUI 架构交互式动画指南

## 概述

本文档提供UGUI系统架构的交互式动画指南，通过可视化方式展示组件间关系、数据流和渲染流程。适合初学者和高级开发者深入了解UGUI架构。

## 使用说明

1. 本文档中的交互式架构动画需要在Unity中查看
2. 按照提供的步骤创建并运行动画场景
3. 使用控制面板探索不同架构层次和流程
4. 通过交互功能深入研究组件之间的联系

## HTML版本交互式动画

您现在可以通过HTML版本查看UGUI架构交互式动画，无需在Unity中设置：

1. 打开网页版架构动画查看器: `/WebViewer/UGUIArchitectureViewer.html`
2. 在浏览器中直接加载架构动画
3. 使用web界面上的控制按钮进行交互操作

特点：
- 完全浏览器兼容，跨平台支持
- 提供完整的架构交互探索功能
- 支持组件关系的动态展示
- 可缩放查看不同层级细节
- 支持搜索和高亮特定组件

### 设置HTML查看器

1. 确保已克隆完整项目仓库
2. 从项目根目录打开命令行工具
3. 运行以下命令启动本地服务器：
   ```
   cd WebViewer
   python -m http.server 8080
   ```
4. 在浏览器中访问: `http://localhost:8080/UGUIArchitectureViewer.html`

## 一、UGUI架构概览

### 创建交互式动画

在Unity中创建一个新场景，然后将以下脚本附加到一个空游戏对象上：

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UGUIArchitectureVisualizer : MonoBehaviour
{
    [Header("可视化配置")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private GameObject labelPrefab;
    
    [Header("UI元素")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Text stepText;
    [SerializeField] private Text descriptionText;
    
    // 架构图节点
    private Dictionary<string, RectTransform> nodes = new Dictionary<string, RectTransform>();
    private Dictionary<string, List<GameObject>> connections = new Dictionary<string, List<GameObject>>();
    private List<GameObject> activeElements = new List<GameObject>();
    
    // 动画状态
    private int currentStep = 0;
    private int totalSteps = 6;
    private Coroutine currentAnimation;
    
    // 架构图结构
    private readonly string[] nodeNames = new string[] 
    {
        "Unity Engine", "Canvas", "CanvasScaler", "GraphicRaycaster", "CanvasRenderer",
        "Layout System", "Event System", "UI Components", "RectTransform", "Visual Components"
    };
    
    // 节点之间的关系
    private readonly (string, string)[] connections1 = new (string, string)[]
    {
        ("Unity Engine", "Canvas"),
        ("Canvas", "CanvasScaler"),
        ("Canvas", "GraphicRaycaster"),
        ("Canvas", "CanvasRenderer")
    };
    
    private readonly (string, string)[] connections2 = new (string, string)[]
    {
        ("Canvas", "Layout System"),
        ("Canvas", "Event System"),
        ("Canvas", "UI Components")
    };
    
    private readonly (string, string)[] connections3 = new (string, string)[]
    {
        ("UI Components", "RectTransform"),
        ("UI Components", "Visual Components")
    };
    
    // 每个步骤的描述
    private readonly string[] stepDescriptions = new string[]
    {
        "UGUI架构概览：Unity的UI系统由多个协同工作的组件构成",
        "Canvas是UGUI的核心，它充当所有UI元素的容器。CanvasScaler处理尺寸适配，GraphicRaycaster处理输入检测，CanvasRenderer负责渲染",
        "Canvas与其他主要系统的联系：布局系统、事件系统和UI组件",
        "UI组件层级：每个UI组件都有RectTransform（用于定位和尺寸）和一个视觉组件（Image、Text等）",
        "事件流程：输入事件从GraphicRaycaster到EventSystem，然后分发到具体的UI组件",
        "渲染流程：从Visual Components到CanvasRenderer，然后合批渲染到屏幕"
    };
    
    private void Start()
    {
        InitializeButtons();
        CreateNodes();
        UpdateUI();
    }
    
    private void InitializeButtons()
    {
        nextButton.onClick.AddListener(NextStep);
        prevButton.onClick.AddListener(PreviousStep);
        resetButton.onClick.AddListener(ResetVisualization);
        
        // 初始状态
        prevButton.interactable = false;
    }
    
    private void CreateNodes()
    {
        // 创建基本节点
        float startY = 200f;
        float nodeSpacing = 120f;
        
        // 第一行 - 引擎
        CreateNode("Unity Engine", new Vector2(0, startY), new Vector2(200, 60), new Color(0.2f, 0.4f, 0.8f));
        
        // 第二行 - Canvas相关
        CreateNode("Canvas", new Vector2(0, startY - nodeSpacing), new Vector2(160, 60), new Color(0.2f, 0.6f, 0.8f));
        CreateNode("CanvasScaler", new Vector2(-200, startY - nodeSpacing * 2), new Vector2(140, 50), new Color(0.3f, 0.7f, 0.9f));
        CreateNode("GraphicRaycaster", new Vector2(0, startY - nodeSpacing * 2), new Vector2(140, 50), new Color(0.3f, 0.7f, 0.9f));
        CreateNode("CanvasRenderer", new Vector2(200, startY - nodeSpacing * 2), new Vector2(140, 50), new Color(0.3f, 0.7f, 0.9f));
        
        // 第三行 - 主要系统
        CreateNode("Layout System", new Vector2(-200, startY - nodeSpacing * 3), new Vector2(140, 50), new Color(0.3f, 0.8f, 0.6f));
        CreateNode("Event System", new Vector2(0, startY - nodeSpacing * 3), new Vector2(140, 50), new Color(0.3f, 0.8f, 0.6f));
        CreateNode("UI Components", new Vector2(200, startY - nodeSpacing * 3), new Vector2(140, 50), new Color(0.3f, 0.8f, 0.6f));
        
        // 第四行 - 组件细节
        CreateNode("RectTransform", new Vector2(120, startY - nodeSpacing * 4), new Vector2(120, 50), new Color(0.4f, 0.9f, 0.5f));
        CreateNode("Visual Components", new Vector2(280, startY - nodeSpacing * 4), new Vector2(120, 50), new Color(0.4f, 0.9f, 0.5f));
        
        // 隐藏所有节点
        foreach (var node in nodes.Values)
        {
            node.gameObject.SetActive(false);
        }
    }
    
    private void CreateNode(string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject nodeObj = Instantiate(nodePrefab, contentParent);
        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        
        // 设置节点颜色
        Image image = nodeObj.GetComponent<Image>();
        image.color = color;
        
        // 设置节点文本
        Text text = nodeObj.GetComponentInChildren<Text>();
        if (text != null)
            text.text = name;
        
        // 添加事件处理
        Button button = nodeObj.AddComponent<Button>();
        button.onClick.AddListener(() => OnNodeClicked(name));
        
        // 添加鼠标悬停信息
        NodeInfoDisplay infoDisplay = nodeObj.AddComponent<NodeInfoDisplay>();
        infoDisplay.SetNodeInfo(name, GetNodeDescription(name));
        
        nodes.Add(name, rectTransform);
    }
    
    private string GetNodeDescription(string nodeName)
    {
        switch (nodeName)
        {
            case "Unity Engine":
                return "Unity引擎是整个系统的基础，提供渲染、输入和更新循环等核心功能";
            case "Canvas":
                return "Canvas是所有UI元素的容器，定义了UI的渲染方式（Screen Space或World Space）";
            case "CanvasScaler":
                return "处理UI的缩放和适配，支持多种缩放模式（Constant Pixel Size、Scale With Screen Size等）";
            case "GraphicRaycaster":
                return "负责检测输入事件（如点击）并将它们发送到事件系统";
            case "CanvasRenderer":
                return "将UI元素转换为可渲染的网格，并处理批处理优化";
            case "Layout System":
                return "管理UI元素的排列和尺寸，包括多种布局组件（如VerticalLayoutGroup等）";
            case "Event System":
                return "处理和分发输入事件，管理UI元素的选择状态和导航";
            case "UI Components":
                return "内置UI组件如Button、Image、Text等，以及用户自定义UI组件";
            case "RectTransform":
                return "扩展自Transform组件，专门处理2D矩形的位置、尺寸和锚点";
            case "Visual Components":
                return "如Image、Text等，负责实际的视觉呈现";
            default:
                return "";
        }
    }
    
    private void OnNodeClicked(string nodeName)
    {
        // 处理节点点击事件，可以显示详细信息或触发相关动画
        descriptionText.text = GetNodeDescription(nodeName);
    }
    
    private void CreateConnection(string fromNode, string toNode, Color color)
    {
        if (!nodes.ContainsKey(fromNode) || !nodes.ContainsKey(toNode))
            return;
            
        RectTransform from = nodes[fromNode];
        RectTransform to = nodes[toNode];
        
        GameObject connection = Instantiate(connectionPrefab, contentParent);
        connection.transform.SetAsFirstSibling(); // 确保线条在节点下方
        
        RectTransform connectionRect = connection.GetComponent<RectTransform>();
        LineRenderer lineRenderer = connection.GetComponent<LineRenderer>();
        
        if (lineRenderer != null)
        {
            // 计算线条起点和终点
            Vector3 startPos = from.position;
            Vector3 endPos = to.position;
            
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
        
        // 保存创建的连接
        string connectionKey = $"{fromNode}-{toNode}";
        if (!connections.ContainsKey(connectionKey))
            connections[connectionKey] = new List<GameObject>();
            
        connections[connectionKey].Add(connection);
        activeElements.Add(connection);
    }
    
    private void NextStep()
    {
        if (currentStep < totalSteps - 1)
        {
            currentStep++;
            UpdateVisualization();
            UpdateUI();
        }
    }
    
    private void PreviousStep()
    {
        if (currentStep > 0)
        {
            currentStep--;
            UpdateVisualization();
            UpdateUI();
        }
    }
    
    private void ResetVisualization()
    {
        currentStep = 0;
        UpdateVisualization();
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        stepText.text = $"步骤 {currentStep + 1}/{totalSteps}";
        descriptionText.text = stepDescriptions[currentStep];
        
        prevButton.interactable = (currentStep > 0);
        nextButton.interactable = (currentStep < totalSteps - 1);
    }
    
    private void UpdateVisualization()
    {
        // 清除当前活动的元素
        foreach (var element in activeElements)
        {
            Destroy(element);
        }
        activeElements.Clear();
        
        // 隐藏所有节点
        foreach (var node in nodes.Values)
        {
            node.gameObject.SetActive(false);
        }
        
        // 根据当前步骤显示相应元素
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(AnimateStep(currentStep));
    }
    
    private IEnumerator AnimateStep(int step)
    {
        float animDelay = 0.2f;
        
        switch (step)
        {
            case 0: // 架构概览
                yield return ShowNodeWithAnimation("Unity Engine", animDelay);
                yield return ShowNodeWithAnimation("Canvas", animDelay);
                break;
                
            case 1: // Canvas及其直接相关组件
                yield return ShowNodeWithAnimation("Unity Engine", 0);
                yield return ShowNodeWithAnimation("Canvas", 0);
                yield return ShowNodeWithAnimation("CanvasScaler", animDelay);
                yield return ShowNodeWithAnimation("GraphicRaycaster", animDelay);
                yield return ShowNodeWithAnimation("CanvasRenderer", animDelay);
                
                // 添加连接
                foreach (var conn in connections1)
                {
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.5f, 0.9f, 0.8f));
                    yield return new WaitForSeconds(animDelay * 0.5f);
                }
                break;
                
            case 2: // Canvas与主要系统的关系
                // 保留上一步的所有内容
                yield return ShowNodeWithAnimation("Unity Engine", 0);
                yield return ShowNodeWithAnimation("Canvas", 0);
                yield return ShowNodeWithAnimation("CanvasScaler", 0);
                yield return ShowNodeWithAnimation("GraphicRaycaster", 0);
                yield return ShowNodeWithAnimation("CanvasRenderer", 0);
                
                foreach (var conn in connections1)
                {
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.5f, 0.9f, 0.8f));
                }
                
                // 添加新内容
                yield return ShowNodeWithAnimation("Layout System", animDelay);
                yield return ShowNodeWithAnimation("Event System", animDelay);
                yield return ShowNodeWithAnimation("UI Components", animDelay);
                
                foreach (var conn in connections2)
                {
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.7f, 0.9f, 0.8f));
                    yield return new WaitForSeconds(animDelay * 0.5f);
                }
                break;
                
            case 3: // UI组件层级
                // 显示所有前面的节点
                foreach (string nodeName in new string[] {
                    "Unity Engine", "Canvas", "CanvasScaler", "GraphicRaycaster", "CanvasRenderer",
                    "Layout System", "Event System", "UI Components"
                })
                {
                    yield return ShowNodeWithAnimation(nodeName, 0);
                }
                
                // 创建前面的连接
                foreach (var conn in connections1)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.5f, 0.9f, 0.8f));
                foreach (var conn in connections2)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.7f, 0.9f, 0.8f));
                
                // 添加UI组件细节
                yield return ShowNodeWithAnimation("RectTransform", animDelay);
                yield return ShowNodeWithAnimation("Visual Components", animDelay);
                
                foreach (var conn in connections3)
                {
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.6f, 0.9f, 0.6f, 0.8f));
                    yield return new WaitForSeconds(animDelay * 0.5f);
                }
                break;
                
            case 4: // 事件流程
                // 显示所有节点
                foreach (string nodeName in nodeNames)
                {
                    yield return ShowNodeWithAnimation(nodeName, 0);
                }
                
                // 创建所有基本连接
                foreach (var conn in connections1)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.5f, 0.9f, 0.5f));
                foreach (var conn in connections2)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.7f, 0.9f, 0.5f));
                foreach (var conn in connections3)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.6f, 0.9f, 0.6f, 0.5f));
                
                // 高亮事件流程
                Color eventFlowColor = new Color(1f, 0.6f, 0.2f, 1f);
                CreateConnection("GraphicRaycaster", "Event System", eventFlowColor);
                yield return new WaitForSeconds(animDelay);
                CreateConnection("Event System", "UI Components", eventFlowColor);
                break;
                
            case 5: // 渲染流程
                // 显示所有节点
                foreach (string nodeName in nodeNames)
                {
                    yield return ShowNodeWithAnimation(nodeName, 0);
                }
                
                // 创建所有基本连接（淡化）
                foreach (var conn in connections1)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.5f, 0.9f, 0.3f));
                foreach (var conn in connections2)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.5f, 0.7f, 0.9f, 0.3f));
                foreach (var conn in connections3)
                    CreateConnection(conn.Item1, conn.Item2, new Color(0.6f, 0.9f, 0.6f, 0.3f));
                
                // 高亮渲染流程
                Color renderFlowColor = new Color(0.2f, 0.9f, 0.4f, 1f);
                CreateConnection("Visual Components", "CanvasRenderer", renderFlowColor);
                yield return new WaitForSeconds(animDelay);
                CreateConnection("CanvasRenderer", "Canvas", renderFlowColor);
                yield return new WaitForSeconds(animDelay);
                CreateConnection("Canvas", "Unity Engine", renderFlowColor);
                break;
        }
    }
    
    private IEnumerator ShowNodeWithAnimation(string nodeName, float delay)
    {
        if (!nodes.ContainsKey(nodeName))
            yield break;
            
        if (delay > 0)
            yield return new WaitForSeconds(delay);
            
        RectTransform nodeTransform = nodes[nodeName];
        nodeTransform.gameObject.SetActive(true);
        
        // 添加简单的缩放动画
        nodeTransform.localScale = Vector3.zero;
        float animTime = 0.3f;
        float startTime = Time.time;
        
        while (Time.time < startTime + animTime)
        {
            float t = (Time.time - startTime) / animTime;
            t = t * t * (3f - 2f * t); // 平滑插值
            nodeTransform.localScale = Vector3.one * t;
            yield return null;
        }
        
        nodeTransform.localScale = Vector3.one;
    }
}

// 用于显示节点信息的组件
public class NodeInfoDisplay : MonoBehaviour
{
    private string nodeName;
    private string nodeDescription;
    
    private GameObject tooltipPanel;
    private Text tooltipText;
    
    public void SetNodeInfo(string name, string description)
    {
        nodeName = name;
        nodeDescription = description;
    }
    
    void Start()
    {
        // 创建提示面板
        tooltipPanel = new GameObject("TooltipPanel");
        tooltipPanel.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform, false);
        
        RectTransform panelRect = tooltipPanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(200, 100);
        
        Image panelImage = tooltipPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        GameObject textObj = new GameObject("TooltipText");
        textObj.transform.SetParent(tooltipPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        tooltipText = textObj.AddComponent<Text>();
        tooltipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tooltipText.fontSize = 14;
        tooltipText.color = Color.white;
        
        // 默认隐藏
        tooltipPanel.SetActive(false);
        
        // 添加事件触发器组件
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { ShowTooltip(); });
        trigger.triggers.Add(enterEntry);
        
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { HideTooltip(); });
        trigger.triggers.Add(exitEntry);
    }
    
    private void ShowTooltip()
    {
        tooltipText.text = $"{nodeName}\n\n{nodeDescription}";
        tooltipPanel.SetActive(true);
        
        // 更新位置
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 position = rect.position + new Vector3(0, rect.rect.height / 2 + 50, 0);
        tooltipPanel.GetComponent<RectTransform>().position = position;
    }
    
    private void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}
```

### 使用指南

1. 创建一个空游戏对象，将上面的脚本附加到它上面
2. 创建一个Canvas对象和以下预制体:
   - nodePrefab: 一个带有Image和Text的基本UI节点
   - connectionPrefab: 一个带有LineRenderer的游戏对象
   - labelPrefab: 一个Text对象用于标签
3. 在Inspector中设置所有引用
4. 运行应用，使用底部的控制按钮浏览架构图

## 二、自定义交互式架构可视化指南

如果您想创建自己的UGUI架构交互式动画，可以按照以下步骤操作：

1. **规划可视化内容**
   - 确定要展示的UGUI组件和关系
   - 设计各个步骤的展示顺序和重点
   - 准备每个组件的简短描述

2. **准备所需资源**
   - 节点图形（可以使用simple shapes或自定义图形）
   - 连接线资源（使用LineRenderer或Image组件）
   - 信息面板和描述文本

3. **实现交互逻辑**
   - 为节点添加点击和悬停事件
   - 实现步骤之间的过渡动画
   - 添加导航控制（前进、后退、重置）

4. **增强用户体验**
   - 添加高亮效果突出当前步骤的重点
   - 添加过渡动画使视觉效果更平滑
   - 提供详细的文本说明

## 三、架构图进阶可视化

对于更高级的UGUI架构可视化，您可以考虑添加以下功能：

### 1. 组件依赖关系可视化

显示UGUI组件之间的依赖关系，用不同颜色的线条表示不同类型的依赖：

```csharp
// 示例代码片段 - 创建不同类型的连接线
private void CreateDependencyConnection(string fromNode, string toNode, DependencyType type)
{
    Color connectionColor = Color.white;
    float lineWidth = 2f;
    
    switch(type)
    {
        case DependencyType.Inheritance:
            connectionColor = new Color(0.2f, 0.5f, 0.9f);
            lineWidth = 3f;
            break;
        case DependencyType.Composition:
            connectionColor = new Color(0.9f, 0.3f, 0.3f);
            lineWidth = 2.5f;
            break;
        case DependencyType.Reference:
            connectionColor = new Color(0.3f, 0.8f, 0.3f);
            lineWidth = 2f;
            break;
        case DependencyType.WeakReference:
            connectionColor = new Color(0.8f, 0.8f, 0.2f);
            lineWidth = 1.5f;
            break;
    }
    
    CreateConnection(fromNode, toNode, connectionColor, lineWidth);
}
```

### 2. 动态流程演示

使用动画粒子沿着连接线移动，可视化数据或事件在组件间的流动：

```csharp
// 示例代码片段 - 创建流动动画
private IEnumerator AnimateDataFlow(string fromNode, string toNode, Color particleColor)
{
    if (!nodes.ContainsKey(fromNode) || !nodes.ContainsKey(toNode))
        yield break;
    
    Vector3 startPos = nodes[fromNode].position;
    Vector3 endPos = nodes[toNode].position;
    
    GameObject particle = Instantiate(particlePrefab, contentParent);
    particle.GetComponent<Image>().color = particleColor;
    
    float animTime = 1.0f;
    float startTime = Time.time;
    
    while (Time.time < startTime + animTime)
    {
        float t = (Time.time - startTime) / animTime;
        // 使用贝塞尔曲线使动画更平滑
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f) + new Vector3(0, 20f, 0);
        Vector3 p01 = Vector3.Lerp(startPos, midPoint, t);
        Vector3 p12 = Vector3.Lerp(midPoint, endPos, t);
        Vector3 position = Vector3.Lerp(p01, p12, t);
        
        particle.transform.position = position;
        yield return null;
    }
    
    Destroy(particle);
}
```

### 3. 可折叠/展开的子系统

允许用户点击主要节点展开或折叠子系统，以便关注特定区域：

```csharp
// 示例代码片段 - 可折叠子系统
private void ToggleSubsystem(string parentNodeName)
{
    if (!subsystems.ContainsKey(parentNodeName))
        return;
        
    bool isExpanded = expandedSubsystems.Contains(parentNodeName);
    
    if (isExpanded)
    {
        // 折叠子系统
        foreach (string childNode in subsystems[parentNodeName])
        {
            nodes[childNode].gameObject.SetActive(false);
            // 也隐藏连接线
            HideConnections(parentNodeName, childNode);
        }
        expandedSubsystems.Remove(parentNodeName);
    }
    else
    {
        // 展开子系统
        foreach (string childNode in subsystems[parentNodeName])
        {
            StartCoroutine(ShowNodeWithAnimation(childNode, 0.1f));
            // 显示连接线
            ShowConnections(parentNodeName, childNode);
        }
        expandedSubsystems.Add(parentNodeName);
    }
}
``` 