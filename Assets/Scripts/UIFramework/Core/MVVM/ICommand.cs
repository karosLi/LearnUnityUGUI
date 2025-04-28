using System;

namespace UIFramework.MVVM
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 是否可以执行
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        void Execute(object parameter);
    }

    /// <summary>
    /// 命令实现类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">是否可以执行</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// 是否可以执行
        /// </summary>
        public bool CanExecute => canExecute == null || canExecute(null);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        public void Execute(object parameter)
        {
            if (CanExecute)
            {
                execute(parameter);
            }
        }
    }
} 