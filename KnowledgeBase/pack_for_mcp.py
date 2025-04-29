#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库MCP打包工具

这个脚本用于将UGUI知识库打包成可分发的MCP格式，
支持将知识库内容、API和客户端工具打包成一个独立的目录，
可以直接分发到MCP平台使用。
"""

import os
import sys
import json
import shutil
import argparse
from rich.console import Console

# 控制台输出美化
console = Console()

# 获取项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

def create_mcp_config(output_dir):
    """创建MCP平台配置文件"""
    mcp_config = {
        "name": "UGUI知识库",
        "version": "1.0.0",
        "description": "Unity UGUI系统的结构化知识库（本地模式）",
        "type": "knowledge_base",
        "entry_point": "mcp_client.py",
        "run_mode": "local",
        "commands": {
            "categories": "列出所有文档分类",
            "documents": "列出指定分类下的所有文档",
            "document": "获取指定文档",
            "search": "搜索文档",
            "visualizations": "列出所有可视化图表",
            "visualize": "获取可视化图表",
            "example": "获取代码示例"
        },
        "dependencies": [
            "flask",
            "flask_cors",
            "markdown",
            "requests",
            "rich"
        ]
    }
    
    config_path = os.path.join(output_dir, "mcp_platform.json")
    with open(config_path, 'w', encoding='utf-8') as f:
        json.dump(mcp_config, f, ensure_ascii=False, indent=2)
    
    console.print(f"[green]已创建MCP平台配置文件: {config_path}[/green]")

def update_client_config(output_dir):
    """更新客户端配置，设置为本地模式"""
    config_path = os.path.join(output_dir, "KnowledgeBase", "mcp_config.json")
    
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    # 修改为本地模式配置
    config["local_mode"] = True
    
    with open(config_path, 'w', encoding='utf-8') as f:
        json.dump(config, f, ensure_ascii=False, indent=2)
    
    console.print(f"[green]已更新客户端配置为本地模式: {config_path}[/green]")

def create_readme(output_dir):
    """创建README文件"""
    readme_content = """\
# UGUI知识库 - MCP平台分发包

这是UGUI知识库的MCP平台分发包，可以在MCP平台上直接使用，无需依赖网络API。

## 使用方法

### 1. 在MCP平台上安装

将此目录上传到MCP平台的工具目录，然后在MCP平台上注册此工具。

### 2. 使用命令

```bash
# 查看所有文档分类
mcp ugui-kb categories

# 查看指定分类下的文档
mcp ugui-kb documents architecture

# 查看指定文档
mcp ugui-kb document architecture basic

# 搜索文档
mcp ugui-kb search "Canvas"

# 查看所有可视化图表
mcp ugui-kb visualizations

# 打开可视化图表
mcp ugui-kb visualize architecture
```

## 本地模式说明

此分发包使用本地模式运行，所有数据都存储在本地文件系统中，不依赖网络API。

## 依赖项

- Python 3.6+
- Flask
- Flask-CORS
- Markdown
- Requests
- Rich

## 许可证

版权所有 © 2023
"""
    
    readme_path = os.path.join(output_dir, "README.md")
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(readme_content)
    
    console.print(f"[green]已创建README文件: {readme_path}[/green]")

def pack_knowledge_base(output_dir):
    """打包知识库为MCP分发包"""
    # 创建输出目录
    if os.path.exists(output_dir):
        shutil.rmtree(output_dir)
    os.makedirs(output_dir)
    console.print(f"[green]已创建输出目录: {output_dir}[/green]")
    
    # 复制知识库目录
    kb_output_dir = os.path.join(output_dir, "KnowledgeBase")
    os.makedirs(kb_output_dir)
    
    # 复制必要的Python文件
    for file in ["api.py", "mcp_client.py", "mcp_config.json", "requirements.txt"]:
        src_path = os.path.join(SCRIPT_DIR, file)
        dst_path = os.path.join(kb_output_dir, file)
        shutil.copy2(src_path, dst_path)
        console.print(f"[green]已复制文件: {file}[/green]")
    
    # 复制文档文件
    docs_dir = os.path.join(ROOT_DIR, "Docs")
    docs_output_dir = os.path.join(output_dir, "Docs")
    if os.path.exists(docs_dir):
        shutil.copytree(docs_dir, docs_output_dir)
        console.print(f"[green]已复制文档目录: Docs[/green]")
    
    # 复制WebViewer目录
    web_viewer_dir = os.path.join(ROOT_DIR, "WebViewer")
    web_viewer_output_dir = os.path.join(output_dir, "WebViewer")
    if os.path.exists(web_viewer_dir):
        shutil.copytree(web_viewer_dir, web_viewer_output_dir)
        console.print(f"[green]已复制WebViewer目录: WebViewer[/green]")
    
    # 复制使用说明和索引文件
    for file in ["使用说明.md", "知识库索引.md", "README.md"]:
        src_path = os.path.join(SCRIPT_DIR, file)
        if os.path.exists(src_path):
            dst_path = os.path.join(kb_output_dir, file)
            shutil.copy2(src_path, dst_path)
            console.print(f"[green]已复制文件: {file}[/green]")
    
    # 创建MCP平台配置文件
    create_mcp_config(output_dir)
    
    # 更新客户端配置
    update_client_config(output_dir)
    
    # 创建README文件
    create_readme(output_dir)
    
    console.print("[bold green]知识库打包完成！[/bold green]")
    console.print(f"输出目录: {os.path.abspath(output_dir)}")
    console.print("可以将此目录上传到MCP平台使用。")

def main():
    """命令行入口函数"""
    parser = argparse.ArgumentParser(description="UGUI知识库MCP打包工具")
    parser.add_argument("--output", "-o", default="mcp_knowledge_pack",
                       help="输出目录名称")
    
    args = parser.parse_args()
    output_dir = os.path.join(os.getcwd(), args.output)
    
    pack_knowledge_base(output_dir)

if __name__ == "__main__":
    main()