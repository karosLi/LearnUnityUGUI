using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UIFramework.MVVM.Bindings;
using UIFramework.MVVM.Signals;

namespace UIFramework.MVVM
{
    /// <summary>
    /// MVVM 面板基类，支持 ViewModel 绑定
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel 类型</typeparam>
    public abstract class MVVMPanel<TViewModel> : UIPanel where TViewModel : ViewModelBase, new()
    {
        // ViewModel 实例
        protected TViewModel ViewModel { get; private set; }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public override void Initialize(string panelName, UIManager.UILayer layer)
        {
            base.Initialize(panelName, layer);
            
            // 创建 ViewModel
            ViewModel = new TViewModel();
            
            // 绑定 ViewModel
            BindViewModel();
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            
            // 更新 ViewModel
            UpdateViewModel(args);
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public override void OnClose()
        {
            // 清理 ViewModel
            if (ViewModel != null)
            {
                ViewModel.Dispose();
                ViewModel = null;
            }
            
            base.OnClose();
        }

        /// <summary>
        /// 绑定 ViewModel
        /// </summary>
        protected virtual void BindViewModel()
        {
            // 获取所有字段
            var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                // 获取绑定注解
                var bindingAttr = field.GetCustomAttribute<BindingAttribute>();
                if (bindingAttr == null)
                {
                    continue;
                }

                // 获取字段值
                var component = field.GetValue(this);
                if (component == null)
                {
                    continue;
                }

                // 根据注解类型进行绑定
                switch (bindingAttr)
                {
                    case OneWayBindingAttribute oneWay:
                        BindOneWay(component, oneWay.PropertyName);
                        break;
                    case TwoWayBindingAttribute twoWay:
                        BindTwoWay(component, twoWay.PropertyName);
                        break;
                    case CommandBindingAttribute command:
                        BindCommand(component, command.PropertyName, command.BindEnabled);
                        break;
                    case SignalBindingAttribute signal:
                        BindSignal(component, signal.SignalName);
                        break;
                }
            }
        }

        /// <summary>
        /// 更新 ViewModel
        /// </summary>
        /// <param name="args">参数</param>
        protected virtual void UpdateViewModel(params object[] args)
        {
            // 子类实现具体的更新逻辑
        }

        /// <summary>
        /// 单向绑定
        /// </summary>
        private void BindOneWay(object component, string propertyName)
        {
            if (component is Text text)
            {
                BindText(text, propertyName);
            }
            else if (component is Image image)
            {
                BindImage(image, propertyName);
            }
        }

        /// <summary>
        /// 双向绑定
        /// </summary>
        private void BindTwoWay(object component, string propertyName)
        {
            if (component is InputField inputField)
            {
                BindInputField(inputField, propertyName);
            }
            else if (component is Toggle toggle)
            {
                BindToggle(toggle, propertyName);
            }
        }

        /// <summary>
        /// 命令绑定
        /// </summary>
        private void BindCommand(object component, string propertyName, bool bindEnabled)
        {
            if (component is Button button)
            {
                BindButton(button, propertyName, bindEnabled ? $"{propertyName}Enabled" : null);
            }
        }

        /// <summary>
        /// 信号绑定
        /// </summary>
        private void BindSignal(object component, string signalName)
        {
            if (component is Button button)
            {
                var signal = Signal.Get<Signal>(signalName);
                button.onClick.AddListener(() => signal.Emit());
            }
        }

        /// <summary>
        /// 绑定文本
        /// </summary>
        private void BindText(Text text, string propertyName)
        {
            if (text == null || ViewModel == null)
            {
                return;
            }

            // 设置初始值
            text.text = ViewModel.GetProperty<string>(propertyName) ?? string.Empty;

            // 监听属性变更
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    text.text = ViewModel.GetProperty<string>(propertyName) ?? string.Empty;
                }
            };
        }

        /// <summary>
        /// 绑定图片
        /// </summary>
        private void BindImage(Image image, string propertyName)
        {
            if (image == null || ViewModel == null)
            {
                return;
            }

            // 设置初始值
            image.sprite = ViewModel.GetProperty<Sprite>(propertyName);

            // 监听属性变更
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    image.sprite = ViewModel.GetProperty<Sprite>(propertyName);
                }
            };
        }

        /// <summary>
        /// 绑定按钮
        /// </summary>
        private void BindButton(Button button, string commandPropertyName, string enabledPropertyName = null)
        {
            if (button == null || ViewModel == null)
            {
                return;
            }

            // 设置初始状态
            if (!string.IsNullOrEmpty(enabledPropertyName))
            {
                button.interactable = ViewModel.GetProperty<bool>(enabledPropertyName);
            }

            // 监听启用状态变更
            if (!string.IsNullOrEmpty(enabledPropertyName))
            {
                ViewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == enabledPropertyName)
                    {
                        button.interactable = ViewModel.GetProperty<bool>(enabledPropertyName);
                    }
                };
            }

            // 获取命令
            var command = ViewModel.GetProperty<ICommand>(commandPropertyName);
            if (command != null)
            {
                // 添加点击事件
                button.onClick.AddListener(() => command.Execute(null));
            }
        }

        /// <summary>
        /// 绑定输入框
        /// </summary>
        private void BindInputField(InputField inputField, string propertyName)
        {
            if (inputField == null || ViewModel == null)
            {
                return;
            }

            // 设置初始值
            inputField.text = ViewModel.GetProperty<string>(propertyName) ?? string.Empty;

            // 监听属性变更
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    inputField.text = ViewModel.GetProperty<string>(propertyName) ?? string.Empty;
                }
            };

            // 监听输入变更
            inputField.onValueChanged.AddListener(value =>
            {
                ViewModel.SetProperty(value, propertyName);
            });
        }

        /// <summary>
        /// 绑定 Toggle
        /// </summary>
        private void BindToggle(Toggle toggle, string propertyName)
        {
            if (toggle == null || ViewModel == null)
            {
                return;
            }

            // 设置初始值
            toggle.isOn = ViewModel.GetProperty<bool>(propertyName);

            // 监听属性变更
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    toggle.isOn = ViewModel.GetProperty<bool>(propertyName);
                }
            };

            // 监听状态变更
            toggle.onValueChanged.AddListener(value =>
            {
                ViewModel.SetProperty(value, propertyName);
            });
        }
    }
} 