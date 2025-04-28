using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// MonoBehaviour的单例模式基类，保证场景中只有一个实例
    /// </summary>
    /// <typeparam name="T">继承该类的子类类型</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static T instance;

        /// <summary>
        /// 是否已销毁
        /// </summary>
        private static bool isDestroyed = false;

        /// <summary>
        /// 线程锁
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// 单例实例访问器
        /// </summary>
        public static T Instance
        {
            get
            {
                // 如果已经被销毁，直接返回null
                if (isDestroyed)
                {
                    Debug.LogWarning($"单例 {typeof(T).Name} 已被销毁，无法访问!");
                    return null;
                }

                lock (lockObj)
                {
                    // 如果实例不存在，尝试查找现有实例或创建新实例
                    if (instance == null)
                    {
                        // 查找场景中是否已存在该类型的对象
                        instance = FindObjectOfType<T>();

                        // 如果场景中不存在，则创建新对象
                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<T>();
                            singleton.name = $"{typeof(T).Name} (Singleton)";
                            
                            // 确保单例对象不会在场景切换时被销毁
                            DontDestroyOnLoad(singleton);
                            
                            Debug.Log($"创建单例: {typeof(T).Name}");
                        }
                        else
                        {
                            Debug.Log($"使用已存在的单例: {typeof(T).Name}");
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 在类被第一次实例化时调用
        /// </summary>
        protected virtual void Awake()
        {
            // 检查场景中是否已存在该类型的实例
            T[] instances = FindObjectsOfType<T>();
            
            // 如果存在多个实例，保留第一个并销毁当前实例
            if (instances.Length > 1)
            {
                // 如果当前实例不是第一个被创建的实例，则销毁当前实例
                if (instances[0] != this)
                {
                    Debug.LogWarning($"销毁重复单例: {typeof(T).Name}");
                    Destroy(gameObject);
                    return;
                }
            }
            
            // 如果实例还未设置，将当前对象设为实例
            if (instance == null)
            {
                instance = this as T;
                
                // 确保单例对象不会在场景切换时被销毁
                DontDestroyOnLoad(gameObject);
            }
            
            // 初始化单例
            InitSingleton();
        }

        /// <summary>
        /// 供子类重写的单例初始化方法
        /// </summary>
        protected virtual void InitSingleton() { }

        /// <summary>
        /// 当对象被销毁时调用
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 如果当前对象是单例实例，则设置标记
            if (instance == this)
            {
                isDestroyed = true;
                instance = null;
            }
        }

        /// <summary>
        /// 当应用程序退出时调用
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            isDestroyed = true;
            instance = null;
        }
    }
} 