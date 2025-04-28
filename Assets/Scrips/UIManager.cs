using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Image backgroundImage;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 初始化按钮点击事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        // 初始化滑块事件
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnProgressChanged);
        }

        // 初始化开关事件
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(OnSoundToggled);
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("开始按钮被点击");
        // 在这里添加开始按钮的具体逻辑
    }

    private void OnProgressChanged(float value)
    {
        Debug.Log($"进度值改变: {value}");
        // 在这里添加进度条改变的具体逻辑
    }

    private void OnSoundToggled(bool isOn)
    {
        Debug.Log($"声音开关状态: {isOn}");
        // 在这里添加声音开关的具体逻辑
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (progressSlider != null)
        {
            progressSlider.onValueChanged.RemoveListener(OnProgressChanged);
        }

        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(OnSoundToggled);
        }
    }
} 