using System;
using System.Collections.Generic;
using UIFramework.MVVM.Rx;

namespace UIFramework.MVVM.Signals
{
    /// <summary>
    /// 信号基类
    /// </summary>
    public abstract class Signal
    {
        private static Dictionary<string, Signal> signals = new Dictionary<string, Signal>();

        /// <summary>
        /// 获取信号实例
        /// </summary>
        /// <typeparam name="T">信号类型</typeparam>
        /// <param name="name">信号名称</param>
        /// <returns>信号实例</returns>
        public static T Get<T>(string name) where T : Signal, new()
        {
            if (!signals.TryGetValue(name, out Signal signal))
            {
                signal = new T();
                signals[name] = signal;
            }
            return (T)signal;
        }

        /// <summary>
        /// 清理所有信号
        /// </summary>
        public static void ClearAll()
        {
            foreach (var signal in signals.Values)
            {
                signal.Dispose();
            }
            signals.Clear();
        }
    }

    /// <summary>
    /// 无参数信号
    /// </summary>
    public class Signal : Signal, IDisposable
    {
        private readonly Subject<Unit> subject = new Subject<Unit>();

        /// <summary>
        /// 触发信号
        /// </summary>
        public void Emit()
        {
            subject.OnNext(Unit.Default);
        }

        /// <summary>
        /// 订阅信号
        /// </summary>
        /// <param name="onNext">数据到达回调</param>
        /// <returns>订阅令牌</returns>
        public IDisposable Subscribe(Action onNext)
        {
            return subject.Subscribe(new Observer<Unit>(_ => onNext?.Invoke()));
        }

        /// <summary>
        /// 订阅信号
        /// </summary>
        /// <param name="observer">观察者</param>
        /// <returns>订阅令牌</returns>
        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            return subject.Subscribe(observer);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            subject.OnCompleted();
            subject.Dispose();
        }
    }

    /// <summary>
    /// 带参数信号
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public class Signal<T> : Signal, IDisposable
    {
        private readonly Subject<T> subject = new Subject<T>();

        /// <summary>
        /// 触发信号
        /// </summary>
        /// <param name="arg">参数</param>
        public void Emit(T arg)
        {
            subject.OnNext(arg);
        }

        /// <summary>
        /// 订阅信号
        /// </summary>
        /// <param name="onNext">数据到达回调</param>
        /// <returns>订阅令牌</returns>
        public IDisposable Subscribe(Action<T> onNext)
        {
            return subject.Subscribe(new Observer<T>(onNext));
        }

        /// <summary>
        /// 订阅信号
        /// </summary>
        /// <param name="observer">观察者</param>
        /// <returns>订阅令牌</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return subject.Subscribe(observer);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            subject.OnCompleted();
            subject.Dispose();
        }
    }

    /// <summary>
    /// 观察者实现
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    private class Observer<T> : IObserver<T>
    {
        private readonly Action<T> onNext;
        private readonly Action<Exception> onError;
        private readonly Action onCompleted;

        public Observer(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onCompleted = onCompleted;
        }

        public void OnNext(T value)
        {
            onNext?.Invoke(value);
        }

        public void OnError(Exception error)
        {
            onError?.Invoke(error);
        }

        public void OnCompleted()
        {
            onCompleted?.Invoke();
        }
    }

    /// <summary>
    /// 空值类型
    /// </summary>
    public struct Unit
    {
        public static readonly Unit Default = new Unit();
    }
} 