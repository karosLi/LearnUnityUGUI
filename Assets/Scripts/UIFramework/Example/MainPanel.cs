using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

/// <summary>
/// 主面板示例，展示UI框架的各种功能
/// </summary>
public class MainPanel : UIPanel
{
    // UI组件引用
    private Text welcomeText;
    private Button settingsButton;
    private Button storeButton;
    private Button inventoryButton;
    private Button messageButton;
    private Button logoutButton;
    private Image avatarImage;
    
    // 玩家数据
    private string username;
    private int notificationCount;
    
    // 事件名称
    private const string EVENT_NOTIFICATION_RECEIVED = "NotificationReceived";
    private const string EVENT_PROFILE_UPDATED = "ProfileUpdated";
    
    /// <summary>
    /// 初始化面板
    /// </summary>
    public override void Initialize(string panelName, UIManager.UILayer layer)
    {
        base.Initialize(panelName, layer);
        
        // 获取UI组件引用
        welcomeText = GetText("WelcomeText");
        settingsButton = GetButton("SettingsButton", OnSettingsClick);
        storeButton = GetButton("StoreButton", OnStoreClick);
        inventoryButton = GetButton("InventoryButton", OnInventoryClick);
        messageButton = GetButton("MessageButton", OnMessageClick);
        logoutButton = GetButton("LogoutButton", OnLogoutClick);
        avatarImage = GetImage("AvatarImage");
        
        // 注册全局事件
        AddEventListener(EVENT_NOTIFICATION_RECEIVED, OnNotificationReceived);
        AddEventListener(EVENT_PROFILE_UPDATED, OnProfileUpdated);
        
        // 设置默认头像
        if (avatarImage != null)
        {
            // 使用资源管理器异步加载头像
            ResourceManager.Instance.LoadSpriteAsync("UI/DefaultAvatar", (sprite) => {
                if (avatarImage != null)
                {
                    avatarImage.sprite = sprite;
                }
            });
        }
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    public override void OnOpen(params object[] args)
    {
        base.OnOpen(args);
        
        // 获取用户名参数
        if (args != null && args.Length > 0 && args[0] is string)
        {
            username = args[0] as string;
        }
        
        // 设置欢迎文本
        if (welcomeText != null)
        {
            welcomeText.text = string.IsNullOrEmpty(username) ? 
                "欢迎回来" : string.Format("欢迎回来，{0}", username);
        }
        
        // 重置通知数量
        notificationCount = 0;
        UpdateNotificationCount();
        
        // 开始模拟接收通知
        StartCoroutine(SimulateNotifications());
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public override void OnClose()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        base.OnClose();
    }

    /// <summary>
    /// 刷新面板
    /// </summary>
    public override void OnRefresh(params object[] args)
    {
        base.OnRefresh(args);
        
        // 更新通知数量显示
        UpdateNotificationCount();
    }

    /// <summary>
    /// 设置按钮点击
    /// </summary>
    private void OnSettingsClick()
    {
        UIManager.Instance.OpenPanel("SettingsPanel");
    }

    /// <summary>
    /// 商店按钮点击
    /// </summary>
    private void OnStoreClick()
    {
        UIManager.Instance.OpenPanel("StorePanel");
    }

    /// <summary>
    /// 背包按钮点击
    /// </summary>
    private void OnInventoryClick()
    {
        UIManager.Instance.OpenPanel("InventoryPanel");
    }

    /// <summary>
    /// 消息按钮点击
    /// </summary>
    private void OnMessageClick()
    {
        // 重置通知数量
        notificationCount = 0;
        UpdateNotificationCount();
        
        // 打开消息面板
        UIManager.Instance.OpenPanel("MessagePanel");
    }

    /// <summary>
    /// 登出按钮点击
    /// </summary>
    private void OnLogoutClick()
    {
        // 显示确认对话框
        UIManager.Instance.OpenPanel("ConfirmPanel", UIManager.UILayer.PopUp, 
            "确定要退出登录吗？", 
            () => {
                // 确认回调
                UIManager.Instance.ClosePanel(PanelName);
                UIManager.Instance.OpenPanel("LoginPanel");
            }, 
            null);
    }

    /// <summary>
    /// 接收到通知事件
    /// </summary>
    private void OnNotificationReceived(object data)
    {
        notificationCount++;
        UpdateNotificationCount();
    }

    /// <summary>
    /// 个人资料更新事件
    /// </summary>
    private void OnProfileUpdated(object data)
    {
        if (data is string)
        {
            // 更新头像
            string avatarPath = data as string;
            if (!string.IsNullOrEmpty(avatarPath) && avatarImage != null)
            {
                ResourceManager.Instance.LoadSpriteAsync(avatarPath, (sprite) => {
                    if (avatarImage != null)
                    {
                        avatarImage.sprite = sprite;
                    }
                });
            }
        }
    }

    /// <summary>
    /// 更新通知数量显示
    /// </summary>
    private void UpdateNotificationCount()
    {
        // 获取通知数量文本
        Text notificationText = GetText("NotificationText");
        if (notificationText != null)
        {
            notificationText.text = notificationCount > 0 ? notificationCount.ToString() : "";
            notificationText.gameObject.SetActive(notificationCount > 0);
        }
        
        // 获取通知红点
        GameObject redDot = GetGameObject("NotificationRedDot");
        if (redDot != null)
        {
            redDot.SetActive(notificationCount > 0);
        }
    }

    /// <summary>
    /// 模拟接收通知
    /// </summary>
    private IEnumerator SimulateNotifications()
    {
        while (true)
        {
            // 随机等待5-15秒
            yield return new WaitForSeconds(Random.Range(5f, 15f));
            
            // 触发通知事件
            EventManager.Instance.TriggerEvent(EVENT_NOTIFICATION_RECEIVED);
        }
    }
} 