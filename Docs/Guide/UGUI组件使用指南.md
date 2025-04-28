# Unity UGUI组件使用指南

## 1. 基础组件使用

### 1.1 Image组件
#### 基本用法：
```csharp
// 1. 获取Image组件
Image image = GetComponent<Image>();

// 2. 设置图片
image.sprite = Resources.Load<Sprite>("UI/button");

// 3. 设置颜色
image.color = new Color(1f, 1f, 1f, 0.5f); // 半透明

// 4. 设置图片类型
image.type = Image.Type.Sliced; // 九宫格
image.type = Image.Type.Tiled;  // 平铺
image.type = Image.Type.Filled; // 填充

// 5. 填充模式（当type为Filled时）
image.fillMethod = Image.FillMethod.Radial360;
image.fillOrigin = 0;
image.fillAmount = 0.5f; // 填充50%

// 6. 设置材质
image.material = new Material(Shader.Find("UI/Default"));

// 7. 设置Raycast Target
image.raycastTarget = true; // 是否接收射线检测
```

#### 好的做法：
```csharp
// 使用Sprite Atlas
[SerializeField] private SpriteAtlas uiAtlas;
private Image image;

void Start()
{
    image = GetComponent<Image>();
    // 从Atlas中获取Sprite
    image.sprite = uiAtlas.GetSprite("button_normal");
    // 设置合适的Pixels Per Unit
    image.sprite.pixelsPerUnit = 100;
    // 控制是否需要接收点击事件
    image.raycastTarget = true;
}

// 纯色背景示例
void SetBackgroundColor()
{
    image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
}
```

#### 不好的做法：
```csharp
// 直接使用大尺寸图片
[SerializeField] private Sprite largeImage;
private Image image;

void Start()
{
    image = GetComponent<Image>();
    // 直接使用大尺寸图片
    image.sprite = largeImage;
    // 不设置Raycast Target
    image.raycastTarget = true;
}

// 使用图片实现纯色背景
void SetBackgroundColor()
{
    image.sprite = backgroundImage; // 使用图片而不是Color
}
```

### 1.2 Text组件
#### 基本用法：
```csharp
// 1. 获取Text组件
Text text = GetComponent<Text>();

// 2. 设置文本
text.text = "Hello World";

// 3. 设置字体
text.font = Resources.Load<Font>("Fonts/Arial");

// 4. 设置字体大小
text.fontSize = 24;

// 5. 设置字体样式
text.fontStyle = FontStyle.Bold;

// 6. 设置对齐方式
text.alignment = TextAnchor.MiddleCenter;

// 7. 设置颜色
text.color = Color.red;

// 8. 设置行间距
text.lineSpacing = 1.2f;

// 9. 设置是否自动换行
text.supportRichText = true;

// 10. 设置是否自动调整大小
text.resizeTextForBestFit = true;
text.resizeTextMinSize = 10;
text.resizeTextMaxSize = 40;
```

#### 好的做法：
```csharp
// 使用TextMeshPro
[SerializeField] private TextMeshProUGUI text;
private StringBuilder stringBuilder;

void Start()
{
    stringBuilder = new StringBuilder();
    // 使用富文本
    text.text = "<color=red>重要</color>提示：<b>请仔细阅读</b>";
    // 设置合适的行间距
    text.lineSpacing = 1.2f;
}

// 动态更新文本
void UpdateText(string content)
{
    stringBuilder.Clear();
    stringBuilder.Append(content);
    text.text = stringBuilder.ToString();
}
```

#### 不好的做法：
```csharp
// 使用旧版Text组件
[SerializeField] private Text text;

void Start()
{
    // 频繁更新文本
    InvokeRepeating("UpdateText", 0f, 0.1f);
}

void UpdateText()
{
    // 直接更新文本，不使用StringBuilder
    text.text = "当前时间：" + Time.time.ToString();
}
```

### 1.3 Button组件
#### 基本用法：
```csharp
// 1. 获取Button组件
Button button = GetComponent<Button>();

// 2. 添加点击事件监听
button.onClick.AddListener(OnButtonClick);

// 3. 设置按钮状态
button.interactable = true; // 是否可交互

// 4. 设置过渡效果
button.transition = Selectable.Transition.ColorTint;
button.colors = new ColorBlock
{
    normalColor = Color.white,
    highlightedColor = Color.yellow,
    pressedColor = Color.gray,
    disabledColor = Color.gray,
    colorMultiplier = 1f,
    fadeDuration = 0.1f
};

// 5. 设置导航
button.navigation = new Navigation
{
    mode = Navigation.Mode.Explicit,
    selectOnUp = upButton,
    selectOnDown = downButton,
    selectOnLeft = leftButton,
    selectOnRight = rightButton
};

// 6. 点击事件处理
void OnButtonClick()
{
    Debug.Log("Button clicked!");
}
```

#### 好的做法：
```csharp
public class ButtonManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // 使用事件系统
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => OnButtonClick(button));
        }
    }

    void OnButtonClick(Button button)
    {
        // 播放音效
        audioSource.PlayOneShot(clickSound);
        // 处理点击逻辑
        Debug.Log($"Button {button.name} clicked");
    }

    void OnDestroy()
    {
        // 清理事件监听
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
```

#### 不好的做法：
```csharp
public class BadButtonManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    void Update()
    {
        // 在Update中检测点击
        if (Input.GetMouseButtonDown(0))
        {
            foreach (var button in buttons)
            {
                if (IsMouseOverButton(button))
                {
                    OnButtonClick(button);
                }
            }
        }
    }

    bool IsMouseOverButton(Button button)
    {
        // 手动检测鼠标是否在按钮上
        Vector2 mousePosition = Input.mousePosition;
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition);
    }
}
```

### 1.4 RawImage组件
#### 基本用法：
```csharp
// 1. 获取RawImage组件
RawImage rawImage = GetComponent<RawImage>();

// 2. 设置纹理
rawImage.texture = Resources.Load<Texture>("UI/background");

// 3. 设置UV矩形
rawImage.uvRect = new Rect(0, 0, 1, 1);

// 4. 设置颜色
rawImage.color = new Color(1f, 1f, 1f, 0.5f); // 半透明

// 5. 设置材质
rawImage.material = new Material(Shader.Find("UI/Default"));

// 6. 设置Raycast Target
rawImage.raycastTarget = true; // 是否接收射线检测
```

#### 好的做法：
```csharp
// 使用RenderTexture
[SerializeField] private RawImage rawImage;
[SerializeField] private Camera renderCamera;
private RenderTexture renderTexture;

void Start()
{
    // 创建RenderTexture
    renderTexture = new RenderTexture(512, 512, 24);
    // 设置相机渲染目标
    renderCamera.targetTexture = renderTexture;
    // 设置RawImage显示renderTexture
    rawImage.texture = renderTexture;
}

void OnDestroy()
{
    // 释放资源
    if (renderTexture != null)
    {
        renderTexture.Release();
        Destroy(renderTexture);
    }
}
```

#### 不好的做法：
```csharp
// 不使用缓存
void Update()
{
    // 每帧都重新加载纹理
    RawImage rawImage = GetComponent<RawImage>();
    rawImage.texture = Resources.Load<Texture>("UI/background");
}
```

### 1.5 Toggle组件
#### 基本用法：
```csharp
// 1. 获取Toggle组件
Toggle toggle = GetComponent<Toggle>();

// 2. 设置初始状态
toggle.isOn = true;

// 3. 添加状态变化事件
toggle.onValueChanged.AddListener(OnToggleValueChanged);

// 4. 设置交互状态
toggle.interactable = true;

// 5. 设置开关组
toggle.group = toggleGroup;

// 6. 事件处理函数
void OnToggleValueChanged(bool isOn)
{
    Debug.Log($"Toggle value changed to {isOn}");
}
```

#### 好的做法：
```csharp
public class ToggleManager : MonoBehaviour
{
    [SerializeField] private Toggle[] toggles;
    [SerializeField] private ToggleGroup toggleGroup;

    void Start()
    {
        // 初始化ToggleGroup
        foreach (var toggle in toggles)
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }
        
        // 设置默认选中的Toggle
        toggles[0].isOn = true;
    }

    void OnToggleValueChanged(Toggle toggle, bool isOn)
    {
        if (isOn)
        {
            Debug.Log($"Toggle {toggle.name} is selected");
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
```

#### 不好的做法：
```csharp
public class BadToggleManager : MonoBehaviour
{
    [SerializeField] private Toggle[] toggles;
    private int selectedIndex = -1;

    void Update()
    {
        // 在Update中检查状态
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn && selectedIndex != i)
            {
                selectedIndex = i;
                OnToggleSelected(i);
            }
        }
    }

    void OnToggleSelected(int index)
    {
        // 手动处理互斥
        for (int i = 0; i < toggles.Length; i++)
        {
            if (i != index)
            {
                toggles[i].isOn = false;
            }
        }
    }
}
```

### 1.6 Slider组件
#### 基本用法：
```csharp
// 1. 获取Slider组件
Slider slider = GetComponent<Slider>();

// 2. 设置值范围
slider.minValue = 0f;
slider.maxValue = 100f;
slider.value = 50f;

// 3. 设置是否为整数
slider.wholeNumbers = true;

// 4. 添加值变化事件
slider.onValueChanged.AddListener(OnSliderValueChanged);

// 5. 设置方向
slider.direction = Slider.Direction.LeftToRight;

// 6. 事件处理函数
void OnSliderValueChanged(float value)
{
    Debug.Log($"Slider value changed to {value}");
}
```

#### 好的做法：
```csharp
public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        // 初始化滑块值
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.5f);
        
        // 更新显示
        UpdateVolumeDisplay(volumeSlider.value);
        
        // 添加事件监听
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float volume)
    {
        // 更新音量
        audioSource.volume = volume;
        
        // 更新显示
        UpdateVolumeDisplay(volume);
        
        // 保存设置
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    void UpdateVolumeDisplay(float volume)
    {
        valueText.text = $"{Mathf.RoundToInt(volume * 100)}%";
    }

    void OnDestroy()
    {
        // 清理事件监听
        volumeSlider.onValueChanged.RemoveAllListeners();
    }
}
```

#### 不好的做法：
```csharp
public class BadVolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    private float lastVolume;

    void Start()
    {
        lastVolume = volumeSlider.value;
    }

    void Update()
    {
        // 在Update中检测值变化
        if (lastVolume != volumeSlider.value)
        {
            OnVolumeChanged();
            lastVolume = volumeSlider.value;
        }
    }

    void OnVolumeChanged()
    {
        // 不使用事件系统
        Debug.Log($"Volume changed to {volumeSlider.value}");
    }
}
```

### 1.7 Scrollbar组件
#### 基本用法：
```csharp
// 1. 获取Scrollbar组件
Scrollbar scrollbar = GetComponent<Scrollbar>();

// 2. 设置值
scrollbar.value = 0.5f; // 0.0 ~ 1.0

// 3. 设置大小
scrollbar.size = 0.2f; // 滑块大小

// 4. 设置步长
scrollbar.numberOfSteps = 10;

// 5. 设置方向
scrollbar.direction = Scrollbar.Direction.LeftToRight;

// 6. 添加值变化事件
scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);

// 7. 事件处理函数
void OnScrollbarValueChanged(float value)
{
    Debug.Log($"Scrollbar value changed to {value}");
}
```

#### 好的做法：
```csharp
public class ScrollbarController : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private RectTransform viewportRectTransform;

    void Start()
    {
        // 初始化scrollbar
        scrollbar.value = 0f;
        scrollbar.onValueChanged.AddListener(OnScrollValueChanged);
        
        // 计算滑块大小
        UpdateScrollbarSize();
    }

    void UpdateScrollbarSize()
    {
        // 根据内容和视口计算滑块大小
        float contentSize = contentRectTransform.rect.height;
        float viewportSize = viewportRectTransform.rect.height;
        
        // 设置滑块大小比例
        if (contentSize > 0)
        {
            scrollbar.size = Mathf.Clamp01(viewportSize / contentSize);
        }
    }

    void OnScrollValueChanged(float value)
    {
        // 更新内容位置
        float contentHeight = contentRectTransform.rect.height;
        float viewportHeight = viewportRectTransform.rect.height;
        float scrollableDistance = contentHeight - viewportHeight;
        
        if (scrollableDistance > 0)
        {
            Vector2 anchoredPosition = contentRectTransform.anchoredPosition;
            anchoredPosition.y = value * scrollableDistance;
            contentRectTransform.anchoredPosition = anchoredPosition;
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        scrollbar.onValueChanged.RemoveAllListeners();
    }
}
```

#### 不好的做法：
```csharp
public class BadScrollbarController : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private RectTransform contentRectTransform;
    private float lastValue;

    void Start()
    {
        lastValue = scrollbar.value;
    }

    void Update()
    {
        // 在Update中检测值变化
        if (lastValue != scrollbar.value)
        {
            OnScrollValueChanged();
            lastValue = scrollbar.value;
        }
    }

    void OnScrollValueChanged()
    {
        // 直接设置位置而不考虑内容大小
        contentRectTransform.anchoredPosition = new Vector2(0, -scrollbar.value * 1000);
    }
}
```

### 1.8 Dropdown组件
#### 基本用法：
```csharp
// 1. 获取Dropdown组件
Dropdown dropdown = GetComponent<Dropdown>();

// 2. 清除选项
dropdown.options.Clear();

// 3. 添加选项
dropdown.options.Add(new Dropdown.OptionData("Option 1"));
dropdown.options.Add(new Dropdown.OptionData("Option 2"));
dropdown.options.Add(new Dropdown.OptionData("Option 3"));

// 4. 设置当前选中项
dropdown.value = 0;
dropdown.RefreshShownValue();

// 5. 添加选择事件
dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

// 6. 事件处理函数
void OnDropdownValueChanged(int index)
{
    Debug.Log($"Selected option index: {index}");
}
```

#### 好的做法：
```csharp
public class DropdownManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private string[] options;
    
    void Start()
    {
        // 初始化选项
        InitializeDropdown();
        
        // 添加事件监听
        dropdown.onValueChanged.AddListener(OnOptionSelected);
        
        // 加载上次选择
        int lastSelection = PlayerPrefs.GetInt("SelectedOption", 0);
        dropdown.value = Mathf.Clamp(lastSelection, 0, dropdown.options.Count - 1);
    }

    void InitializeDropdown()
    {
        dropdown.options.Clear();
        
        foreach (string option in options)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }
        
        dropdown.RefreshShownValue();
    }

    void OnOptionSelected(int index)
    {
        Debug.Log($"Selected: {options[index]}");
        
        // 保存选择
        PlayerPrefs.SetInt("SelectedOption", index);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        // 清理事件监听
        dropdown.onValueChanged.RemoveAllListeners();
    }
}
```

#### 不好的做法：
```csharp
public class BadDropdownManager : MonoBehaviour
{
    [SerializeField] private Dropdown dropdown;
    private int lastValue;

    void Start()
    {
        // 硬编码选项
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("Option 1"));
        dropdown.options.Add(new Dropdown.OptionData("Option 2"));
        dropdown.value = 0;
        
        lastValue = dropdown.value;
    }

    void Update()
    {
        // 在Update中检测值变化
        if (lastValue != dropdown.value)
        {
            OnDropdownValueChanged();
            lastValue = dropdown.value;
        }
    }

    void OnDropdownValueChanged()
    {
        // 不使用事件系统
        Debug.Log($"Selected option: {dropdown.value}");
    }
}
```

### 1.9 InputField组件
#### 基本用法：
```csharp
// 1. 获取InputField组件
InputField inputField = GetComponent<InputField>();
// 或者使用TMP_InputField
TMP_InputField tmpInputField = GetComponent<TMP_InputField>();

// 2. 设置文本
inputField.text = "默认文本";

// 3. 设置占位符文本
inputField.placeholder.GetComponent<Text>().text = "请输入...";

// 4. 设置输入类型
inputField.contentType = InputField.ContentType.Standard;
inputField.contentType = InputField.ContentType.Password; // 密码
inputField.contentType = InputField.ContentType.IntegerNumber; // 整数
inputField.contentType = InputField.ContentType.DecimalNumber; // 小数

// 5. 设置字符限制
inputField.characterLimit = 10;

// 6. 添加事件
inputField.onValueChanged.AddListener(OnInputValueChanged);
inputField.onEndEdit.AddListener(OnInputEndEdit);

// 7. 事件处理函数
void OnInputValueChanged(string text)
{
    Debug.Log($"Input value changed: {text}");
}

void OnInputEndEdit(string text)
{
    Debug.Log($"Input end edit: {text}");
}
```

#### 好的做法：
```csharp
public class LoginForm : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    
    void Start()
    {
        // 设置输入框属性
        usernameInput.contentType = TMP_InputField.ContentType.Name;
        usernameInput.characterLimit = 20;
        
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        passwordInput.characterLimit = 16;
        
        // 添加事件监听
        usernameInput.onValueChanged.AddListener(OnInputValueChanged);
        passwordInput.onValueChanged.AddListener(OnInputValueChanged);
        usernameInput.onEndEdit.AddListener(OnUsernameEndEdit);
        
        // 设置默认状态
        loginButton.interactable = false;
    }

    void OnInputValueChanged(string text)
    {
        // 验证表单是否可提交
        bool canSubmit = !string.IsNullOrEmpty(usernameInput.text) && 
                         !string.IsNullOrEmpty(passwordInput.text);
        
        loginButton.interactable = canSubmit;
    }

    void OnUsernameEndEdit(string username)
    {
        // 自动跳转到下一个输入框
        if (!string.IsNullOrEmpty(username))
        {
            passwordInput.Select();
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        usernameInput.onValueChanged.RemoveAllListeners();
        passwordInput.onValueChanged.RemoveAllListeners();
        usernameInput.onEndEdit.RemoveAllListeners();
    }
}
```

#### 不好的做法：
```csharp
public class BadLoginForm : MonoBehaviour
{
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    
    void Update()
    {
        // 在Update中检测输入变化
        bool canSubmit = !string.IsNullOrEmpty(usernameInput.text) && 
                        !string.IsNullOrEmpty(passwordInput.text);
        
        loginButton.interactable = canSubmit;
    }

    public void OnLoginClick()
    {
        // 不验证输入
        Debug.Log($"Login with username: {usernameInput.text}, password: {passwordInput.text}");
    }
}
```

### 1.10 Toggle Group组件
#### 基本用法：
```csharp
// 1. 获取ToggleGroup组件
ToggleGroup toggleGroup = GetComponent<ToggleGroup>();

// 2. 将Toggle添加到组
toggle1.group = toggleGroup;
toggle2.group = toggleGroup;
toggle3.group = toggleGroup;

// 3. 设置允许开关切换
toggleGroup.allowSwitchOff = false;

// 4. 获取当前选中的Toggle
Toggle activeToggle = toggleGroup.GetFirstActiveToggle();
```

#### 好的做法：
```csharp
public class TabGroup : MonoBehaviour
{
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private Toggle[] tabToggles;
    [SerializeField] private GameObject[] tabContents;
    
    void Start()
    {
        // 初始化Toggle Group
        toggleGroup.allowSwitchOff = false;
        
        // 设置所有Toggle的Group
        for (int i = 0; i < tabToggles.Length; i++)
        {
            int index = i; // 捕获索引
            tabToggles[i].group = toggleGroup;
            tabToggles[i].onValueChanged.AddListener((isOn) => OnTabToggleChanged(index, isOn));
        }
        
        // 默认选中第一个标签
        tabToggles[0].isOn = true;
        UpdateTabContents(0);
    }

    void OnTabToggleChanged(int index, bool isOn)
    {
        if (isOn)
        {
            UpdateTabContents(index);
        }
    }

    void UpdateTabContents(int activeIndex)
    {
        // 只显示选中的内容
        for (int i = 0; i < tabContents.Length; i++)
        {
            tabContents[i].SetActive(i == activeIndex);
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        foreach (Toggle toggle in tabToggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
```

#### 不好的做法：
```csharp
public class BadTabGroup : MonoBehaviour
{
    [SerializeField] private Toggle[] tabToggles;
    [SerializeField] private GameObject[] tabContents;
    private int currentTab = -1;
    
    void Update()
    {
        // 在Update中检测Toggle状态
        for (int i = 0; i < tabToggles.Length; i++)
        {
            if (tabToggles[i].isOn && currentTab != i)
            {
                currentTab = i;
                UpdateTabContents();
            }
        }
    }

    void UpdateTabContents()
    {
        // 手动处理互斥
        for (int i = 0; i < tabToggles.Length; i++)
        {
            if (i != currentTab)
            {
                tabToggles[i].isOn = false;
            }
            tabContents[i].SetActive(i == currentTab);
        }
    }
}
```

### 1.11 Canvas Group组件
#### 基本用法：
```csharp
// 1. 获取CanvasGroup组件
CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

// 2. 设置透明度
canvasGroup.alpha = 0.5f;

// 3. 设置是否可交互
canvasGroup.interactable = true;

// 4. 设置是否阻挡射线
canvasGroup.blocksRaycasts = true;

// 5. 设置是否忽略父物体
canvasGroup.ignoreParentGroups = false;
```

#### 好的做法：
```csharp
public class UIFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private Coroutine fadeCoroutine;
    
    void Start()
    {
        // 初始状态
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeIn()
    {
        // 中断正在进行的渐变
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeRoutine(1f, fadeInDuration, true));
    }

    public void FadeOut()
    {
        // 中断正在进行的渐变
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeRoutine(0f, fadeOutDuration, false));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool interactable)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;
        
        // 立即设置交互状态
        if (interactable)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        
        // 确保达到目标值
        canvasGroup.alpha = targetAlpha;
        
        // 如果是淡出，禁用交互
        if (!interactable)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        fadeCoroutine = null;
    }
}
```

#### 不好的做法：
```csharp
public class BadUIFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;
    private float fadeSpeed = 1f;
    
    void Update()
    {
        // 在Update中处理渐变
        if (canvasGroup.alpha != targetAlpha)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            
            // 不使用协程，逻辑分散
            if (Mathf.Approximately(canvasGroup.alpha, 0f))
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else if (Mathf.Approximately(canvasGroup.alpha, 1f))
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    public void SetTargetAlpha(float alpha)
    {
        targetAlpha = alpha;
    }
}
```

### 1.12 Mask组件
#### 基本用法：
```csharp
// 1. 获取Mask组件
Mask mask = GetComponent<Mask>();

// 2. 设置是否显示遮罩图像
mask.showMaskGraphic = true;
```

#### 好的做法：
```csharp
public class RoundedCornerMask : MonoBehaviour
{
    [SerializeField] private Mask mask;
    [SerializeField] private Image maskImage;
    
    void Start()
    {
        // 初始化遮罩
        mask.showMaskGraphic = true;
        
        // 设置圆角遮罩图像
        maskImage.sprite = GetRoundedCornerSprite();
    }

    private Sprite GetRoundedCornerSprite()
    {
        // 加载圆角Sprite
        return Resources.Load<Sprite>("UI/RoundedCorner");
    }
    
    // 动态调整圆角大小
    public void SetCornerRadius(float radius)
    {
        RectTransform rectTransform = maskImage.rectTransform;
        Vector2 size = rectTransform.sizeDelta;
        
        // 通过调整缩放来改变圆角大小
        maskImage.pixelsPerUnitMultiplier = radius;
    }
}
```

#### 不好的做法：
```csharp
public class BadMaskImplementation : MonoBehaviour
{
    [SerializeField] private GameObject content;
    
    void Start()
    {
        // 不使用Mask组件，而是尝试通过裁剪脚本实现
        ClipContent();
    }

    void ClipContent()
    {
        // 自定义裁剪逻辑不如使用内置Mask
        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect rect = rectTransform.rect;
        
        // 尝试手动裁剪内容（不正确的实现）
        // ...
    }
}
```

### 1.13 Rect Mask 2D组件
#### 基本用法：
```csharp
// 1. 获取RectMask2D组件
RectMask2D rectMask = GetComponent<RectMask2D>();

// 2. 设置内边距
rectMask.padding = new Vector4(10, 10, 10, 10); // 左下右上

// 3. 设置柔和边缘
rectMask.softness = new Vector2Int(10, 10);
```

#### 好的做法：
```csharp
public class OptimizedScrollView : MonoBehaviour
{
    [SerializeField] private RectMask2D viewportMask;
    [SerializeField] private ScrollRect scrollRect;
    
    void Start()
    {
        // 使用RectMask2D代替Mask提高性能
        viewportMask.padding = Vector4.zero;
        viewportMask.softness = Vector2Int.zero;
        
        // 配置ScrollRect
        scrollRect.viewport = viewportMask.rectTransform;
    }
    
    // 动态调整内边距
    public void SetMaskPadding(float padding)
    {
        viewportMask.padding = new Vector4(padding, padding, padding, padding);
    }
}
```

#### 不好的做法：
```csharp
public class BadScrollViewMask : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    
    void Start()
    {
        // 使用性能更差的Mask而不是RectMask2D
        Mask mask = scrollRect.viewport.gameObject.AddComponent<Mask>();
        
        // 添加不必要的Image组件作为遮罩图形
        Image maskImage = scrollRect.viewport.gameObject.AddComponent<Image>();
        maskImage.color = Color.white;
    }
}
```

### 1.14 Content Size Fitter组件
#### 基本用法：
```csharp
// 1. 获取ContentSizeFitter组件
ContentSizeFitter contentSizeFitter = GetComponent<ContentSizeFitter>();

// 2. 设置水平适配模式
contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

// 3. 设置垂直适配模式
contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
```

#### 好的做法：
```csharp
public class DynamicPanel : MonoBehaviour
{
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private TextMeshProUGUI contentText;
    
    void Start()
    {
        // 初始化ContentSizeFitter
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // 设置LayoutGroup
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
    }
    
    public void SetContent(string text)
    {
        // 更新文本内容
        contentText.text = text;
        
        // 强制重新计算布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
```

#### 不好的做法：
```csharp
public class BadDynamicPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contentText;
    
    void Start()
    {
        // 不使用ContentSizeFitter，而是手动计算大小
    }
    
    public void SetContent(string text)
    {
        contentText.text = text;
        
        // 手动计算和设置大小
        contentText.ForceMeshUpdate();
        Vector2 textSize = contentText.GetRenderedValues(false);
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, textSize.y + 40);
    }
}
```

### 1.15 Aspect Ratio Fitter组件
#### 基本用法：
```csharp
// 1. 获取AspectRatioFitter组件
AspectRatioFitter aspectRatioFitter = GetComponent<AspectRatioFitter>();

// 2. 设置纵横比
aspectRatioFitter.aspectRatio = 16f / 9f; // 宽高比

// 3. 设置适配模式
aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
```

#### 好的做法：
```csharp
public class VideoPlayer : MonoBehaviour
{
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    [SerializeField] private RawImage videoDisplay;
    
    void Start()
    {
        // 设置16:9的纵横比
        aspectRatioFitter.aspectRatio = 16f / 9f;
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    }
    
    public void SetVideoTexture(Texture videoTexture)
    {
        videoDisplay.texture = videoTexture;
        
        // 根据视频纵横比更新
        if (videoTexture != null)
        {
            float videoRatio = (float)videoTexture.width / videoTexture.height;
            aspectRatioFitter.aspectRatio = videoRatio;
        }
    }
}
```

#### 不好的做法：
```csharp
public class BadVideoPlayer : MonoBehaviour
{
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private RectTransform videoRectTransform;
    
    void Start()
    {
        // 手动设置纵横比
        videoRectTransform.sizeDelta = new Vector2(1280, 720);
    }
    
    void Update()
    {
        // 在Update中调整大小
        RectTransform parentRect = videoRectTransform.parent as RectTransform;
        float parentWidth = parentRect.rect.width;
        float targetHeight = parentWidth * (9f / 16f);
        
        videoRectTransform.sizeDelta = new Vector2(parentWidth, targetHeight);
    }
}
```

## 2. 设计理念与最佳实践

### 2.1 UI架构设计
```csharp
// 1. 使用MVC模式
public class UIModel
{
    public string Text { get; set; }
    public bool IsVisible { get; set; }
    public event System.Action OnDataChanged;

    public void UpdateData(string text)
    {
        Text = text;
        OnDataChanged?.Invoke();
    }
}

public class UIController : MonoBehaviour
{
    [SerializeField] private UIView view;
    private UIModel model;

    void Start()
    {
        model = new UIModel();
        view.Initialize(model);
    }

    void UpdateModel(string text)
    {
        model.UpdateData(text);
    }
}

public class UIView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private UIModel model;

    public void Initialize(UIModel model)
    {
        this.model = model;
        model.OnDataChanged += UpdateView;
    }

    public void UpdateView()
    {
        text.text = model.Text;
    }

    void OnDestroy()
    {
        if (model != null)
        {
            model.OnDataChanged -= UpdateView;
        }
    }
}

// 2. 使用事件系统
public class UIEventManager : MonoBehaviour
{
    private static UIEventManager instance;
    public static UIEventManager Instance => instance;

    private Dictionary<string, System.Action<object>> eventHandlers = new Dictionary<string, System.Action<object>>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEvent(string eventName, System.Action<object> handler)
    {
        if (!eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] = handler;
        }
        else
        {
            eventHandlers[eventName] += handler;
        }
    }

    public void UnregisterEvent(string eventName, System.Action<object> handler)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] -= handler;
        }
    }

    public void TriggerEvent(string eventName, object data = null)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName]?.Invoke(data);
        }
    }
}

// 3. 使用对象池
public class UIPoolManager : MonoBehaviour
{
    private static UIPoolManager instance;
    public static UIPoolManager Instance => instance;

    private Dictionary<string, ObjectPool<GameObject>> pools = new Dictionary<string, ObjectPool<GameObject>>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreatePool(string poolName, GameObject prefab, int initialSize = 10)
    {
        if (!pools.ContainsKey(poolName))
        {
            pools[poolName] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: (item) => item.SetActive(true),
                actionOnRelease: (item) => item.SetActive(false),
                actionOnDestroy: (item) => Destroy(item),
                defaultCapacity: initialSize
            );
        }
    }

    public GameObject GetFromPool(string poolName)
    {
        if (pools.ContainsKey(poolName))
        {
            return pools[poolName].Get();
        }
        return null;
    }

    public void ReturnToPool(string poolName, GameObject item)
    {
        if (pools.ContainsKey(poolName))
        {
            pools[poolName].Release(item);
        }
    }
}

// 4. 使用UI管理器
public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private CanvasGroup[] uiPanels;
    private Dictionary<string, CanvasGroup> panelDict = new Dictionary<string, CanvasGroup>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始化面板字典
        foreach (var panel in uiPanels)
        {
            panelDict[panel.name] = panel;
        }
    }

    public void ShowPanel(string panelName)
    {
        if (panelDict.ContainsKey(panelName))
        {
            var panel = panelDict[panelName];
            panel.alpha = 1f;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }
    }

    public void HidePanel(string panelName)
    {
        if (panelDict.ContainsKey(panelName))
        {
            var panel = panelDict[panelName];
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
    }

    public void ShowLoading()
    {
        // 显示加载界面
        ShowPanel("LoadingPanel");
    }

    public void HideLoading()
    {
        // 隐藏加载界面
        HidePanel("LoadingPanel");
    }
}
```

### 2.2 性能优化
```csharp
// 1. 使用虚拟列表
public class VirtualList : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int itemHeight = 100;
    [SerializeField] private int bufferCount = 2;

    private List<ItemData> allItems = new List<ItemData>();
    private List<GameObject> visibleItems = new List<GameObject>();
    private int firstVisibleIndex = 0;
    private int lastVisibleIndex = 0;

    void Start()
    {
        // 初始化数据
        for (int i = 0; i < 1000; i++)
        {
            allItems.Add(new ItemData { id = i, text = $"Item {i}" });
        }

        // 设置ScrollRect
        scrollRect.onValueChanged.AddListener(OnScroll);
        
        // 初始化可见项
        UpdateVisibleItems();
    }

    void OnScroll(Vector2 position)
    {
        UpdateVisibleItems();
    }

    void UpdateVisibleItems()
    {
        // 计算可见范围
        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = allItems.Count * itemHeight;
        float scrollPosition = scrollRect.content.anchoredPosition.y;

        int firstIndex = Mathf.FloorToInt(scrollPosition / itemHeight);
        int lastIndex = Mathf.CeilToInt((scrollPosition + viewportHeight) / itemHeight);

        // 添加缓冲区
        firstIndex = Mathf.Max(0, firstIndex - bufferCount);
        lastIndex = Mathf.Min(allItems.Count - 1, lastIndex + bufferCount);

        // 更新可见项
        if (firstIndex != firstVisibleIndex || lastIndex != lastVisibleIndex)
        {
            firstVisibleIndex = firstIndex;
            lastVisibleIndex = lastIndex;
            UpdateItems();
        }
    }

    void UpdateItems()
    {
        // 回收不需要的项目
        for (int i = visibleItems.Count - 1; i >= 0; i--)
        {
            var item = visibleItems[i];
            int itemIndex = int.Parse(item.name);
            if (itemIndex < firstVisibleIndex || itemIndex > lastVisibleIndex)
            {
                UIPoolManager.Instance.ReturnToPool("ListItem", item);
                visibleItems.RemoveAt(i);
            }
        }

        // 添加新的项目
        for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
        {
            if (!visibleItems.Exists(item => int.Parse(item.name) == i))
            {
                var item = UIPoolManager.Instance.GetFromPool("ListItem");
                item.name = i.ToString();
                item.transform.SetParent(scrollRect.content, false);
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * itemHeight);
                item.GetComponent<TextMeshProUGUI>().text = allItems[i].text;
                visibleItems.Add(item);
            }
        }
    }
}

// 2. 使用对象池
public class ObjectPool<T>
{
    private readonly Func<T> createFunc;
    private readonly Action<T> actionOnGet;
    private readonly Action<T> actionOnRelease;
    private readonly Action<T> actionOnDestroy;
    private readonly int maxSize;

    private readonly Stack<T> objects;
    private readonly HashSet<T> activeObjects;

    public ObjectPool(
        Func<T> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        int defaultCapacity = 10,
        int maxSize = 100)
    {
        this.createFunc = createFunc;
        this.actionOnGet = actionOnGet;
        this.actionOnRelease = actionOnRelease;
        this.actionOnDestroy = actionOnDestroy;
        this.maxSize = maxSize;

        objects = new Stack<T>(defaultCapacity);
        activeObjects = new HashSet<T>();
    }

    public T Get()
    {
        T item;
        if (objects.Count > 0)
        {
            item = objects.Pop();
        }
        else
        {
            item = createFunc();
        }

        actionOnGet?.Invoke(item);
        activeObjects.Add(item);
        return item;
    }

    public void Release(T item)
    {
        if (activeObjects.Remove(item))
        {
            if (objects.Count < maxSize)
            {
                actionOnRelease?.Invoke(item);
                objects.Push(item);
            }
            else
            {
                actionOnDestroy?.Invoke(item);
            }
        }
    }

    public void Clear()
    {
        foreach (var item in objects)
        {
            actionOnDestroy?.Invoke(item);
        }
        objects.Clear();
        activeObjects.Clear();
    }
}

// 3. 使用缓存
public class UICache : MonoBehaviour
{
    private static UICache instance;
    public static UICache Instance => instance;

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetSprite(string path)
    {
        if (!spriteCache.ContainsKey(path))
        {
            spriteCache[path] = Resources.Load<Sprite>(path);
        }
        return spriteCache[path];
    }

    public GameObject GetPrefab(string path)
    {
        if (!prefabCache.ContainsKey(path))
        {
            prefabCache[path] = Resources.Load<GameObject>(path);
        }
        return prefabCache[path];
    }

    public void ClearCache()
    {
        spriteCache.Clear();
        prefabCache.Clear();
    }
}
```

### 2.3 资源管理
```csharp
// 1. 使用Sprite Atlas
public class SpriteAtlasManager : MonoBehaviour
{
    [SerializeField] private SpriteAtlas uiAtlas;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Start()
    {
        // 预加载常用Sprite
        string[] spriteNames = { "button_normal", "button_pressed", "icon_1", "icon_2" };
        foreach (var name in spriteNames)
        {
            spriteCache[name] = uiAtlas.GetSprite(name);
        }
    }

    public Sprite GetSprite(string name)
    {
        if (!spriteCache.ContainsKey(name))
        {
            spriteCache[name] = uiAtlas.GetSprite(name);
        }
        return spriteCache[name];
    }

    void OnDestroy()
    {
        spriteCache.Clear();
    }
}

// 2. 使用资源加载器
public class ResourceLoader : MonoBehaviour
{
    private static ResourceLoader instance;
    public static ResourceLoader Instance => instance;

    private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string key = $"{typeof(T).Name}_{path}";
        if (!resourceCache.ContainsKey(key))
        {
            resourceCache[key] = Resources.Load<T>(path);
        }
        return resourceCache[key] as T;
    }

    public void ClearCache()
    {
        resourceCache.Clear();
    }
}
```

### 2.4 事件系统
```csharp
// 1. 使用事件管理器
public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    public static EventManager Instance => instance;

    private Dictionary<string, System.Action<object>> eventHandlers = new Dictionary<string, System.Action<object>>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEvent(string eventName, System.Action<object> handler)
    {
        if (!eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] = handler;
        }
        else
        {
            eventHandlers[eventName] += handler;
        }
    }

    public void UnregisterEvent(string eventName, System.Action<object> handler)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] -= handler;
        }
    }

    public void TriggerEvent(string eventName, object data = null)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName]?.Invoke(data);
        }
    }
}

// 2. 使用事件接口
public interface IUIEventHandler
{
    void OnShow();
    void OnHide();
    void OnUpdate();
}

public class UIBase : MonoBehaviour, IUIEventHandler
{
    public virtual void OnShow()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnHide()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnUpdate()
    {
        // 子类实现
    }
}

// 3. 使用事件委托
public class UIEventDelegate : MonoBehaviour
{
    public event System.Action OnShow;
    public event System.Action OnHide;
    public event System.Action OnUpdate;

    public void Show()
    {
        OnShow?.Invoke();
    }

    public void Hide()
    {
        OnHide?.Invoke();
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }
}
```

## 3. 布局组件

### 3.1 Layout Group
#### 好的做法：
```csharp
public class LayoutManager : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup verticalLayout;
    [SerializeField] private ContentSizeFitter contentSizeFitter;

    void Start()
    {
        // 设置合适的间距
        verticalLayout.spacing = 10f;
        verticalLayout.padding = new RectOffset(10, 10, 10, 10);
        
        // 使用ContentSizeFitter
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // 动态添加元素
    void AddElement(GameObject element)
    {
        element.transform.SetParent(transform, false);
        // Layout会自动处理排列
    }
}
```

#### 不好的做法：
```csharp
public class BadLayoutManager : MonoBehaviour
{
    [SerializeField] private GameObject[] elements;
    private float currentY = 0f;

    void Start()
    {
        // 手动设置每个元素的位置
        foreach (var element in elements)
        {
            RectTransform rectTransform = element.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, currentY);
            currentY -= rectTransform.rect.height + 10f;
        }
    }
}
```

### 3.2 Scroll View
#### 好的做法：
```csharp
public class ScrollViewManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject itemPrefab;
    private ObjectPool<GameObject> itemPool;
    private List<GameObject> activeItems = new List<GameObject>();

    void Start()
    {
        // 创建对象池
        itemPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(itemPrefab),
            actionOnGet: (item) => item.SetActive(true),
            actionOnRelease: (item) => item.SetActive(false),
            actionOnDestroy: (item) => Destroy(item),
            defaultCapacity: 20
        );

        // 设置ScrollRect
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    void OnScroll(Vector2 position)
    {
        // 实现虚拟列表
        UpdateVisibleItems();
    }

    void UpdateVisibleItems()
    {
        // 根据滚动位置更新可见项
        // 实现虚拟列表逻辑
    }
}
```

#### 不好的做法：
```csharp
public class BadScrollViewManager : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform content;
    private List<GameObject> allItems = new List<GameObject>();

    void Start()
    {
        // 一次性加载所有内容
        for (int i = 0; i < 1000; i++)
        {
            GameObject item = Instantiate(itemPrefab, content);
            allItems.Add(item);
        }
    }
}
```

## 4. 画布管理

### 4.1 Canvas Group
#### 好的做法：
```csharp
public class CanvasManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] canvasGroups;
    [SerializeField] private CanvasScaler canvasScaler;

    void Start()
    {
        // 设置Canvas Scaler
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;

        // 使用Canvas Group控制显示
        foreach (var group in canvasGroups)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    void ShowUI(string uiName)
    {
        // 使用Canvas Group控制UI显示
        CanvasGroup targetGroup = canvasGroups.FirstOrDefault(g => g.name == uiName);
        if (targetGroup != null)
        {
            targetGroup.alpha = 1f;
            targetGroup.interactable = true;
            targetGroup.blocksRaycasts = true;
        }
    }
}
```

#### 不好的做法：
```csharp
public class BadCanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject[] uiPanels;

    void Start()
    {
        // 创建多个Canvas
        foreach (var panel in uiPanels)
        {
            Canvas canvas = panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            panel.AddComponent<GraphicRaycaster>();
        }
    }

    void ShowUI(string uiName)
    {
        // 直接设置GameObject的激活状态
        foreach (var panel in uiPanels)
        {
            panel.SetActive(panel.name == uiName);
        }
    }
}
```

## 5. 常见问题解决

### 5.1 点击穿透
```csharp
public class ClickThroughManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup topPanel;
    [SerializeField] private CanvasGroup bottomPanel;

    void Start()
    {
        // 使用Canvas Group控制点击穿透
        topPanel.blocksRaycasts = true;
        bottomPanel.blocksRaycasts = false;
    }

    void ToggleClickThrough(bool allowClickThrough)
    {
        // 控制点击穿透
        topPanel.blocksRaycasts = !allowClickThrough;
        bottomPanel.blocksRaycasts = allowClickThrough;
    }
}
```

### 5.2 性能问题
```csharp
public class PerformanceManager : MonoBehaviour
{
    private ObjectPool<GameObject> itemPool;
    private List<GameObject> activeItems = new List<GameObject>();

    void Start()
    {
        // 使用对象池
        itemPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(itemPrefab),
            actionOnGet: (item) => item.SetActive(true),
            actionOnRelease: (item) => item.SetActive(false),
            actionOnDestroy: (item) => Destroy(item),
            defaultCapacity: 20
        );
    }

    void UpdateItems()
    {
        // 回收不需要的项目
        foreach (var item in activeItems.ToList())
        {
            if (!IsItemVisible(item))
            {
                itemPool.Release(item);
                activeItems.Remove(item);
            }
        }
    }
}
```

### 5.3 适配问题
```csharp
public class UIScaler : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform[] uiElements;

    void Start()
    {
        // 设置Canvas Scaler
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;

        // 设置UI元素的锚点
        foreach (var element in uiElements)
        {
            // 设置合适的锚点
            element.anchorMin = new Vector2(0.5f, 0.5f);
            element.anchorMax = new Vector2(0.5f, 0.5f);
            element.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
```

## 6. 最佳实践总结

### 6.1 使用合适的组件
```csharp
public class UIComponentManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private CanvasGroup canvasGroup;

    void Start()
    {
        // 使用TextMeshPro
        text.text = "使用TextMeshPro";
        
        // 使用Layout Group
        layoutGroup.spacing = 10f;
        
        // 使用Canvas Group
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }
}
```

### 6.2 性能优化
```csharp
public class PerformanceOptimizer : MonoBehaviour
{
    private ObjectPool<GameObject> itemPool;
    private List<GameObject> activeItems = new List<GameObject>();

    void Start()
    {
        // 使用对象池
        itemPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(itemPrefab),
            actionOnGet: (item) => item.SetActive(true),
            actionOnRelease: (item) => item.SetActive(false),
            actionOnDestroy: (item) => Destroy(item),
            defaultCapacity: 20
        );
    }

    void Update()
    {
        // 避免频繁重建
        if (needsUpdate)
        {
            UpdateUI();
            needsUpdate = false;
        }
    }
}
```

### 6.3 代码组织
```csharp
// MVC模式示例
public class UIModel
{
    public string Text { get; set; }
    public bool IsVisible { get; set; }
}

public class UIController : MonoBehaviour
{
    [SerializeField] private UIView view;
    private UIModel model;

    void Start()
    {
        model = new UIModel();
        view.Initialize(model);
    }

    void UpdateModel(string text)
    {
        model.Text = text;
        view.UpdateView();
    }
}

public class UIView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private UIModel model;

    public void Initialize(UIModel model)
    {
        this.model = model;
    }

    public void UpdateView()
    {
        text.text = model.Text;
    }
}
```

### 6.4 资源管理
```csharp
public class ResourceManager : MonoBehaviour
{
    [SerializeField] private SpriteAtlas uiAtlas;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Start()
    {
        // 使用Sprite Atlas
        LoadSprites();
    }

    void LoadSprites()
    {
        // 预加载常用Sprite
        string[] spriteNames = { "button_normal", "button_pressed", "icon_1", "icon_2" };
        foreach (var name in spriteNames)
        {
            spriteCache[name] = uiAtlas.GetSprite(name);
        }
    }

    void OnDestroy()
    {
        // 清理资源
        spriteCache.Clear();
    }
}
``` 