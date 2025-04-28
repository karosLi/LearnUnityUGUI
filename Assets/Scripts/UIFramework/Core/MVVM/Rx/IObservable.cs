using System;

namespace UIFramework.MVVM.Rx
{
    /// <summary>
    /// 可观察对象接口
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IObservable<T>
    {
        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="observer">观察者</param>
        /// <returns>订阅令牌</returns>
        IDisposable Subscribe(IObserver<T> observer);
    }

    /// <summary>
    /// 观察者接口
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IObserver<T>
    {
        /// <summary>
        /// 数据到达
        /// </summary>
        /// <param name="value">数据</param>
        void OnNext(T value);

        /// <summary>
        /// 发生错误
        /// </summary>
        /// <param name="error">错误</param>
        void OnError(Exception error);

        /// <summary>
        /// 完成
        /// </summary>
        void OnCompleted();
    }

    /// <summary>
    /// 订阅令牌
    /// </summary>
    public interface IDisposable
    {
        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }
} 