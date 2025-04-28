using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UIFramework.MVVM
{
    /// <summary>
    /// ViewModel 基类，实现属性变更通知
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // 属性变更事件
        public event PropertyChangedEventHandler PropertyChanged;

        // 属性值字典
        private Dictionary<string, object> propertyValues = new Dictionary<string, object>();

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否发生变更</returns>
        protected bool SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            // 获取旧值
            T oldValue = default;
            if (propertyValues.TryGetValue(propertyName, out object existingValue))
            {
                oldValue = (T)existingValue;
            }

            // 如果值没有变化，直接返回
            if (EqualityComparer<T>.Default.Equals(oldValue, value))
            {
                return false;
            }

            // 更新值
            propertyValues[propertyName] = value;

            // 触发属性变更事件
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        protected T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (propertyValues.TryGetValue(propertyName, out object value))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public virtual void Dispose()
        {
            propertyValues.Clear();
            PropertyChanged = null;
        }
    }
} 