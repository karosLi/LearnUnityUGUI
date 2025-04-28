using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponentExample : MonoBehaviour
{
    [Header("UI 组件示例")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private Slider slider;
    [SerializeField] private Toggle toggle;
    [SerializeField] private InputField inputField;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Dropdown dropdown;

    private void Start()
    {
        // 初始化按钮点击事件
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        // 初始化滑块事件
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        // 初始化开关事件
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        // 初始化输入框事件
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnInputValueChanged);
            inputField.onEndEdit.AddListener(OnInputEndEdit);
        }

        // 初始化下拉框事件
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    private void OnButtonClick()
    {
        Debug.Log("按钮被点击");
        if (text != null)
        {
            text.text = "按钮被点击了！";
        }
    }

    private void OnSliderValueChanged(float value)
    {
        Debug.Log($"滑块值改变: {value}");
        if (text != null)
        {
            text.text = $"滑块值: {value:F2}";
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"开关状态: {isOn}");
        if (text != null)
        {
            text.text = $"开关状态: {(isOn ? "开启" : "关闭")}";
        }
    }

    private void OnInputValueChanged(string value)
    {
        Debug.Log($"输入框值改变: {value}");
    }

    private void OnInputEndEdit(string value)
    {
        Debug.Log($"输入框编辑结束: {value}");
        if (text != null)
        {
            text.text = $"输入的内容: {value}";
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        Debug.Log($"下拉框选择改变: {index}");
        if (text != null && dropdown != null)
        {
            text.text = $"选择的选项: {dropdown.options[index].text}";
        }
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }

        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputValueChanged);
            inputField.onEndEdit.RemoveListener(OnInputEndEdit);
        }

        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }
} 