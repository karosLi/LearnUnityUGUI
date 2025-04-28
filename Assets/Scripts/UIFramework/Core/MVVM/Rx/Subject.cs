using System;
using System.Collections.Generic;

namespace UIFramework.MVVM.Rx
{
    /// <summary>
    /// 主题类，既是观察者又是可观察对象
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class Subject<T> : IObservable<T>, IObserver<T>
    {
        private List<IObserver<T>> observers = new List<IObserver<T>>();
        private bool isDisposed;
        private bool isCompleted;
        private Exception error;

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="observer">观察者</param>
        /// <returns>订阅令牌</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(Subject<T>));
            }

            if (isCompleted)
            {
                observer.OnCompleted();
                return EmptyDisposable.Instance;
            }

            if (error != null)
            {
                observer.OnError(error);
                return EmptyDisposable.Instance;
            }

            observers.Add(observer);
            return new Subscription(this, observer);
        }

        /// <summary>
        /// 数据到达
        /// </summary>
        /// <param name="value">数据</param>
        public void OnNext(T value)
        {
            if (isDisposed || isCompleted || error != null)
            {
                return;
            }

            foreach (var observer in observers)
            {
                observer.OnNext(value);
            }
        }

        /// <summary>
        /// 发生错误
        /// </summary>
        /// <param name="error">错误</param>
        public void OnError(Exception error)
        {
            if (isDisposed || isCompleted || this.error != null)
            {
                return;
            }

            this.error = error;
            foreach (var observer in observers)
            {
                observer.OnError(error);
            }
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void OnCompleted()
        {
            if (isDisposed || isCompleted || error != null)
            {
                return;
            }

            isCompleted = true;
            foreach (var observer in observers)
            {
                observer.OnCompleted();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                observers.Clear();
            }
        }

        /// <summary>
        /// 订阅令牌
        /// </summary>
        private class Subscription : IDisposable
        {
            private readonly Subject<T> subject;
            private readonly IObserver<T> observer;

            public Subscription(Subject<T> subject, IObserver<T> observer)
            {
                this.subject = subject;
                this.observer = observer;
            }

            public void Dispose()
            {
                if (subject != null && observer != null)
                {
                    subject.observers.Remove(observer);
                }
            }
        }

        /// <summary>
        /// 空订阅令牌
        /// </summary>
        private class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();

            private EmptyDisposable() { }

            public void Dispose() { }
        }
    }
} 