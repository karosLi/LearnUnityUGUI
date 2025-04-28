using System;

namespace UIFramework.MVVM.Bindings
{
    /// <summary>
    /// 绑定注解基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public abstract class BindingAttribute : Attribute
    {
        /// <summary>
        /// 绑定的属性名称
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyName">绑定的属性名称</param>
        protected BindingAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// 单向绑定注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class OneWayBindingAttribute : BindingAttribute
    {
        public OneWayBindingAttribute(string propertyName) : base(propertyName) { }
    }

    /// <summary>
    /// 双向绑定注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class TwoWayBindingAttribute : BindingAttribute
    {
        public TwoWayBindingAttribute(string propertyName) : base(propertyName) { }
    }

    /// <summary>
    /// 命令绑定注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class CommandBindingAttribute : BindingAttribute
    {
        /// <summary>
        /// 是否启用状态绑定
        /// </summary>
        public bool BindEnabled { get; }

        public CommandBindingAttribute(string propertyName, bool bindEnabled = false) 
            : base(propertyName)
        {
            BindEnabled = bindEnabled;
        }
    }

    /// <summary>
    /// 信号绑定注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class SignalBindingAttribute : BindingAttribute
    {
        /// <summary>
        /// 信号名称
        /// </summary>
        public string SignalName { get; }

        public SignalBindingAttribute(string signalName) 
            : base(null)
        {
            SignalName = signalName;
        }
    }
} 