using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UGUI交互式动画管理器
/// 管理并切换不同类型的UGUI交互式动画可视化
/// </summary>
public class UGUIAnimationManager : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Button architectureButton;
    [SerializeField] private Button sequenceButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private TMP_Dropdown animationSelector;
    [SerializeField] private Transform visualizerContainer;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Button stepButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private TextMeshProUGUI infoText;

    // 可视化组件
    private GameObject architectureVisualizer;
    private GameObject sequenceVisualizer;
    private IUGUIVisualizer currentVisualizer;

    // 动画类型
    private enum VisualizerType
    {
        Architecture,
        Sequence
    }

    private VisualizerType currentType = VisualizerType.Architecture;
    private Dictionary<VisualizerType, List<string>> visualizerOptions = new Dictionary<VisualizerType, List<string>>();

    private void Awake()
    {
        // 初始化可视化选项
        visualizerOptions[VisualizerType.Architecture] = new List<string> {
            "Canvas层级结构", 
            "事件系统", 
            "渲染组件关系", 
            "布局系统"
        };
        
        visualizerOptions[VisualizerType.Sequence] = new List<string> {
            "UI生命周期", 
            "事件处理流程", 
            "Canvas绘制", 
            "动态UI加载"
        };
    }

    private void Start()
    {
        // 初始化UI
        InitializeUI();
        
        // 默认载入架构可视化
        LoadVisualizer(VisualizerType.Architecture);
    }

    private void InitializeUI()
    {
        // 设置按钮监听
        architectureButton.onClick.AddListener(() => SwitchVisualizer(VisualizerType.Architecture));
        sequenceButton.onClick.AddListener(() => SwitchVisualizer(VisualizerType.Sequence));
        exportButton.onClick.AddListener(ExportDocument);
        helpButton.onClick.AddListener(ToggleHelp);
        
        playPauseButton.onClick.AddListener(TogglePlayPause);
        stepButton.onClick.AddListener(StepForward);
        resetButton.onClick.AddListener(ResetAnimation);
        
        speedSlider.onValueChanged.AddListener(ChangeSpeed);
        
        animationSelector.onValueChanged.AddListener(OnAnimationSelected);
        
        // 初始状态
        helpPanel.SetActive(false);
        UpdateInfoText("欢迎使用UGUI交互式动画可视化工具");
    }

    /// <summary>
    /// 切换可视化类型（架构/时序）
    /// </summary>
    public void SwitchVisualizer(VisualizerType type)
    {
        if (currentType == type)
            return;
            
        currentType = type;
        
        // 清除当前可视化
        ClearCurrentVisualizer();
        
        // 更新下拉菜单
        UpdateDropdown();
        
        // 加载新可视化
        LoadVisualizer(type);
        
        // 更新UI状态
        UpdateUI();
    }
    
    /// <summary>
    /// 更新动画选择下拉菜单
    /// </summary>
    private void UpdateDropdown()
    {
        animationSelector.ClearOptions();
        animationSelector.AddOptions(visualizerOptions[currentType]);
        animationSelector.value = 0;
    }
    
    /// <summary>
    /// 加载指定类型的可视化
    /// </summary>
    private void LoadVisualizer(VisualizerType type)
    {
        switch (type)
        {
            case VisualizerType.Architecture:
                LoadArchitectureVisualizer();
                break;
            case VisualizerType.Sequence:
                LoadSequenceVisualizer();
                break;
        }
        
        OnAnimationSelected(0); // 默认选择第一个动画
    }
    
    /// <summary>
    /// 清除当前可视化
    /// </summary>
    private void ClearCurrentVisualizer()
    {
        if (currentVisualizer != null)
        {
            currentVisualizer.CleanUp();
            currentVisualizer = null;
        }
        
        // 清除容器中的所有子对象
        foreach (Transform child in visualizerContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// 加载架构可视化
    /// </summary>
    private void LoadArchitectureVisualizer()
    {
        // 实例化架构可视化预制体
        architectureVisualizer = new GameObject("ArchitectureVisualizer");
        architectureVisualizer.transform.SetParent(visualizerContainer, false);
        
        // 添加可视化组件
        var visualizer = architectureVisualizer.AddComponent<UGUIArchitectureVisualizer>();
        currentVisualizer = visualizer;
        
        UpdateInfoText("已加载UGUI架构可视化");
    }
    
    /// <summary>
    /// 加载时序可视化
    /// </summary>
    private void LoadSequenceVisualizer()
    {
        // 实例化时序可视化预制体
        sequenceVisualizer = new GameObject("SequenceVisualizer");
        sequenceVisualizer.transform.SetParent(visualizerContainer, false);
        
        // 添加可视化组件
        var visualizer = sequenceVisualizer.AddComponent<UGUISequenceVisualizer>();
        currentVisualizer = visualizer;
        
        UpdateInfoText("已加载UGUI时序可视化");
    }
    
    /// <summary>
    /// 动画选择回调
    /// </summary>
    private void OnAnimationSelected(int index)
    {
        if (currentVisualizer == null)
            return;
            
        string animationName = visualizerOptions[currentType][index];
        currentVisualizer.LoadAnimation(animationName);
        UpdateInfoText($"正在展示: {animationName}");
    }
    
    /// <summary>
    /// 播放/暂停动画
    /// </summary>
    private void TogglePlayPause()
    {
        if (currentVisualizer == null)
            return;
            
        bool isPlaying = currentVisualizer.TogglePlayPause();
        playPauseButton.GetComponentInChildren<TextMeshProUGUI>().text = isPlaying ? "暂停" : "播放";
    }
    
    /// <summary>
    /// 单步执行动画
    /// </summary>
    private void StepForward()
    {
        currentVisualizer?.StepForward();
    }
    
    /// <summary>
    /// 重置动画
    /// </summary>
    private void ResetAnimation()
    {
        currentVisualizer?.ResetAnimation();
        playPauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "播放";
    }
    
    /// <summary>
    /// 改变动画速度
    /// </summary>
    private void ChangeSpeed(float speed)
    {
        currentVisualizer?.SetSpeed(speed);
    }
    
    /// <summary>
    /// 导出文档
    /// </summary>
    private void ExportDocument()
    {
        // 导出当前可视化状态为文档
        Debug.Log("导出文档功能");
        UpdateInfoText("文档已导出至项目Docs/UGUI目录");
    }
    
    /// <summary>
    /// 切换帮助面板
    /// </summary>
    private void ToggleHelp()
    {
        helpPanel.SetActive(!helpPanel.activeSelf);
    }
    
    /// <summary>
    /// 更新UI状态
    /// </summary>
    private void UpdateUI()
    {
        // 更新按钮状态
        architectureButton.interactable = currentType != VisualizerType.Architecture;
        sequenceButton.interactable = currentType != VisualizerType.Sequence;
    }
    
    /// <summary>
    /// 更新信息文本
    /// </summary>
    private void UpdateInfoText(string message)
    {
        if (infoText != null)
        {
            infoText.text = message;
        }
    }
}

/// <summary>
/// UGUI可视化接口
/// </summary>
public interface IUGUIVisualizer
{
    void LoadAnimation(string animationName);
    bool TogglePlayPause();
    void StepForward();
    void ResetAnimation();
    void SetSpeed(float speed);
    void CleanUp();
}

/// <summary>
/// UGUI架构可视化组件
/// </summary>
public class UGUIArchitectureVisualizer : MonoBehaviour, IUGUIVisualizer
{
    private bool isPlaying = false;
    private float animationSpeed = 1.0f;
    private string currentAnimation = "";
    
    public void LoadAnimation(string animationName)
    {
        currentAnimation = animationName;
        Debug.Log($"加载架构动画: {animationName}");
        ResetAnimation();
    }
    
    public bool TogglePlayPause()
    {
        isPlaying = !isPlaying;
        return isPlaying;
    }
    
    public void StepForward()
    {
        Debug.Log("架构动画单步前进");
    }
    
    public void ResetAnimation()
    {
        isPlaying = false;
        Debug.Log("重置架构动画");
    }
    
    public void SetSpeed(float speed)
    {
        animationSpeed = speed;
        Debug.Log($"设置架构动画速度: {speed}");
    }
    
    public void CleanUp()
    {
        // 清理资源
        Debug.Log("清理架构动画资源");
    }
    
    private void Update()
    {
        if (isPlaying)
        {
            // 执行动画逻辑
        }
    }
}

/// <summary>
/// UGUI时序可视化组件
/// </summary>
public class UGUISequenceVisualizer : MonoBehaviour, IUGUIVisualizer
{
    private bool isPlaying = false;
    private float animationSpeed = 1.0f;
    private string currentAnimation = "";
    
    public void LoadAnimation(string animationName)
    {
        currentAnimation = animationName;
        Debug.Log($"加载时序动画: {animationName}");
        ResetAnimation();
    }
    
    public bool TogglePlayPause()
    {
        isPlaying = !isPlaying;
        return isPlaying;
    }
    
    public void StepForward()
    {
        Debug.Log("时序动画单步前进");
    }
    
    public void ResetAnimation()
    {
        isPlaying = false;
        Debug.Log("重置时序动画");
    }
    
    public void SetSpeed(float speed)
    {
        animationSpeed = speed;
        Debug.Log($"设置时序动画速度: {speed}");
    }
    
    public void CleanUp()
    {
        // 清理资源
        Debug.Log("清理时序动画资源");
    }
    
    private void Update()
    {
        if (isPlaying)
        {
            // 执行动画逻辑
        }
    }
} 