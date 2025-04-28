using System;
using UIFramework.MVVM;
using UIFramework.MVVM.Signals;

namespace UIFramework.Example.MVVM
{
    /// <summary>
    /// 登录面板 ViewModel
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        // 用户名
        private string username;
        public string Username
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        // 密码
        private string password;
        public string Password
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        // 错误信息
        private string errorMessage;
        public string ErrorMessage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        // 是否记住用户名
        private bool rememberUsername;
        public bool RememberUsername
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        // 是否正在登录
        private bool isLoggingIn;
        public bool IsLoggingIn
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        // 登录命令
        private ICommand loginCommand;
        public ICommand LoginCommand
        {
            get => GetProperty<ICommand>();
            set => SetProperty(value);
        }

        // 注册命令
        private ICommand registerCommand;
        public ICommand RegisterCommand
        {
            get => GetProperty<ICommand>();
            set => SetProperty(value);
        }

        // 信号
        public static readonly Signal<string> LoginSuccessSignal = new Signal<string>();
        public static readonly Signal<string> LoginFailedSignal = new Signal<string>();

        // 常量
        private const string REMEMBER_USERNAME_KEY = "RememberUsername";
        private const string USERNAME_KEY = "Username";

        /// <summary>
        /// 构造函数
        /// </summary>
        public LoginViewModel()
        {
            // 初始化命令
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);

            // 加载记住的用户名
            LoadRememberedUsername();
        }

        /// <summary>
        /// 加载记住的用户名
        /// </summary>
        private void LoadRememberedUsername()
        {
            RememberUsername = PlayerPrefs.GetInt(REMEMBER_USERNAME_KEY, 0) == 1;
            if (RememberUsername)
            {
                Username = PlayerPrefs.GetString(USERNAME_KEY, "");
            }
        }

        /// <summary>
        /// 保存记住的用户名
        /// </summary>
        public void SaveRememberedUsername()
        {
            PlayerPrefs.SetInt(REMEMBER_USERNAME_KEY, RememberUsername ? 1 : 0);
            if (RememberUsername && !string.IsNullOrEmpty(Username))
            {
                PlayerPrefs.SetString(USERNAME_KEY, Username);
            }
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 是否可以执行登录
        /// </summary>
        private bool CanExecuteLogin(object parameter)
        {
            return !IsLoggingIn && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        /// <summary>
        /// 执行登录
        /// </summary>
        private void ExecuteLogin(object parameter)
        {
            if (!CanExecuteLogin(parameter))
            {
                return;
            }

            // 设置登录状态
            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            // 模拟登录
            System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
            {
                // 简单验证：admin/admin 或记住的用户名/任意密码
                bool isValid = (Username == "admin" && Password == "admin") ||
                              (Username == PlayerPrefs.GetString(USERNAME_KEY) && !string.IsNullOrEmpty(Password));

                if (isValid)
                {
                    // 触发登录成功信号
                    LoginSuccessSignal.Emit(Username);
                }
                else
                {
                    // 触发登录失败信号
                    LoginFailedSignal.Emit("用户名或密码错误");
                }

                // 重置登录状态
                IsLoggingIn = false;
            });
        }

        /// <summary>
        /// 执行注册
        /// </summary>
        private void ExecuteRegister(object parameter)
        {
            // 打开注册面板
            UIManager.Instance.OpenPanel("RegisterPanel");
        }
    }
} 