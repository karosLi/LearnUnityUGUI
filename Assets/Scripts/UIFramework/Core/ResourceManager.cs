using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UIFramework
{
    /// <summary>
    /// 资源管理器，负责加载和缓存资源
    /// </summary>
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        // 资源缓存字典
        private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();
        
        // 异步加载中的资源
        private Dictionary<string, List<Action<UnityEngine.Object>>> loadingResources = new Dictionary<string, List<Action<UnityEngine.Object>>>();

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="cache">是否缓存</param>
        /// <returns>加载的资源</returns>
        public T Load<T>(string path, bool cache = true) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("资源路径为空!");
                return null;
            }

            // 尝试从缓存获取
            string key = GetCacheKey<T>(path);
            if (resourceCache.TryGetValue(key, out UnityEngine.Object cachedResource))
            {
                return cachedResource as T;
            }

            // 加载资源
            T resource = Resources.Load<T>(path);
            if (resource == null)
            {
                Debug.LogError($"无法加载资源: {path}");
                return null;
            }

            // 缓存资源
            if (cache)
            {
                resourceCache[key] = resource;
            }

            return resource;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="cache">是否缓存</param>
        public void LoadAsync<T>(string path, Action<T> callback, bool cache = true) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("资源路径为空!");
                if (callback != null)
                {
                    callback(null);
                }
                return;
            }

            // 尝试从缓存获取
            string key = GetCacheKey<T>(path);
            if (resourceCache.TryGetValue(key, out UnityEngine.Object cachedResource))
            {
                if (callback != null)
                {
                    callback(cachedResource as T);
                }
                return;
            }

            // 添加到加载队列
            if (!loadingResources.TryGetValue(key, out List<Action<UnityEngine.Object>> callbacks))
            {
                callbacks = new List<Action<UnityEngine.Object>>();
                loadingResources[key] = callbacks;

                // 启动异步加载协程
                StartCoroutine(LoadAsyncCoroutine<T>(path, key, cache));
            }

            // 添加回调
            if (callback != null)
            {
                callbacks.Add((obj) => callback(obj as T));
            }
        }

        /// <summary>
        /// 异步加载协程
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="key">缓存键</param>
        /// <param name="cache">是否缓存</param>
        private IEnumerator LoadAsyncCoroutine<T>(string path, string key, bool cache) where T : UnityEngine.Object
        {
            // 异步加载资源
            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;

            // 获取加载结果
            T resource = request.asset as T;

            // 缓存资源
            if (cache && resource != null)
            {
                resourceCache[key] = resource;
            }

            // 如果没有回调队列，直接返回
            if (!loadingResources.TryGetValue(key, out List<Action<UnityEngine.Object>> callbacks))
            {
                yield break;
            }

            // 调用所有回调
            foreach (var callback in callbacks)
            {
                callback?.Invoke(resource);
            }

            // 移除回调队列
            loadingResources.Remove(key);
        }

        /// <summary>
        /// 加载GameObject
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="cache">是否缓存</param>
        /// <returns>加载的GameObject</returns>
        public GameObject LoadGameObject(string path, bool cache = true)
        {
            return Load<GameObject>(path, cache);
        }

        /// <summary>
        /// 异步加载GameObject
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="cache">是否缓存</param>
        public void LoadGameObjectAsync(string path, Action<GameObject> callback, bool cache = true)
        {
            LoadAsync<GameObject>(path, callback, cache);
        }

        /// <summary>
        /// 加载精灵
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="cache">是否缓存</param>
        /// <returns>加载的精灵</returns>
        public Sprite LoadSprite(string path, bool cache = true)
        {
            return Load<Sprite>(path, cache);
        }

        /// <summary>
        /// 异步加载精灵
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="cache">是否缓存</param>
        public void LoadSpriteAsync(string path, Action<Sprite> callback, bool cache = true)
        {
            LoadAsync<Sprite>(path, callback, cache);
        }

        /// <summary>
        /// 加载纹理
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="cache">是否缓存</param>
        /// <returns>加载的纹理</returns>
        public Texture LoadTexture(string path, bool cache = true)
        {
            return Load<Texture>(path, cache);
        }

        /// <summary>
        /// 异步加载纹理
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="cache">是否缓存</param>
        public void LoadTextureAsync(string path, Action<Texture> callback, bool cache = true)
        {
            LoadAsync<Texture>(path, callback, cache);
        }

        /// <summary>
        /// 加载音频剪辑
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="cache">是否缓存</param>
        /// <returns>加载的音频剪辑</returns>
        public AudioClip LoadAudioClip(string path, bool cache = true)
        {
            return Load<AudioClip>(path, cache);
        }

        /// <summary>
        /// 异步加载音频剪辑
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="cache">是否缓存</param>
        public void LoadAudioClipAsync(string path, Action<AudioClip> callback, bool cache = true)
        {
            LoadAsync<AudioClip>(path, callback, cache);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        public void Unload<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
                return;

            string key = GetCacheKey<T>(path);
            if (resourceCache.TryGetValue(key, out UnityEngine.Object resource))
            {
                resourceCache.Remove(key);
                // 注意：这里不能直接Destroy资源，因为可能在其他地方仍在使用
                // 使用Resources.UnloadUnusedAssets()来卸载不再使用的资源
            }
        }

        /// <summary>
        /// 清空所有缓存的资源
        /// </summary>
        public void ClearCache()
        {
            resourceCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 获取缓存键
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>缓存键</returns>
        private string GetCacheKey<T>(string path) where T : UnityEngine.Object
        {
            return $"{typeof(T).Name}_{path}";
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearCache();
        }
    }
} 