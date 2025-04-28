using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UIFramework
{
    /// <summary>
    /// 通用对象池，可以复用任何类型的对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class ObjectPool<T> where T : class
    {
        // 对象创建委托
        private Func<T> createFunc;
        
        // 对象获取/回收/销毁时的回调
        private Action<T> actionOnGet;
        private Action<T> actionOnRelease;
        private Action<T> actionOnDestroy;
        
        // 对象池容量
        private int maxSize;
        
        // 存储未使用对象的栈
        private Stack<T> pool;
        
        // 当前处于活动状态的对象集合
        private HashSet<T> activeObjects;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">创建对象的方法</param>
        /// <param name="actionOnGet">获取对象时的回调</param>
        /// <param name="actionOnRelease">释放对象时的回调</param>
        /// <param name="actionOnDestroy">销毁对象时的回调</param>
        /// <param name="defaultCapacity">默认容量</param>
        /// <param name="maxSize">最大容量</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");
            
            if (defaultCapacity <= 0)
                defaultCapacity = 10;
            
            if (maxSize <= 0)
                maxSize = 100;

            this.createFunc = createFunc;
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.actionOnDestroy = actionOnDestroy;
            this.maxSize = maxSize;

            pool = new Stack<T>(defaultCapacity);
            activeObjects = new HashSet<T>();
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns>池中的对象</returns>
        public T Get()
        {
            T item;
            
            // 如果池中有对象，则直接取出
            if (pool.Count > 0)
            {
                item = pool.Pop();
            }
            else
            {
                // 否则创建新对象
                item = createFunc();
            }

            // 添加到活动对象集合
            activeObjects.Add(item);
            
            // 调用获取回调
            actionOnGet?.Invoke(item);
            
            return item;
        }

        /// <summary>
        /// 释放对象回池中
        /// </summary>
        /// <param name="item">要释放的对象</param>
        public void Release(T item)
        {
            if (item == null)
            {
                Debug.LogError("尝试释放空对象到对象池!");
                return;
            }

            // 如果不是活动对象，直接返回
            if (!activeObjects.Contains(item))
            {
                Debug.LogWarning("尝试释放不是从该对象池获取的对象!");
                return;
            }

            // 从活动对象集合中移除
            activeObjects.Remove(item);

            // 如果池未满，则回收对象
            if (pool.Count < maxSize)
            {
                // 调用释放回调
                actionOnRelease?.Invoke(item);
                
                // 放回池中
                pool.Push(item);
            }
            else
            {
                // 如果池已满，则销毁对象
                actionOnDestroy?.Invoke(item);
            }
        }

        /// <summary>
        /// 释放所有活动对象
        /// </summary>
        public void ReleaseAll()
        {
            // 创建临时列表，避免集合修改异常
            var tempList = new List<T>(activeObjects);
            
            foreach (var item in tempList)
            {
                Release(item);
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            // 销毁所有活动对象
            foreach (var item in activeObjects)
            {
                actionOnDestroy?.Invoke(item);
            }
            
            // 销毁所有池中对象
            foreach (var item in pool)
            {
                actionOnDestroy?.Invoke(item);
            }
            
            activeObjects.Clear();
            pool.Clear();
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="count">预创建的对象数量</param>
        public void Preload(int count)
        {
            if (count <= 0)
                return;

            // 限制预创建数量不超过最大容量
            count = Mathf.Min(count, maxSize - pool.Count);
            
            // 预创建指定数量的对象
            for (int i = 0; i < count; i++)
            {
                T item = createFunc();
                actionOnRelease?.Invoke(item);
                pool.Push(item);
                
                // 如果池已满，则停止创建
                if (pool.Count >= maxSize)
                    break;
            }
        }

        /// <summary>
        /// 获取当前池中对象数量
        /// </summary>
        public int CountInactive => pool.Count;

        /// <summary>
        /// 获取当前活动对象数量
        /// </summary>
        public int CountActive => activeObjects.Count;

        /// <summary>
        /// 获取当前总对象数量(活动+非活动)
        /// </summary>
        public int CountAll => pool.Count + activeObjects.Count;
    }

    /// <summary>
    /// 游戏对象池，专门用于管理GameObject对象
    /// </summary>
    public class GameObjectPool
    {
        // 对象池字典
        private static Dictionary<string, ObjectPool<GameObject>> poolDict = new Dictionary<string, ObjectPool<GameObject>>();

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父节点</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <returns>创建的对象池</returns>
        public static ObjectPool<GameObject> CreatePool(string poolName, GameObject prefab, Transform parent = null, int initialSize = 10, int maxSize = 100)
        {
            if (string.IsNullOrEmpty(poolName) || prefab == null)
            {
                Debug.LogError("创建对象池失败: 名称为空或预制体为空!");
                return null;
            }

            // 如果已存在同名对象池，则返回
            if (poolDict.ContainsKey(poolName))
            {
                Debug.LogWarning($"对象池 {poolName} 已存在!");
                return poolDict[poolName];
            }

            // 创建对象池
            ObjectPool<GameObject> objectPool = new ObjectPool<GameObject>(
                createFunc: () => UnityEngine.Object.Instantiate(prefab, parent),
                actionOnGet: (go) => go.SetActive(true),
                actionOnRelease: (go) => go.SetActive(false),
                actionOnDestroy: (go) => UnityEngine.Object.Destroy(go),
                defaultCapacity: initialSize,
                maxSize: maxSize
            );

            // 预热对象池
            objectPool.Preload(initialSize);

            // 添加到字典
            poolDict[poolName] = objectPool;

            return objectPool;
        }

        /// <summary>
        /// 获取对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象池</returns>
        public static ObjectPool<GameObject> GetPool(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
                return null;

            if (poolDict.TryGetValue(poolName, out ObjectPool<GameObject> pool))
            {
                return pool;
            }

            return null;
        }

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>获取的对象</returns>
        public static GameObject Get(string poolName)
        {
            ObjectPool<GameObject> pool = GetPool(poolName);
            if (pool == null)
            {
                Debug.LogError($"对象池 {poolName} 不存在!");
                return null;
            }

            return pool.Get();
        }

        /// <summary>
        /// 释放对象回对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="obj">要释放的对象</param>
        public static void Release(string poolName, GameObject obj)
        {
            if (obj == null)
                return;

            ObjectPool<GameObject> pool = GetPool(poolName);
            if (pool == null)
            {
                Debug.LogError($"对象池 {poolName} 不存在!");
                return;
            }

            pool.Release(obj);
        }

        /// <summary>
        /// 清空指定对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        public static void ClearPool(string poolName)
        {
            ObjectPool<GameObject> pool = GetPool(poolName);
            if (pool == null)
                return;

            pool.Clear();
            poolDict.Remove(poolName);
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public static void ClearAllPools()
        {
            foreach (var pool in poolDict.Values)
            {
                pool.Clear();
            }
            poolDict.Clear();
        }
    }
} 