using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

/// <summary>
/// 消息面板示例，展示UI框架的列表功能
/// </summary>
public class MessagePanel : UIPanel
{
    // UI组件引用
    private Button backButton;
    private Button deleteAllButton;
    private Transform messageContainer;
    private GameObject messageItemPrefab;
    private Text emptyText;
    
    // 消息数据
    private List<MessageData> messageList = new List<MessageData>();
    
    // 事件名称
    private const string EVENT_MESSAGE_DELETED = "MessageDeleted";
    
    /// <summary>
    /// 消息数据结构
    /// </summary>
    public class MessageData
    {
        public string id;
        public string title;
        public string content;
        public string time;
        public bool isRead;
        
        public MessageData(string id, string title, string content, string time, bool isRead = false)
        {
            this.id = id;
            this.title = title;
            this.content = content;
            this.time = time;
            this.isRead = isRead;
        }
    }
    
    /// <summary>
    /// 初始化面板
    /// </summary>
    public override void Initialize(string panelName, UIManager.UILayer layer)
    {
        base.Initialize(panelName, layer);
        
        // 获取UI组件引用
        backButton = GetButton("BackButton", OnBackClick);
        deleteAllButton = GetButton("DeleteAllButton", OnDeleteAllClick);
        messageContainer = GetTransform("MessageContainer");
        messageItemPrefab = GetGameObject("MessageItemPrefab");
        emptyText = GetText("EmptyText");
        
        // 隐藏预制体
        if (messageItemPrefab != null)
        {
            messageItemPrefab.SetActive(false);
        }
        
        // 注册事件
        AddEventListener(EVENT_MESSAGE_DELETED, OnMessageDeleted);
    }
    
    /// <summary>
    /// 打开面板
    /// </summary>
    public override void OnOpen(params object[] args)
    {
        base.OnOpen(args);
        
        // 生成测试数据
        if (messageList.Count == 0)
        {
            GenerateTestData();
        }
        
        // 渲染消息列表
        RenderMessageList();
    }
    
    /// <summary>
    /// 关闭面板
    /// </summary>
    public override void OnClose()
    {
        // 清空消息容器
        ClearMessageContainer();
        
        base.OnClose();
    }
    
    /// <summary>
    /// 返回按钮点击
    /// </summary>
    private void OnBackClick()
    {
        UIManager.Instance.ClosePanel(PanelName);
    }
    
    /// <summary>
    /// 删除全部按钮点击
    /// </summary>
    private void OnDeleteAllClick()
    {
        if (messageList.Count == 0)
        {
            return;
        }
        
        // 显示确认对话框
        UIManager.Instance.OpenPanel("ConfirmPanel", UIManager.UILayer.PopUp, 
            "确定要删除全部消息吗？", 
            () => {
                // 确认回调
                messageList.Clear();
                RenderMessageList();
            }, 
            null);
    }
    
    /// <summary>
    /// 消息删除事件
    /// </summary>
    private void OnMessageDeleted(object data)
    {
        if (data is string)
        {
            string messageId = data as string;
            
            // 查找并删除消息
            for (int i = 0; i < messageList.Count; i++)
            {
                if (messageList[i].id == messageId)
                {
                    messageList.RemoveAt(i);
                    break;
                }
            }
            
            // 更新UI
            RenderMessageList();
        }
    }
    
    /// <summary>
    /// 消息项点击
    /// </summary>
    private void OnMessageItemClick(MessageData message)
    {
        // 标记为已读
        message.isRead = true;
        
        // 显示消息详情
        UIManager.Instance.OpenPanel("MessageDetailPanel", UIManager.UILayer.PopUp, message);
        
        // 更新UI
        RenderMessageList();
    }
    
    /// <summary>
    /// 删除消息
    /// </summary>
    private void DeleteMessage(string messageId)
    {
        // 触发消息删除事件
        EventManager.Instance.TriggerEvent(EVENT_MESSAGE_DELETED, messageId);
    }
    
    /// <summary>
    /// 渲染消息列表
    /// </summary>
    private void RenderMessageList()
    {
        // 清空消息容器
        ClearMessageContainer();
        
        // 显示空消息提示
        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(messageList.Count == 0);
        }
        
        // 更新删除全部按钮状态
        if (deleteAllButton != null)
        {
            deleteAllButton.interactable = messageList.Count > 0;
        }
        
        // 如果没有消息，直接返回
        if (messageList.Count == 0 || messageContainer == null || messageItemPrefab == null)
        {
            return;
        }
        
        // 创建消息项
        for (int i = 0; i < messageList.Count; i++)
        {
            MessageData message = messageList[i];
            
            // 实例化消息项
            GameObject messageItem = Instantiate(messageItemPrefab, messageContainer);
            messageItem.SetActive(true);
            
            // 设置消息项数据
            Text titleText = messageItem.transform.Find("TitleText")?.GetComponent<Text>();
            Text timeText = messageItem.transform.Find("TimeText")?.GetComponent<Text>();
            GameObject unreadIcon = messageItem.transform.Find("UnreadIcon")?.gameObject;
            Button deleteButton = messageItem.transform.Find("DeleteButton")?.GetComponent<Button>();
            Button itemButton = messageItem.GetComponent<Button>();
            
            if (titleText != null)
            {
                titleText.text = message.title;
            }
            
            if (timeText != null)
            {
                timeText.text = message.time;
            }
            
            if (unreadIcon != null)
            {
                unreadIcon.SetActive(!message.isRead);
            }
            
            if (itemButton != null)
            {
                // 添加点击事件
                MessageData capturedMessage = message; // 捕获当前消息数据
                itemButton.onClick.AddListener(() => OnMessageItemClick(capturedMessage));
            }
            
            if (deleteButton != null)
            {
                // 添加删除按钮点击事件
                string capturedId = message.id; // 捕获当前消息ID
                deleteButton.onClick.AddListener(() => DeleteMessage(capturedId));
            }
        }
    }
    
    /// <summary>
    /// 清空消息容器
    /// </summary>
    private void ClearMessageContainer()
    {
        if (messageContainer == null)
        {
            return;
        }
        
        // 删除所有消息项，保留预制体
        for (int i = messageContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = messageContainer.GetChild(i);
            if (child.gameObject != messageItemPrefab)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    /// <summary>
    /// 生成测试数据
    /// </summary>
    private void GenerateTestData()
    {
        messageList.Clear();
        
        // 添加一些测试消息
        messageList.Add(new MessageData("msg001", "系统公告", "欢迎使用UI框架示例！", "昨天 12:30"));
        messageList.Add(new MessageData("msg002", "活动通知", "周末双倍经验活动即将开始", "昨天 10:15"));
        messageList.Add(new MessageData("msg003", "好友请求", "玩家 [星空漫步者] 请求添加您为好友", "前天 08:45"));
        messageList.Add(new MessageData("msg004", "成就达成", "恭喜您获得成就 [初出茅庐]", "3天前", true));
        messageList.Add(new MessageData("msg005", "礼包发放", "您有一个每日登录礼包待领取", "4天前", true));
    }
} 