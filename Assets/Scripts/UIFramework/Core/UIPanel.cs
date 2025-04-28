using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFramework
{
    /// <summary>
    /// UI面板基类，所有UI面板继承此类
    /// </summary>
    public class UIPanel : MonoBehaviour
    {
        // 面板信息
        public string PanelName { get; private set; }
        public UIManager.UILayer Layer { get; private set; }
        
        // 面板状态
        public bool IsInitialized { get; private set; }
        
        // 组件缓存
        protected Dictionary<string, UIBehaviour> componentDict = new Dictionary<string, UIBehaviour>();
        
        // 事件监听
        private List<EventListener> eventListeners = new List<EventListener>();

        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <param name="layer">面板层级</param>
        public virtual void Initialize(string panelName, UIManager.UILayer layer)
        {
            if (IsInitialized) return;
            
            PanelName = panelName;
            Layer = layer;
            IsInitialized = true;
            
            // 子类可以重写此方法进行额外初始化
        }

        /// <summary>
        /// 面板打开时调用
        /// </summary>
        /// <param name="args">传递的参数</param>
        public virtual void OnOpen(params object[] args)
        {
            gameObject.SetActive(true);
            // 子类可以重写此方法进行打开逻辑
        }

        /// <summary>
        /// 面板关闭时调用
        /// </summary>
        public virtual void OnClose()
        {
            // 移除所有事件监听
            RemoveAllEventListeners();
            
            // 子类可以重写此方法进行关闭逻辑
        }

        /// <summary>
        /// 面板刷新时调用
        /// </summary>
        /// <param name="args">传递的参数</param>
        public virtual void OnRefresh(params object[] args)
        {
            // 子类可以重写此方法进行刷新逻辑
        }

        /// <summary>
        /// 在面板被销毁前调用
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 移除所有事件监听
            RemoveAllEventListeners();
            
            // 清空组件缓存
            componentDict.Clear();
        }

        #region 组件查找和缓存

        /// <summary>
        /// 获取组件并缓存
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="path">组件路径</param>
        /// <returns>组件</returns>
        public T GetUIComponent<T>(string path) where T : UIBehaviour
        {
            string key = $"{typeof(T).Name}_{path}";
            
            if (componentDict.TryGetValue(key, out UIBehaviour component))
            {
                return component as T;
            }
            
            Transform trans = string.IsNullOrEmpty(path) ? transform : transform.Find(path);
            if (trans == null)
            {
                Debug.LogError($"在面板{PanelName}中找不到路径: {path}");
                return null;
            }
            
            T comp = trans.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"在面板{PanelName}的路径{path}上找不到组件: {typeof(T).Name}");
                return null;
            }
            
            componentDict[key] = comp;
            return comp;
        }

        /// <summary>
        /// 获取按钮组件并添加点击事件
        /// </summary>
        /// <param name="path">按钮路径</param>
        /// <param name="onClick">点击回调</param>
        /// <returns>按钮组件</returns>
        public Button GetButton(string path, UnityEngine.Events.UnityAction onClick)
        {
            Button button = GetUIComponent<Button>(path);
            if (button != null && onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
            return button;
        }

        /// <summary>
        /// 获取图片组件
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>图片组件</returns>
        public Image GetImage(string path)
        {
            return GetUIComponent<Image>(path);
        }

        /// <summary>
        /// 获取文本组件
        /// </summary>
        /// <param name="path">文本路径</param>
        /// <returns>文本组件</returns>
        public Text GetText(string path)
        {
            return GetUIComponent<Text>(path);
        }

        /// <summary>
        /// 获取输入框组件
        /// </summary>
        /// <param name="path">输入框路径</param>
        /// <returns>输入框组件</returns>
        public InputField GetInputField(string path)
        {
            return GetUIComponent<InputField>(path);
        }

        /// <summary>
        /// 获取下拉框组件
        /// </summary>
        /// <param name="path">下拉框路径</param>
        /// <returns>下拉框组件</returns>
        public Dropdown GetDropdown(string path)
        {
            return GetUIComponent<Dropdown>(path);
        }

        /// <summary>
        /// 获取滑动条组件
        /// </summary>
        /// <param name="path">滑动条路径</param>
        /// <returns>滑动条组件</returns>
        public Slider GetSlider(string path)
        {
            return GetUIComponent<Slider>(path);
        }

        /// <summary>
        /// 获取Toggle组件
        /// </summary>
        /// <param name="path">Toggle路径</param>
        /// <returns>Toggle组件</returns>
        public Toggle GetToggle(string path)
        {
            return GetUIComponent<Toggle>(path);
        }

        /// <summary>
        /// 获取滚动视图组件
        /// </summary>
        /// <param name="path">滚动视图路径</param>
        /// <returns>滚动视图组件</returns>
        public ScrollRect GetScrollRect(string path)
        {
            return GetUIComponent<ScrollRect>(path);
        }

        #endregion

        #region 事件监听

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">回调</param>
        public void AddEventListener(string eventName, System.Action<object> callback)
        {
            EventListener listener = new EventListener
            {
                EventName = eventName,
                Callback = callback
            };
            
            EventManager.Instance.AddEventListener(eventName, callback);
            eventListeners.Add(listener);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">回调</param>
        public void RemoveEventListener(string eventName, System.Action<object> callback)
        {
            EventManager.Instance.RemoveEventListener(eventName, callback);
            
            eventListeners.RemoveAll(listener => 
                listener.EventName == eventName && listener.Callback == callback);
        }

        /// <summary>
        /// 移除所有事件监听
        /// </summary>
        public void RemoveAllEventListeners()
        {
            foreach (var listener in eventListeners)
            {
                EventManager.Instance.RemoveEventListener(listener.EventName, listener.Callback);
            }
            eventListeners.Clear();
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">事件数据</param>
        public void SendEvent(string eventName, object data = null)
        {
            EventManager.Instance.TriggerEvent(eventName, data);
        }

        /// <summary>
        /// 事件监听器
        /// </summary>
        private class EventListener
        {
            public string EventName;
            public System.Action<object> Callback;
        }

        #endregion
    }
} 