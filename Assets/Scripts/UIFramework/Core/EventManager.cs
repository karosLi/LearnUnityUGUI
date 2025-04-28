using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 事件管理器，负责处理全局事件的注册和分发
    /// </summary>
    public class EventManager : MonoSingleton<EventManager>
    {
        // 事件处理器字典，key为事件名称，value为事件回调列表
        private Dictionary<string, List<System.Action<object>>> eventListeners = new Dictionary<string, List<System.Action<object>>>();

        // 标记为移除的事件，在安全时机进行处理
        private List<EventRemoveRecord> pendingRemoveList = new List<EventRemoveRecord>();
        private bool isTriggering = false;

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">事件回调</param>
        public void AddEventListener(string eventName, System.Action<object> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                Debug.LogError("事件名称为空或回调为空!");
                return;
            }

            if (!eventListeners.TryGetValue(eventName, out List<System.Action<object>> listeners))
            {
                listeners = new List<System.Action<object>>();
                eventListeners[eventName] = listeners;
            }

            // 检查是否已经存在相同的回调
            if (!listeners.Contains(callback))
            {
                listeners.Add(callback);
            }
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">事件回调</param>
        public void RemoveEventListener(string eventName, System.Action<object> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                return;
            }

            // 如果当前正在触发事件，则延迟移除
            if (isTriggering)
            {
                pendingRemoveList.Add(new EventRemoveRecord { EventName = eventName, Callback = callback });
                return;
            }

            // 直接移除
            RemoveListenerImmediate(eventName, callback);
        }

        /// <summary>
        /// 立即移除事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">事件回调</param>
        private void RemoveListenerImmediate(string eventName, System.Action<object> callback)
        {
            if (eventListeners.TryGetValue(eventName, out List<System.Action<object>> listeners))
            {
                listeners.Remove(callback);

                // 如果该事件没有监听者了，从字典中移除
                if (listeners.Count == 0)
                {
                    eventListeners.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">事件数据</param>
        public void TriggerEvent(string eventName, object data = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError("事件名称为空!");
                return;
            }

            // 如果该事件没有监听者，直接返回
            if (!eventListeners.TryGetValue(eventName, out List<System.Action<object>> listeners) || listeners.Count == 0)
            {
                return;
            }

            isTriggering = true;

            // 创建临时列表，避免在遍历过程中添加或移除监听者导致的问题
            List<System.Action<object>> tempListeners = new List<System.Action<object>>(listeners);

            // 触发所有回调
            foreach (var listener in tempListeners)
            {
                try
                {
                    listener?.Invoke(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"事件触发异常: {eventName}, 错误: {e.Message}");
                }
            }

            isTriggering = false;

            // 处理延迟移除的监听者
            ProcessPendingRemoves();
        }

        /// <summary>
        /// 处理延迟移除的监听者
        /// </summary>
        private void ProcessPendingRemoves()
        {
            if (pendingRemoveList.Count > 0)
            {
                foreach (var record in pendingRemoveList)
                {
                    RemoveListenerImmediate(record.EventName, record.Callback);
                }
                pendingRemoveList.Clear();
            }
        }

        /// <summary>
        /// 清空所有事件监听
        /// </summary>
        public void ClearAllEvents()
        {
            eventListeners.Clear();
            pendingRemoveList.Clear();
            isTriggering = false;
        }

        /// <summary>
        /// 待移除的事件记录
        /// </summary>
        private class EventRemoveRecord
        {
            public string EventName;
            public System.Action<object> Callback;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearAllEvents();
        }
    }
} 