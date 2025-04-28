using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

/// <summary>
/// 消息详情面板示例
/// </summary>
public class MessageDetailPanel : UIPanel
{
    // UI组件引用
    private Button closeButton;
    private Text titleText;
    private Text timeText;
    private Text contentText;
    private Button deleteButton;
    
    // 当前消息数据
    private MessagePanel.MessageData currentMessage;
    
    /// <summary>
    /// 初始化面板
    /// </summary>
    public override void Initialize(string panelName, UIManager.UILayer layer)
    {
        base.Initialize(panelName, layer);
        
        // 获取UI组件引用
        closeButton = GetButton("CloseButton", OnCloseClick);
        titleText = GetText("TitleText");
        timeText = GetText("TimeText");
        contentText = GetText("ContentText");
        deleteButton = GetButton("DeleteButton", OnDeleteClick);
    }
    
    /// <summary>
    /// 打开面板
    /// </summary>
    public override void OnOpen(params object[] args)
    {
        base.OnOpen(args);
        
        // 获取消息数据
        if (args != null && args.Length > 0 && args[0] is MessagePanel.MessageData)
        {
            currentMessage = args[0] as MessagePanel.MessageData;
            UpdateUI();
        }
        else
        {
            Debug.LogError("MessageDetailPanel 需要 MessageData 参数");
            UIManager.Instance.ClosePanel(PanelName);
        }
    }
    
    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    private void OnCloseClick()
    {
        UIManager.Instance.ClosePanel(PanelName);
    }
    
    /// <summary>
    /// 删除按钮点击
    /// </summary>
    private void OnDeleteClick()
    {
        // 显示确认对话框
        UIManager.Instance.OpenPanel("ConfirmPanel", UIManager.UILayer.PopUp, 
            "确定要删除此消息吗？", 
            () => {
                // 确认删除
                if (currentMessage != null)
                {
                    // 触发消息删除事件
                    EventManager.Instance.TriggerEvent("MessageDeleted", currentMessage.id);
                    // 关闭本面板
                    UIManager.Instance.ClosePanel(PanelName);
                }
            }, 
            null);
    }
    
    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (currentMessage == null)
        {
            return;
        }
        
        // 设置标题和内容
        if (titleText != null)
        {
            titleText.text = currentMessage.title;
        }
        
        if (timeText != null)
        {
            timeText.text = currentMessage.time;
        }
        
        if (contentText != null)
        {
            contentText.text = currentMessage.content;
        }
    }
} 