#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库启动脚本

这个脚本用于一键启动UGUI知识库系统，包括API服务器和WebViewer服务器。
"""

import os
import sys
import subprocess
import threading
import webbrowser
import time

# 获取项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

def check_dependencies():
    """检查依赖是否已安装"""
    try:
        import flask
        import flask_cors
        import markdown
        import requests
        import rich
        return True
    except ImportError as e:
        print(f"错误: 缺少依赖 {e.name}")
        print("请先安装所有依赖:")
        print("pip install -r KnowledgeBase/requirements.txt")
        return False

def run_api_server():
    """运行API服务器"""
    api_script = os.path.join(SCRIPT_DIR, 'api.py')
    print("正在启动API服务器...")
    subprocess.Popen([sys.executable, api_script])

def run_web_viewer():
    """运行WebViewer服务器"""
    web_script = os.path.join(ROOT_DIR, 'WebViewer', 'server.py')
    print("正在启动WebViewer服务器...")
    subprocess.Popen([sys.executable, web_script])

def open_documentation():
    """打开文档查看器"""
    # 等待服务器启动
    time.sleep(2)
    print("正在打开文档查看器...")
    webbrowser.open('http://localhost:8080/WebViewer/UGUIDocumentationViewer.html')

def main():
    """主函数"""
    print("=== UGUI知识库系统启动工具 ===")
    
    # 检查依赖
    if not check_dependencies():
        return
    
    # 启动API服务器
    api_thread = threading.Thread(target=run_api_server)
    api_thread.daemon = True
    api_thread.start()
    
    # 启动WebViewer服务器
    web_thread = threading.Thread(target=run_web_viewer)
    web_thread.daemon = True
    web_thread.start()
    
    # 打开文档查看器
    doc_thread = threading.Thread(target=open_documentation)
    doc_thread.daemon = True
    doc_thread.start()
    
    print("\n知识库系统已启动!")
    print("API服务器运行在: http://localhost:5000")
    print("WebViewer服务器运行在: http://localhost:8080")
    print("\n可用的WebViewer页面:")
    print("- 文档查看器: http://localhost:8080/WebViewer/UGUIDocumentationViewer.html")
    print("- 架构可视化: http://localhost:8080/WebViewer/UGUIArchitectureViewer.html")
    print("- 动画可视化: http://localhost:8080/WebViewer/UGUIAnimation.html")
    print("\n使用MCP客户端工具:")
    print("python KnowledgeBase/mcp_client.py categories")
    print("\n按Ctrl+C退出")
    
    try:
        # 保持主线程运行
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\n正在关闭知识库系统...")
        sys.exit(0)

if __name__ == "__main__":
    main()