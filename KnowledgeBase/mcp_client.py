#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库MCP客户端工具

这个模块提供了通过MCP工具访问UGUI知识库的客户端接口，
可以用于查询文档、搜索内容、获取可视化图表等功能。
支持本地模式和服务器模式两种运行方式。
"""

import os
import json
import requests
import argparse
import webbrowser
import importlib.util
from rich.console import Console
from rich.markdown import Markdown
from rich.panel import Panel
from rich.table import Table

# 控制台输出美化
console = Console()

# 配置文件路径
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_PATH = os.path.join(SCRIPT_DIR, 'mcp_config.json')

# 运行模式：local或server
RUN_MODE = 'server'

class MCPClient:
    """UGUI知识库MCP客户端"""
    
    def __init__(self, config_path=CONFIG_PATH, mode=None):
        """初始化客户端"""
        with open(config_path, 'r', encoding='utf-8') as f:
            self.config = json.load(f)
        
        self.base_url = self.config['api']['base_url']
        self.endpoints = self.config['api']['endpoints']
        self.categories = self.config['categories']
        self.visualizations = self.config['visualizations']
        
        # 设置运行模式
        global RUN_MODE
        if mode:
            RUN_MODE = mode
        
        # 本地模式下，导入本地API
        self.local_api = None
        if RUN_MODE == 'local':
            try:
                # 动态导入api模块
                api_path = os.path.join(SCRIPT_DIR, 'api.py')
                spec = importlib.util.spec_from_file_location("api", api_path)
                api_module = importlib.util.module_from_spec(spec)
                spec.loader.exec_module(api_module)
                
                # 获取本地API实例
                self.local_api = api_module.local_api
                console.print("[green]已加载本地API模式[/green]")
            except Exception as e:
                console.print(f"[red]加载本地API失败: {str(e)}[/red]")
                console.print("[yellow]将使用服务器模式[/yellow]")
                RUN_MODE = 'server'
    
    def get_index(self):
        """获取知识库索引"""
        if RUN_MODE == 'local' and self.local_api:
            return self.local_api.get_index()
        else:
            url = f"{self.base_url}{self.endpoints['index']['path']}"
            response = requests.get(url)
            return response.json()
    
    def get_document(self, category, doc_id):
        """获取指定文档"""
        if RUN_MODE == 'local' and self.local_api:
            return self.local_api.get_document(category, doc_id)
        else:
            url = f"{self.base_url}{self.endpoints['document']['path']}"
            url = url.replace('{category}', category).replace('{doc_id}', doc_id)
            response = requests.get(url)
            return response.json()
    
    def search(self, keyword):
        """搜索文档"""
        if RUN_MODE == 'local' and self.local_api:
            return self.local_api.search_docs(keyword)
        else:
            url = f"{self.base_url}{self.endpoints['search']['path']}?keyword={keyword}"
            response = requests.get(url)
            return response.json()
    
    def get_visualization(self, diagram_id):
        """获取可视化图表"""
        if RUN_MODE == 'local' and self.local_api:
            # 本地模式下直接打开HTML文件
            viz_path = self.local_api.visualization_index.get(diagram_id)
            if not viz_path or not os.path.exists(viz_path):
                return {"error": "可视化图表不存在"}
            
            # 使用file://协议打开本地HTML文件
            webbrowser.open(f"file://{os.path.abspath(viz_path)}")
            return {"status": "success", "message": f"已在浏览器中打开{self.visualizations[diagram_id]}"}
        else:
            url = f"{self.base_url}{self.endpoints['visualization']['path']}"
            url = url.replace('{diagram_id}', diagram_id)
            # 直接在浏览器中打开可视化页面
            webbrowser.open(url)
            return {"status": "success", "message": f"已在浏览器中打开{self.visualizations[diagram_id]}"}
    
    def get_example(self, example_id):
        """获取代码示例"""
        if RUN_MODE == 'local' and self.local_api:
            # 本地模式下直接读取文件
            examples_dir = os.path.join(os.path.dirname(self.local_api.root_dir), 'Assets', 'Scripts')
            example_path = os.path.join(examples_dir, f"{example_id}.cs")
            
            if not os.path.exists(example_path):
                return {"error": "代码示例不存在"}
            
            try:
                with open(example_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    return {
                        'id': example_id,
                        'content': content
                    }
            except Exception as e:
                return {"error": f"读取代码示例失败: {str(e)}"}
        else:
            url = f"{self.base_url}{self.endpoints['example']['path']}"
            url = url.replace('{example_id}', example_id)
            response = requests.get(url)
            return response.json()
    
    def display_categories(self):
        """显示所有文档分类"""
        table = Table(title="UGUI知识库分类")
        table.add_column("分类ID", style="cyan")
        table.add_column("分类名称", style="green")
        table.add_column("文档数量", style="magenta")
        
        for category_id, category in self.categories.items():
            table.add_row(
                category_id,
                category['name'],
                str(len(category['documents']))
            )
        
        console.print(table)
    
    def display_documents(self, category):
        """显示指定分类下的所有文档"""
        if category not in self.categories:
            console.print(f"[red]错误: 分类 '{category}' 不存在[/red]")
            return
        
        table = Table(title=f"{self.categories[category]['name']}文档列表")
        table.add_column("文档ID", style="cyan")
        table.add_column("文档名称", style="green")
        
        for doc_id, doc_name in self.categories[category]['documents'].items():
            table.add_row(doc_id, doc_name)
        
        console.print(table)
    
    def display_visualizations(self):
        """显示所有可视化图表"""
        table = Table(title="UGUI可视化图表")
        table.add_column("图表ID", style="cyan")
        table.add_column("图表名称", style="green")
        
        for viz_id, viz_name in self.visualizations.items():
            table.add_row(viz_id, viz_name)
        
        console.print(table)
    
    def display_document(self, doc_data):
        """显示文档内容"""
        if 'error' in doc_data:
            console.print(f"[red]错误: {doc_data['error']}[/red]")
            return
        
        # 使用Rich的Markdown渲染器显示文档内容
        md = Markdown(doc_data['content'])
        console.print(Panel(md, title=doc_data['id'], subtitle=doc_data['category']))
    
    def display_search_results(self, search_data):
        """显示搜索结果"""
        if 'error' in search_data:
            console.print(f"[red]错误: {search_data['error']}[/red]")
            return
        
        console.print(f"关键词 '[cyan]{search_data['keyword']}[/cyan]' 的搜索结果: {search_data['count']} 个匹配")
        
        for i, result in enumerate(search_data['results'], 1):
            console.print(f"\n[bold green]{i}. {result['title']}[/bold green]")
            console.print(f"   分类: {result['category']}")
            console.print(f"   文档ID: {result['id']}")
            console.print(f"   路径: {result['path']}")
            console.print("   匹配内容:")
            
            for match in result['matches']:
                console.print(Panel(
                    match['context'],
                    title=f"第 {match['line']} 行",
                    border_style="yellow"
                ))

def main():
    """命令行入口函数"""
    parser = argparse.ArgumentParser(description="UGUI知识库MCP客户端工具")
    
    # 添加全局参数
    parser.add_argument("--mode", choices=["local", "server"], default=None,
                        help="运行模式：local(本地模式)或server(服务器模式)")
    
    subparsers = parser.add_subparsers(dest="command", help="命令")
    
    # 列出分类
    subparsers.add_parser("categories", help="列出所有文档分类")
    
    # 列出文档
    docs_parser = subparsers.add_parser("documents", help="列出指定分类下的所有文档")
    docs_parser.add_argument("category", help="分类ID")
    
    # 获取文档
    doc_parser = subparsers.add_parser("document", help="获取指定文档")
    doc_parser.add_argument("category", help="分类ID")
    doc_parser.add_argument("doc_id", help="文档ID")
    
    # 搜索
    search_parser = subparsers.add_parser("search", help="搜索文档")
    search_parser.add_argument("keyword", help="搜索关键词")
    
    # 列出可视化图表
    subparsers.add_parser("visualizations", help="列出所有可视化图表")
    
    # 获取可视化图表
    viz_parser = subparsers.add_parser("visualize", help="获取可视化图表")
    viz_parser.add_argument("diagram_id", help="图表ID")
    
    # 获取代码示例
    example_parser = subparsers.add_parser("example", help="获取代码示例")
    example_parser.add_argument("example_id", help="示例ID")
    
    # 打包命令
    pack_parser = subparsers.add_parser("pack", help="打包知识库为MCP分发包")
    pack_parser.add_argument("--output", "-o", default="mcp_knowledge_pack",
                           help="输出目录名称")
    
    args = parser.parse_args()
    
    # 设置运行模式
    if args.mode:
        global RUN_MODE
        RUN_MODE = args.mode
        console.print(f"[green]运行模式: {RUN_MODE}[/green]")
    
    client = MCPClient(mode=args.mode)
    
    if args.command == "categories":
        client.display_categories()
    
    elif args.command == "documents":
        client.display_documents(args.category)
    
    elif args.command == "document":
        doc_data = client.get_document(args.category, args.doc_id)
        client.display_document(doc_data)
    
    elif args.command == "search":
        search_data = client.search(args.keyword)
        client.display_search_results(search_data)
    
    elif args.command == "visualizations":
        client.display_visualizations()
    
    elif args.command == "visualize":
        result = client.get_visualization(args.diagram_id)
        if "error" in result:
            console.print(f"[red]错误: {result['error']}[/red]")
        else:
            console.print(f"[green]{result['message']}[/green]")
    
    elif args.command == "example":
        example_data = client.get_example(args.example_id)
        if "error" in example_data:
            console.print(f"[red]错误: {example_data['error']}[/red]")
        else:
            console.print(Panel(
                example_data['content'],
                title=f"代码示例: {example_data['id']}",
                border_style="green"
            ))
    
    elif args.command == "pack":
        # 调用打包脚本
        try:
            from pack_for_mcp import pack_knowledge_base
            output_dir = os.path.join(os.getcwd(), args.output)
            console.print(f"[bold]开始打包知识库到: {output_dir}[/bold]")
            pack_knowledge_base(output_dir)
        except Exception as e:
            console.print(f"[red]打包失败: {str(e)}[/red]")
            console.print("请确保pack_for_mcp.py文件存在且可执行。")
            return 1
    
    else:
        parser.print_help()

if __name__ == "__main__":
    main()