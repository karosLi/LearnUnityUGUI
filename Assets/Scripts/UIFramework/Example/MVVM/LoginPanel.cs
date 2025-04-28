using UnityEngine;
using UnityEngine.UI;
using UIFramework.MVVM;
using UIFramework.MVVM.Bindings;
using UIFramework.MVVM.Signals;
using System;

namespace UIFramework.Example.MVVM
{
    /// <summary>
    /// 登录面板示例，使用 MVVM 模式
    /// </summary>
    public class LoginPanel : MVVMPanel<LoginViewModel>
    {
        // UI组件引用
        [TwoWayBinding("Username")]
        private InputField usernameInput;

        [TwoWayBinding("Password")]
        private InputField passwordInput;

        [CommandBinding("LoginCommand", true)]
        private Button loginButton;

        [CommandBinding("RegisterCommand")]
        private Button registerButton;

        [OneWayBinding("ErrorMessage")]
        private Text errorText;

        [TwoWayBinding("RememberUsername")]
        private Toggle rememberToggle;

        // 订阅令牌
        private IDisposable loginSuccessSubscription;
        private IDisposable loginFailedSubscription;

        /// <summary>
        /// 初始化面板
        /// </summary>
        public override void Initialize(string panelName, UIManager.UILayer layer)
        {
            base.Initialize(panelName, layer);
            
            // 获取UI组件引用
            usernameInput = GetInputField("UsernameInput");
            passwordInput = GetInputField("PasswordInput");
            loginButton = GetButton("LoginButton", null);
            registerButton = GetButton("RegisterButton", null);
            errorText = GetText("ErrorText");
            rememberToggle = GetToggle("RememberToggle");
            
            // 隐藏错误文本
            errorText.gameObject.SetActive(false);
            
            // 设置密码输入框类型
            if (passwordInput != null)
            {
                passwordInput.contentType = InputField.ContentType.Password;
            }
            
            // 订阅信号
            loginSuccessSubscription = LoginViewModel.LoginSuccessSignal.Subscribe(OnLoginSuccess);
            loginFailedSubscription = LoginViewModel.LoginFailedSignal.Subscribe(OnLoginFailed);
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            
            // 清空密码
            ViewModel.Password = string.Empty;
            
            // 隐藏错误信息
            ViewModel.ErrorMessage = string.Empty;
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public override void OnClose()
        {
            // 保存记住的用户名设置
            ViewModel.SaveRememberedUsername();
            
            // 取消订阅信号
            loginSuccessSubscription?.Dispose();
            loginFailedSubscription?.Dispose();
            
            base.OnClose();
        }

        /// <summary>
        /// 登录成功处理
        /// </summary>
        private void OnLoginSuccess(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                // 关闭登录面板
                UIManager.Instance.ClosePanel(PanelName);
                
                // 打开主界面
                UIManager.Instance.OpenPanel("MainPanel", null, username);
            }
        }

        /// <summary>
        /// 登录失败处理
        /// </summary>
        private void OnLoginFailed(string errorMessage)
        {
            ViewModel.ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "登录失败，请重试" : errorMessage;
        }
    }
} 