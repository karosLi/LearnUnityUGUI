#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库启动脚本

这个脚本用于一键启动UGUI知识库系统，包括API服务器、WebViewer服务器和MCP服务器。
"""

import os
import sys
import subprocess
import threading
import webbrowser
import time
import argparse
import json

# 获取项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

def check_dependencies(mcp_mode=False):
    """检查依赖是否已安装"""
    try:
        import flask
        import flask_cors
        import markdown
        import requests
        import rich
        
        # 如果是MCP模式，还需要检查MCP相关依赖
        if mcp_mode:
            import fastapi
            import uvicorn
            import pydantic
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

def run_mcp_server():
    """运行MCP服务器"""
    try:
        from fastapi import FastAPI
        import uvicorn
        
        print("正在启动MCP服务器...")
        # 使用子进程启动MCP服务器
        mcp_script = os.path.join(SCRIPT_DIR, 'mcp_server.py')
        if not os.path.exists(mcp_script):
            # 如果mcp_server.py不存在，使用test_fastmcp_server.py作为替代
            mcp_script = os.path.join(SCRIPT_DIR, 'test_fastmcp_server.py')
        
        # 设置环境变量
        env = os.environ.copy()
        if 'DOCS_DIR' in os.environ:
            docs_dir = os.environ['DOCS_DIR']
        else:
            docs_dir = os.path.join(ROOT_DIR, 'Docs')
        env['DOCS_DIR'] = docs_dir
        
        subprocess.Popen([sys.executable, mcp_script], env=env)
        print(f"MCP服务器已启动，使用文档目录: {docs_dir}")
    except Exception as e:
        print(f"启动MCP服务器时出错: {str(e)}")

def main():
    """主函数"""
    # 解析命令行参数
    parser = argparse.ArgumentParser(description='UGUI知识库系统启动工具')
    parser.add_argument('--mcp-server', action='store_true', help='启动MCP服务器模式')
    parser.add_argument('--no-browser', action='store_true', help='不自动打开浏览器')
    args = parser.parse_args()
    
    # 检查是否为MCP服务器模式
    mcp_mode = args.mcp_server or os.environ.get('MCP_SERVER_ENABLED', '').lower() == 'true'
    
    print("=== UGUI知识库系统启动工具 ===")
    if mcp_mode:
        print("运行模式: MCP服务器")
    
    # 检查依赖
    if not check_dependencies(mcp_mode):
        return
    
    # 启动API服务器
    api_thread = threading.Thread(target=run_api_server)
    api_thread.daemon = True
    api_thread.start()
    
    # 如果是MCP模式，启动MCP服务器
    if mcp_mode:
        mcp_thread = threading.Thread(target=run_mcp_server)
        mcp_thread.daemon = True
        mcp_thread.start()
    else:
        # 非MCP模式才启动WebViewer和打开浏览器
        web_thread = threading.Thread(target=run_web_viewer)
        web_thread.daemon = True
        web_thread.start()
        
        # 打开文档查看器（除非指定了--no-browser）
        if not args.no_browser:
            doc_thread = threading.Thread(target=open_documentation)
            doc_thread.daemon = True
            doc_thread.start()
    
    print("\n知识库系统已启动!")
    print("API服务器运行在: http://localhost:5000")
    
    if mcp_mode:
        print("MCP服务器运行在: http://localhost:8000")
        print("\n使用MCP客户端工具:")
        print("python KnowledgeBase/mcp_client.py categories")
    else:
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