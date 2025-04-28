using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

/// <summary>
/// 登录面板示例，展示UI框架的基本使用方法
/// </summary>
public class LoginPanel : UIPanel
{
    // UI组件引用
    [TwoWayBinding("Username")]
    private InputField usernameInput;
    private InputField passwordInput;
    [CommandBinding("LoginCommand", true)]
    [SignalBinding("LoginSuccess")]
    private Button loginButton;
    private Button registerButton;
    private Text errorText;
    private Toggle rememberToggle;
    [OneWayBinding("Title")]
    private Text titleText;

    // 数据
    private string lastUsername;
    private bool isLoggingIn;

    // 常量
    private const string REMEMBER_USERNAME_KEY = "RememberUsername";
    private const string USERNAME_KEY = "Username";
    
    // 事件名称
    public const string EVENT_LOGIN_SUCCESS = "LoginSuccess";
    public const string EVENT_LOGIN_FAILED = "LoginFailed";

    public static readonly Signal<string> LoginSuccessSignal = new Signal<string>();
    private IDisposable subscription;

    /// <summary>
    /// 面板初始化
    /// </summary>
    /// <param name="panelName">面板名称</param>
    /// <param name="layer">面板层级</param>
    public override void Initialize(string panelName, UIManager.UILayer layer)
    {
        base.Initialize(panelName, layer);
        
        // 获取UI组件引用
        usernameInput = GetInputField("UsernameInput");
        passwordInput = GetInputField("PasswordInput");
        loginButton = GetButton("LoginButton", OnLoginClick);
        registerButton = GetButton("RegisterButton", OnRegisterClick);
        errorText = GetText("ErrorText");
        rememberToggle = GetToggle("RememberToggle");
        
        // 隐藏错误文本
        errorText.gameObject.SetActive(false);
        
        // 添加输入框事件监听
        if (usernameInput != null)
        {
            usernameInput.onValueChanged.AddListener(OnInputChanged);
        }
        
        if (passwordInput != null)
        {
            passwordInput.onValueChanged.AddListener(OnInputChanged);
            passwordInput.contentType = InputField.ContentType.Password;
        }
        
        // 添加全局事件监听
        AddEventListener(EVENT_LOGIN_SUCCESS, OnLoginSuccess);
        AddEventListener(EVENT_LOGIN_FAILED, OnLoginFailed);
    }

    /// <summary>
    /// 面板打开
    /// </summary>
    /// <param name="args">参数</param>
    public override void OnOpen(params object[] args)
    {
        base.OnOpen(args);
        
        // 加载上次记住的用户名
        bool rememberUsername = PlayerPrefs.GetInt(REMEMBER_USERNAME_KEY, 0) == 1;
        if (rememberToggle != null)
        {
            rememberToggle.isOn = rememberUsername;
        }
        
        if (rememberUsername && usernameInput != null)
        {
            usernameInput.text = PlayerPrefs.GetString(USERNAME_KEY, "");
        }
        
        // 清空密码
        if (passwordInput != null)
        {
            passwordInput.text = "";
        }
        
        // 隐藏错误信息
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
        
        // 重置登录状态
        isLoggingIn = false;
        
        // 更新按钮状态
        UpdateButtonState();
        
        subscription = LoginSuccessSignal.Subscribe(username => {
            // 处理登录成功
        });
    }

    /// <summary>
    /// 面板关闭
    /// </summary>
    public override void OnClose()
    {
        // 记住用户名设置
        if (rememberToggle != null)
        {
            PlayerPrefs.SetInt(REMEMBER_USERNAME_KEY, rememberToggle.isOn ? 1 : 0);
            
            if (rememberToggle.isOn && usernameInput != null && !string.IsNullOrEmpty(usernameInput.text))
            {
                PlayerPrefs.SetString(USERNAME_KEY, usernameInput.text);
            }
            
            PlayerPrefs.Save();
        }
        
        // 取消登录
        isLoggingIn = false;
        
        subscription.Dispose();
        base.OnClose();
    }

    /// <summary>
    /// 登录按钮点击事件
    /// </summary>
    private void OnLoginClick()
    {
        if (isLoggingIn || !IsInputValid())
            return;
        
        // 隐藏错误信息
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
        
        // 设置登录状态
        isLoggingIn = true;
        
        // 更新按钮状态
        UpdateButtonState();
        
        // 模拟登录
        StartCoroutine(SimulateLogin());
    }

    /// <summary>
    /// 注册按钮点击事件
    /// </summary>
    private void OnRegisterClick()
    {
        // 打开注册面板
        UIManager.Instance.OpenPanel("RegisterPanel");
    }

    /// <summary>
    /// 输入框内容变化事件
    /// </summary>
    /// <param name="value">输入内容</param>
    private void OnInputChanged(string value)
    {
        // 更新按钮状态
        UpdateButtonState();
    }

    /// <summary>
    /// 登录成功事件处理
    /// </summary>
    /// <param name="data">用户数据</param>
    private void OnLoginSuccess(object data)
    {
        string username = data as string;
        if (!string.IsNullOrEmpty(username))
        {
            lastUsername = username;
        }
        
        // 关闭登录面板
        UIManager.Instance.ClosePanel(PanelName);
        
        // 打开主界面
        UIManager.Instance.OpenPanel("MainPanel", null, lastUsername);
        
        LoginSuccessSignal.Emit(username);
    }

    /// <summary>
    /// 登录失败事件处理
    /// </summary>
    /// <param name="data">错误信息</param>
    private void OnLoginFailed(object data)
    {
        string errorMessage = data as string;
        
        // 显示错误信息
        if (errorText != null)
        {
            errorText.text = string.IsNullOrEmpty(errorMessage) ? "登录失败，请重试" : errorMessage;
            errorText.gameObject.SetActive(true);
        }
        
        // 重置登录状态
        isLoggingIn = false;
        
        // 更新按钮状态
        UpdateButtonState();
    }

    /// <summary>
    /// 检查输入是否有效
    /// </summary>
    /// <returns>输入是否有效</returns>
    private bool IsInputValid()
    {
        // 检查用户名
        if (usernameInput == null || string.IsNullOrEmpty(usernameInput.text))
        {
            if (errorText != null)
            {
                errorText.text = "请输入用户名";
                errorText.gameObject.SetActive(true);
            }
            return false;
        }
        
        // 检查密码
        if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
        {
            if (errorText != null)
            {
                errorText.text = "请输入密码";
                errorText.gameObject.SetActive(true);
            }
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonState()
    {
        if (loginButton != null)
        {
            // 登录中或输入为空时禁用按钮
            loginButton.interactable = !isLoggingIn && 
                                       usernameInput != null && !string.IsNullOrEmpty(usernameInput.text) &&
                                       passwordInput != null && !string.IsNullOrEmpty(passwordInput.text);
        }
    }

    /// <summary>
    /// 模拟登录过程
    /// </summary>
    private IEnumerator SimulateLogin()
    {
        // 模拟网络延迟
        yield return new WaitForSeconds(1.5f);
        
        // 简单验证：admin/admin 或记住的用户名/任意密码
        string username = usernameInput?.text ?? "";
        string password = passwordInput?.text ?? "";
        
        bool isValid = (username == "admin" && password == "admin") || 
                      (username == lastUsername && !string.IsNullOrEmpty(password));
        
        if (isValid)
        {
            // 触发登录成功事件
            SendEvent(EVENT_LOGIN_SUCCESS, username);
        }
        else
        {
            // 触发登录失败事件
            SendEvent(EVENT_LOGIN_FAILED, "用户名或密码错误");
        }
    }
} 