using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    /// <summary>
    /// UI管理器，负责管理所有UI面板的显示、隐藏和生命周期
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        // UI层级枚举
        public enum UILayer
        {
            Bottom = 0,     // 底层（如背景）
            Normal = 1,     // 普通层（如主界面）
            Top = 2,        // 顶层（如对话框）
            System = 3,     // 系统层（如加载、提示）
        }

        // UI面板配置信息
        [System.Serializable]
        public class UIPanelInfo
        {
            public string PanelName;              // 面板名称
            public string PrefabPath;             // 预制体路径
            public UILayer Layer = UILayer.Normal;// 面板层级
            public bool CachePanel = true;        // 是否缓存面板
            public bool ShowMask = false;         // 是否显示遮罩
        }

        [Header("UI层级设置")]
        [SerializeField] private Canvas[] uiLayerCanvases;     // 各层级的Canvas
        [SerializeField] private GameObject maskPrefab;         // 遮罩预制体

        [Header("UI面板配置")]
        [SerializeField] private List<UIPanelInfo> panelInfos; // 面板配置列表
        private Dictionary<string, UIPanelInfo> panelInfoDict; // 面板配置字典

        private Dictionary<string, UIPanel> activePanels;      // 已激活的面板
        private Dictionary<string, UIPanel> cachedPanels;      // 已缓存的面板
        private Dictionary<string, GameObject> panelMasks;     // 面板遮罩

        // 初始化
        protected override void Awake()
        {
            base.Awake();
            
            // 初始化字典
            panelInfoDict = new Dictionary<string, UIPanelInfo>();
            activePanels = new Dictionary<string, UIPanel>();
            cachedPanels = new Dictionary<string, UIPanel>();
            panelMasks = new Dictionary<string, GameObject>();

            // 转换panel信息为字典
            foreach (var info in panelInfos)
            {
                panelInfoDict[info.PanelName] = info;
            }

            // 确保所有层级Canvas都存在
            if (uiLayerCanvases.Length != System.Enum.GetValues(typeof(UILayer)).Length)
            {
                Debug.LogError("UI层级Canvas数量与UILayer枚举数量不匹配!");
            }

            // 设置各层级Canvas的排序
            for (int i = 0; i < uiLayerCanvases.Length; i++)
            {
                if (uiLayerCanvases[i] != null)
                {
                    uiLayerCanvases[i].sortingOrder = i * 100; // 每层预留100个排序值
                }
            }

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <param name="onPanelOpened">面板打开回调</param>
        /// <param name="args">传递给面板的参数</param>
        /// <returns>打开的面板</returns>
        public UIPanel OpenPanel(string panelName, System.Action<UIPanel> onPanelOpened = null, params object[] args)
        {
            // 检查面板是否已经打开
            if (activePanels.TryGetValue(panelName, out UIPanel panel))
            {
                panel.OnRefresh(args);
                onPanelOpened?.Invoke(panel);
                return panel;
            }

            // 检查面板是否已缓存
            if (cachedPanels.TryGetValue(panelName, out panel))
            {
                // 从缓存恢复面板
                activePanels[panelName] = panel;
                cachedPanels.Remove(panelName);
                panel.gameObject.SetActive(true);
                panel.OnOpen(args);
                onPanelOpened?.Invoke(panel);

                // 显示遮罩
                if (panelInfoDict.TryGetValue(panelName, out UIPanelInfo info) && info.ShowMask)
                {
                    ShowPanelMask(panelName, info.Layer);
                }

                return panel;
            }

            // 检查面板信息是否存在
            if (!panelInfoDict.TryGetValue(panelName, out UIPanelInfo panelInfo))
            {
                Debug.LogError($"找不到面板信息: {panelName}");
                return null;
            }

            // 加载面板预制体
            GameObject panelGo = ResourceManager.Instance.LoadGameObject(panelInfo.PrefabPath);
            if (panelGo == null)
            {
                Debug.LogError($"无法加载面板预制体: {panelInfo.PrefabPath}");
                return null;
            }

            // 设置面板父级
            Canvas targetCanvas = uiLayerCanvases[(int)panelInfo.Layer];
            if (targetCanvas == null)
            {
                Debug.LogError($"UI层级Canvas不存在: {panelInfo.Layer}");
                return null;
            }

            // 实例化面板
            GameObject panelInstance = Instantiate(panelGo, targetCanvas.transform);
            panelInstance.name = panelName;

            // 获取UIPanel组件
            panel = panelInstance.GetComponent<UIPanel>();
            if (panel == null)
            {
                panel = panelInstance.AddComponent<UIPanel>();
            }

            // 初始化面板
            panel.Initialize(panelName, panelInfo.Layer);
            activePanels[panelName] = panel;
            panel.OnOpen(args);
            onPanelOpened?.Invoke(panel);

            // 显示遮罩
            if (panelInfo.ShowMask)
            {
                ShowPanelMask(panelName, panelInfo.Layer);
            }

            return panel;
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <param name="destroyImmediately">是否立即销毁不缓存</param>
        public void ClosePanel(string panelName, bool destroyImmediately = false)
        {
            if (!activePanels.TryGetValue(panelName, out UIPanel panel))
            {
                Debug.LogWarning($"尝试关闭未打开的面板: {panelName}");
                return;
            }

            // 调用面板关闭方法
            panel.OnClose();
            activePanels.Remove(panelName);

            // 获取面板信息
            bool cachePanel = panelInfoDict.TryGetValue(panelName, out UIPanelInfo info) && info.CachePanel && !destroyImmediately;

            // 缓存或销毁面板
            if (cachePanel)
            {
                panel.gameObject.SetActive(false);
                cachedPanels[panelName] = panel;
            }
            else
            {
                Destroy(panel.gameObject);
            }

            // 隐藏遮罩
            HidePanelMask(panelName);
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        /// <param name="destroyImmediately">是否立即销毁不缓存</param>
        public void CloseAllPanels(bool destroyImmediately = false)
        {
            // 创建临时列表避免字典修改异常
            List<string> panelsToClose = new List<string>(activePanels.Keys);
            
            foreach (string panelName in panelsToClose)
            {
                ClosePanel(panelName, destroyImmediately);
            }
        }

        /// <summary>
        /// 获取当前打开的面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <returns>面板实例</returns>
        public UIPanel GetPanel(string panelName)
        {
            if (activePanels.TryGetValue(panelName, out UIPanel panel))
            {
                return panel;
            }
            return null;
        }

        /// <summary>
        /// 显示面板遮罩
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <param name="layer">所在层级</param>
        private void ShowPanelMask(string panelName, UILayer layer)
        {
            if (maskPrefab == null)
            {
                Debug.LogWarning("遮罩预制体未设置!");
                return;
            }

            // 如果已存在遮罩，则直接显示
            if (panelMasks.TryGetValue(panelName, out GameObject maskGo))
            {
                maskGo.SetActive(true);
                return;
            }

            // 创建新遮罩
            Canvas targetCanvas = uiLayerCanvases[(int)layer];
            GameObject mask = Instantiate(maskPrefab, targetCanvas.transform);
            mask.name = $"{panelName}_Mask";
            
            // 确保遮罩在面板下方
            mask.transform.SetAsFirstSibling();
            
            // 记录遮罩
            panelMasks[panelName] = mask;
        }

        /// <summary>
        /// 隐藏面板遮罩
        /// </summary>
        /// <param name="panelName">面板名称</param>
        private void HidePanelMask(string panelName)
        {
            if (panelMasks.TryGetValue(panelName, out GameObject maskGo))
            {
                Destroy(maskGo);
                panelMasks.Remove(panelName);
            }
        }

        /// <summary>
        /// 检查面板是否打开
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <returns>是否打开</returns>
        public bool IsPanelOpen(string panelName)
        {
            return activePanels.ContainsKey(panelName);
        }

        /// <summary>
        /// 清理缓存的面板
        /// </summary>
        public void ClearCache()
        {
            foreach (var panel in cachedPanels.Values)
            {
                Destroy(panel.gameObject);
            }
            cachedPanels.Clear();
        }
    }
} 