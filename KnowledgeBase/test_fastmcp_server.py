#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库FastMCP服务器测试脚本

这个脚本用于测试FastMCP服务器的功能，
确保在部署到Smithery.ai平台前一切正常。
"""

import os
import sys
import json
import time
import subprocess
from rich.console import Console
from rich.panel import Panel

# 控制台输出美化
console = Console()

# 获取项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

def check_fastmcp_installed():
    """检查FastMCP是否已安装"""
    try:
        import fastmcp
        return True
    except ImportError:
        console.print("[red]错误: FastMCP未安装[/red]")
        console.print("请先安装FastMCP:")
        console.print("uv pip install fastmcp")
        return False

def check_server_script():
    """检查服务器脚本是否存在"""
    server_path = os.path.join(SCRIPT_DIR, "ugui_kb_server.py")
    if not os.path.exists(server_path):
        console.print("[red]错误: ugui_kb_server.py 不存在[/red]")
        console.print("请先运行 deploy_to_smithery.py --create-server 创建服务器脚本")
        return False
    return True

def check_config_file():
    """检查配置文件是否存在"""
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    if not os.path.exists(config_path):
        console.print("[red]错误: mcp_config.json 不存在[/red]")
        return False
    
    # 检查配置文件格式
    try:
        with open(config_path, 'r', encoding='utf-8') as f:
            config = json.load(f)
        return True
    except json.JSONDecodeError:
        console.print("[red]错误: mcp_config.json 格式不正确[/red]")
        return False

def test_server_startup():
    """测试服务器启动"""
    server_path = os.path.join(SCRIPT_DIR, "ugui_kb_server.py")
    
    console.print("[bold]正在测试FastMCP服务器启动...[/bold]")
    
    try:
        # 启动服务器进程
        process = subprocess.Popen(
            [sys.executable, server_path],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        
        # 等待服务器启动
        console.print("等待服务器启动...")
        time.sleep(3)
        
        # 检查服务器是否正常运行
        if process.poll() is None:
            console.print("[green]服务器启动成功![/green]")
            
            # 终止服务器进程
            process.terminate()
            process.wait()
            
            return True
        else:
            stdout, stderr = process.communicate()
            console.print("[red]服务器启动失败![/red]")
            console.print(f"标准输出:\n{stdout}")
            console.print(f"错误输出:\n{stderr}")
            return False
    
    except Exception as e:
        console.print(f"[red]测试过程中出错: {str(e)}[/red]")
        return False

def test_docs_directory():
    """测试文档目录结构"""
    docs_dir = os.path.join(ROOT_DIR, "Docs")
    if not os.path.exists(docs_dir):
        console.print("[red]警告: Docs目录不存在[/red]")
        return False
    
    # 检查配置文件中的分类是否在文档目录中存在
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    missing_categories = []
    for category in config.get('categories', {}).keys():
        category_dir = os.path.join(docs_dir, category)
        if not os.path.exists(category_dir):
            missing_categories.append(category)
    
    if missing_categories:
        console.print("[yellow]警告: 以下分类在Docs目录中不存在:[/yellow]")
        for category in missing_categories:
            console.print(f"- {category}")
        return False
    
    console.print("[green]文档目录结构检查通过[/green]")
    return True

def test_web_viewer():
    """测试WebViewer目录"""
    web_dir = os.path.join(ROOT_DIR, "WebViewer")
    if not os.path.exists(web_dir):
        console.print("[red]警告: WebViewer目录不存在[/red]")
        return False
    
    # 检查配置文件中的可视化图表是否在WebViewer目录中存在
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    missing_visualizations = []
    for vis_id in config.get('visualizations', {}).keys():
        vis_file = os.path.join(web_dir, f"{vis_id}.html")
        if not os.path.exists(vis_file):
            missing_visualizations.append(vis_id)
    
    if missing_visualizations:
        console.print("[yellow]警告: 以下可视化图表在WebViewer目录中不存在:[/yellow]")
        for vis_id in missing_visualizations:
            console.print(f"- {vis_id}.html")
        return False
    
    console.print("[green]WebViewer目录检查通过[/green]")
    return True

def main():
    """主函数"""
    console.print(Panel("[bold]UGUI知识库FastMCP服务器测试[/bold]"))
    
    # 检查FastMCP是否已安装
    if not check_fastmcp_installed():
        return
    
    # 检查服务器脚本是否存在
    if not check_server_script():
        return
    
    # 检查配置文件是否存在
    if not check_config_file():
        return
    
    # 测试服务器启动
    if not test_server_startup():
        return
    
    # 测试文档目录结构
    test_docs_directory()
    
    # 测试WebViewer目录
    test_web_viewer()
    
    console.print(Panel("[bold green]测试完成![/bold green]"))
    console.print("您的FastMCP服务器已准备就绪，可以部署到Smithery.ai平台")
    console.print("运行以下命令开始部署:")
    console.print("python deploy_to_smithery.py --deploy")

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        console.print("\n[yellow]测试已取消[/yellow]")
    except Exception as e:
        console.print(f"\n[red]测试过程中出错: {str(e)}[/red]")