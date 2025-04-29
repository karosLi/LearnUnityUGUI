#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库Smithery.ai部署工具

这个脚本用于将UGUI知识库打包并部署到Smithery.ai平台，
实现基于FastMCP的MCP工具分发。
"""

import os
import sys
import json
import shutil
import argparse
import subprocess
import time
from rich.console import Console
from rich.panel import Panel
from rich.progress import Progress

# 控制台输出美化
console = Console()

# 获取项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

def check_dependencies():
    """检查依赖是否已安装"""
    missing_deps = []
    has_uv = False
    has_npx = False
    
    # 检查Python依赖
    try:
        import fastmcp
    except ImportError:
        missing_deps.append("fastmcp")
    
    # 检查uv是否可用
    try:
        result = subprocess.run(["uv", "--version"], capture_output=True, text=True)
        if result.returncode == 0:
            has_uv = True
    except FileNotFoundError:
        pass
    
    # 检查Node.js和npx是否可用
    try:
        result = subprocess.run(["npm", "-v"], capture_output=True, text=True)
        if result.returncode == 0:
            # Node.js已安装，检查npx
            try:
                result = subprocess.run(["npx", "--version"], capture_output=True, text=True)
                if result.returncode == 0:
                    has_npx = True
            except FileNotFoundError:
                pass
        else:
            missing_deps.append("Node.js")
    except FileNotFoundError:
        missing_deps.append("Node.js")
    
    # 检查Smithery CLI
    smithery_installed = False
    try:
        result = subprocess.run(["smithery", "-v"], capture_output=True, text=True)
        if result.returncode == 0:
            smithery_installed = True
    except FileNotFoundError:
        # 检查是否可以通过npx使用smithery
        if has_npx:
            try:
                result = subprocess.run(["npx", "-y", "@smithery/cli@latest", "-v"], capture_output=True, text=True)
                if result.returncode == 0:
                    smithery_installed = True
            except Exception:
                pass
    
    if not smithery_installed:
        missing_deps.append("@smithery/cli")
    
    if missing_deps:
        console.print(Panel("[bold red]缺少必要依赖[/bold red]"))
        console.print("请安装以下依赖:")
        
        if "fastmcp" in missing_deps:
            console.print("[yellow]安装 FastMCP:[/yellow]")
            if has_uv:
                # 检查是否存在虚拟环境
                venv_exists = False
                try:
                    # 检查VIRTUAL_ENV环境变量
                    if os.environ.get('VIRTUAL_ENV'):
                        venv_exists = True
                except Exception:
                    pass
                
                if venv_exists:
                    console.print("uv pip install fastmcp")
                else:
                    console.print("[bold]选项1: 创建并使用虚拟环境[/bold]")
                    console.print("uv venv && uv pip install fastmcp")
                    console.print("[bold]选项2: 直接安装到系统环境[/bold]")
                    console.print("uv pip install --system fastmcp")
            else:
                console.print("pip install fastmcp")
        
        if "Node.js" in missing_deps:
            console.print("[yellow]安装 Node.js:[/yellow]")
            console.print("请访问 https://nodejs.org/ 下载并安装")
        
        if "@smithery/cli" in missing_deps:
            console.print("[yellow]安装 Smithery CLI:[/yellow]")
            if has_npx:
                console.print("npx -y @smithery/cli@latest 或 npm install -g @smithery/cli")
            else:
                console.print("npm install -g @smithery/cli")
        
        return False
    
    return True, has_uv, has_npx

def create_ugui_kb_server():
    """创建FastMCP服务器脚本"""
    server_path = os.path.join(SCRIPT_DIR, "ugui_kb_server.py")
    
    # 如果文件已存在，询问是否覆盖
    if os.path.exists(server_path):
        console.print("[yellow]警告: ugui_kb_server.py 已存在[/yellow]")
        response = input("是否覆盖? (y/n): ")
        if response.lower() != 'y':
            console.print("[yellow]跳过创建 ugui_kb_server.py[/yellow]")
            return
    
    # 从实施方案中提取的服务器代码
    server_code = '''#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库MCP服务器

这个模块提供了基于FastMCP的UGUI知识库服务器，
可以用于查询文档、搜索内容、获取可视化图表等功能。
"""

import os
import sys
import json

# 尝试导入rich模块，如果失败则安装
try:
    from rich.console import Console
    console = Console()
except ImportError:
    print("正在安装rich模块...")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "rich"])
    from rich.console import Console
    console = Console()

# 尝试导入fastmcp模块，如果失败则安装
try:
    from fastmcp import FastMCP
except ImportError:
    console.print("[yellow]正在安装fastmcp模块...[/yellow]")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "fastmcp"])
    from fastmcp import FastMCP

# 获取脚本目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

# 检查API密钥
API_KEY = os.environ.get('UGUI_KB_API_KEY')

# 创建MCP服务器
mcp = FastMCP("UGUI知识库", dependencies=["flask", "flask_cors", "markdown", "requests", "rich"])

# 加载配置
with open(os.path.join(SCRIPT_DIR, 'mcp_config.json'), 'r', encoding='utf-8') as f:
    config = json.load(f)

# 定义工具：获取文档分类
@mcp.tool()
def categories() -> list:
    """获取所有文档分类"""
    # 如果设置了API密钥，检查是否匹配
    if API_KEY and os.environ.get('UGUI_KB_API_KEY') != API_KEY:
        return {"error": "API密钥验证失败"}
    return list(config['categories'].keys())

# 定义工具：获取指定分类下的文档
@mcp.tool()
def documents(category: str) -> list:
    """获取指定分类下的所有文档
    
    Args:
        category: 文档分类名称
    """
    # 如果设置了API密钥，检查是否匹配
    if API_KEY and os.environ.get('UGUI_KB_API_KEY') != API_KEY:
        return {"error": "API密钥验证失败"}
    
    if category not in config['categories']:
        return []
    return config['categories'][category]['documents']

# 定义工具：获取指定文档
@mcp.tool()
def document(category: str, doc_id: str) -> str:
    """获取指定文档的内容
    
    Args:
        category: 文档分类名称
        doc_id: 文档ID
    """
    # 如果设置了API密钥，检查是否匹配
    if API_KEY and os.environ.get('UGUI_KB_API_KEY') != API_KEY:
        return {"error": "API密钥验证失败"}
    
    # 实现文档读取逻辑
    doc_path = os.path.join(SCRIPT_DIR, '..', 'Docs', category, f"{doc_id}.md")
    if not os.path.exists(doc_path):
        return f"文档不存在: {category}/{doc_id}"
    
    with open(doc_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    return content

# 定义工具：搜索文档
@mcp.tool()
def search(keyword: str) -> list:
    """搜索文档内容
    
    Args:
        keyword: 搜索关键词
    """
    # 如果设置了API密钥，检查是否匹配
    if API_KEY and os.environ.get('UGUI_KB_API_KEY') != API_KEY:
        return {"error": "API密钥验证失败"}
    
    # 实现搜索逻辑
    results = []
    for category, info in config['categories'].items():
        for doc_id in info['documents']:
            doc_path = os.path.join(SCRIPT_DIR, '..', 'Docs', category, f"{doc_id}.md")
            if os.path.exists(doc_path):
                with open(doc_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                if keyword.lower() in content.lower():
                    results.append({
                        "category": category,
                        "doc_id": doc_id,
                        "title": info['documents'][doc_id],
                        "preview": content[:200] + "..."
                    })
    return results

# 定义工具：获取所有可视化图表
@mcp.tool()
def visualizations() -> list:
    """获取所有可视化图表"""
    # 如果设置了API密钥，检查是否匹配
    if API_KEY and os.environ.get('UGUI_KB_API_KEY') != API_KEY:
        return {"error": "API密钥验证失败"}
    
    return list(config['visualizations'].keys())

# 定义工具：获取可视化图表
@mcp.tool()
def visualize(diagram_id: str) -> str:
    """获取可视化图表
    
    Args:
        diagram_id: 图表ID
    """
    # 如果设置了API密钥，检查是否匹配
    if API_KEY and os.environ.get('UGUI_KB_API_KEY') != API_KEY:
        return {"error": "API密钥验证失败"}
    
    if diagram_id not in config['visualizations']:
        return f"图表不存在: {diagram_id}"
    
    # 返回图表HTML路径
    return os.path.join(SCRIPT_DIR, '..', 'WebViewer', f"{diagram_id}.html")

# 主函数
if __name__ == "__main__":
    mcp.run()
'''
    
    with open(server_path, 'w', encoding='utf-8') as f:
        f.write(server_code)
    
    # 设置执行权限
    os.chmod(server_path, 0o755)
    
    console.print(f"[green]已创建FastMCP服务器脚本: {server_path}[/green]")

def create_dockerfile():
    """创建Dockerfile"""
    dockerfile_path = os.path.join(ROOT_DIR, "Dockerfile")
    
    # 如果文件已存在，询问是否覆盖
    if os.path.exists(dockerfile_path):
        console.print("[yellow]警告: Dockerfile 已存在[/yellow]")
        response = input("是否覆盖? (y/n): ")
        if response.lower() != 'y':
            console.print("[yellow]跳过创建 Dockerfile[/yellow]")
            return
    
    dockerfile_content = '''FROM python:3.9-slim

WORKDIR /app

# 复制必要的文件
COPY KnowledgeBase/ /app/KnowledgeBase/
COPY Docs/ /app/Docs/
COPY WebViewer/ /app/WebViewer/

# 安装依赖 - 使用pip安装所有必要的依赖
RUN pip install --no-cache-dir fastmcp flask flask_cors markdown requests rich

# 确保rich模块正确安装
RUN pip install --no-cache-dir rich --upgrade

# 设置工作目录
WORKDIR /app/KnowledgeBase

# 命令将由smithery.yaml提供
CMD ["python", "ugui_kb_server.py"]
'''
    
    with open(dockerfile_path, 'w', encoding='utf-8') as f:
        f.write(dockerfile_content)
    
    console.print(f"[green]已创建Dockerfile: {dockerfile_path}[/green]")

def create_smithery_yaml():
    """创建smithery.yaml配置文件"""
    yaml_path = os.path.join(ROOT_DIR, "smithery.yaml")
    
    # 如果文件已存在，询问是否覆盖
    if os.path.exists(yaml_path):
        console.print("[yellow]警告: smithery.yaml 已存在[/yellow]")
        response = input("是否覆盖? (y/n): ")
        if response.lower() != 'y':
            console.print("[yellow]跳过创建 smithery.yaml[/yellow]")
            return
    
    # 从配置文件中获取项目信息
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    # 获取用户信息
    author_name = input("请输入作者姓名: ")
    author_email = input("请输入作者邮箱: ")
    
    # 询问是否需要API密钥
    use_api_key = input("是否需要API密钥进行身份验证? (y/n): ").lower() == 'y'
    api_key_config = ""
    if use_api_key:
        api_key_config = '''
  configSchema:
    type: object
    properties:
      key:
        type: string
        description: "API密钥用于身份验证"
    required: ["key"]
'''
    else:
        api_key_config = '''
  configSchema:
    type: object
    properties: {}
    required: []
'''
    
    yaml_content = f'''name: ugui-knowledge-base
description: {config.get('description', 'Unity UGUI系统的结构化知识库')}
version: {config.get('version', '1.0.0')}
authors:
  - name: {author_name}
    email: {author_email}

startCommand:
  type: stdio{api_key_config}
  commandFunction: |
    function getCommand(config) {{
      const args = ["KnowledgeBase/ugui_kb_server.py"];
      const env = {{}};
      
      // 如果提供了API密钥，添加到环境变量
      if (config.key) {{
        env.UGUI_KB_API_KEY = config.key;
      }}
      
      return {{
        command: "python",
        args: args,
        env: env
      }};
    }}

build:
  dockerfile: Dockerfile
  dockerBuildPath: .
'''
    
    with open(yaml_path, 'w', encoding='utf-8') as f:
        f.write(yaml_content)
    
    console.print(f"[green]已创建smithery.yaml: {yaml_path}[/green]")

def test_local_server(has_npx=False):
    """测试本地MCP服务器"""
    server_path = os.path.join(SCRIPT_DIR, "ugui_kb_server.py")
    
    if not os.path.exists(server_path):
        console.print("[red]错误: ugui_kb_server.py 不存在，请先创建服务器脚本[/red]")
        return False
    
    console.print("[bold]正在测试本地MCP服务器...[/bold]")
    
    # 检查是否需要测试API密钥功能
    test_api_key = input("是否测试API密钥功能? (y/n): ").lower() == 'y'
    env = None
    
    if test_api_key:
        api_key = input("请输入测试用API密钥: ")
        env = os.environ.copy()
        env['UGUI_KB_API_KEY'] = api_key
        console.print(f"[yellow]使用API密钥: {api_key}[/yellow]")
    
    try:
        # 启动服务器进程
        process = subprocess.Popen(
            [sys.executable, server_path],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            env=env
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
            
            # 如果使用npx，也测试通过npx运行
            if has_npx:
                console.print("\n[bold]测试通过npx运行服务器...[/bold]")
                console.print("[yellow]注意: 这将需要先部署到Smithery.ai平台[/yellow]")
                test_npx = input("是否测试通过npx运行? (y/n): ").lower() == 'y'
                
                if test_npx:
                    # 获取包名
                    yaml_path = os.path.join(ROOT_DIR, "smithery.yaml")
                    if os.path.exists(yaml_path):
                        with open(yaml_path, 'r', encoding='utf-8') as f:
                            for line in f:
                                if line.startswith('name:'):
                                    package_name = line.split(':', 1)[1].strip()
                                    break
                            else:
                                package_name = "ugui-knowledge-base"
                    else:
                        package_name = "ugui-knowledge-base"
                    
                    npx_cmd = ["npx", "-y", "@smithery/cli@latest", "run", package_name]
                    if test_api_key:
                        npx_cmd.extend(["--key", api_key])
                    
                    console.print(f"[bold]运行命令: {' '.join(npx_cmd)}[/bold]")
                    console.print("[yellow]按Ctrl+C终止测试[/yellow]")
                    
                    try:
                        npx_process = subprocess.Popen(npx_cmd)
                        input("按Enter键终止测试...")
                        npx_process.terminate()
                        npx_process.wait()
                    except KeyboardInterrupt:
                        console.print("[yellow]测试已终止[/yellow]")
                    except Exception as e:
                        console.print(f"[red]npx测试出错: {str(e)}[/red]")
            
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

def deploy_to_github():
    """部署到GitHub仓库"""
    # 创建GitHub Actions工作流
    workflow_dir = os.path.join(ROOT_DIR, ".github", "workflows")
    os.makedirs(workflow_dir, exist_ok=True)
    workflow_path = os.path.join(workflow_dir, "deploy.yml")
    
    workflow_content = """
name: Deploy UGUI Knowledge Base

on:
  push:
    branches: [ "main" ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2
      - name: Login to GitHub Container Registry
        uses: docker/login-action@v2
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - name: Build and push Docker image
        uses: docker/build-push-action@v4
        with:
          context: .
          push: true
          tags: |
            ghcr.io/${{ github.repository }}:latest
            ghcr.io/${{ github.repository }}:${{ github.sha }}
      - name: Create package.json
        run: |
          echo '{
            "name": "@${{ github.repository_owner }}/ugui-knowledge-base",
            "version": "1.0.0",
            "description": "UGUI Knowledge Base MCP Server",
            "repository": "github.com/${{ github.repository }}",
            "main": "KnowledgeBase/ugui_kb_server.py",
            "scripts": {
              "serve": "python KnowledgeBase/ugui_kb_server.py"
            },
            "dependencies": {
              "fastmcp": "^1.2.0"
            }
          }' > package.json
"""
    
    with open(workflow_path, 'w', encoding='utf-8') as f:
        f.write(workflow_content)
    
    console.print(f"[green]已创建GitHub Actions工作流文件: {workflow_path}[/green]")
    
    # 创建package.json
    package_path = os.path.join(ROOT_DIR, "package.json")
    package_content = '''{
  "name": "@your-github-org/ugui-knowledge-base",
  "version": "1.0.0",
  "description": "UGUI Knowledge Base MCP Server",
  "repository": "https://github.com/your-org/ugui-knowledge-base",
  "main": "KnowledgeBase/ugui_kb_server.py",
  "scripts": {
    "serve": "python KnowledgeBase/ugui_kb_server.py"
  },
  "dependencies": {
    "fastmcp": "^1.2.0"
  }
}'''
    
    with open(package_path, 'w', encoding='utf-8') as f:
        f.write(package_content)
    
    console.print(f"[green]已创建package.json文件: {package_path}[/green]")
    
    return True
    """部署到GitHub仓库"""
    # 检查是否在Git仓库中
    if not os.path.exists(os.path.join(ROOT_DIR, ".git")):
        console.print("[red]错误: 当前目录不是Git仓库[/red]")
        return False

    # 创建GitHub工作流
        create_github_workflow()

        # 添加package.json
        create_package_json()

        console.print("\n[bold]部署准备完成，请执行以下命令:[/bold]")
        console.print("git add .")
        console.print("git commit -m '准备MCP服务器部署'")
        console.print("git push origin main")
        return True


def deploy_to_smithery(has_npx=False):
    """部署到Smithery.ai平台"""
    # 检查必要文件是否存在
    dockerfile_path = os.path.join(ROOT_DIR, "Dockerfile")
    yaml_path = os.path.join(ROOT_DIR, "smithery.yaml")
    server_path = os.path.join(SCRIPT_DIR, "ugui_kb_server.py")
    
    missing_files = []
    if not os.path.exists(dockerfile_path):
        missing_files.append("Dockerfile")
    if not os.path.exists(yaml_path):
        missing_files.append("smithery.yaml")
    if not os.path.exists(server_path):
        missing_files.append("ugui_kb_server.py")
    
    if missing_files:
        console.print("[red]错误: 缺少以下必要文件:[/red]")
        for file in missing_files:
            console.print(f"- {file}")
        console.print("请先创建这些文件再进行部署")
        return False
    
    # 登录Smithery
    console.print("[bold]登录Smithery.ai平台...[/bold]")
    
    if has_npx:
        login_cmd = ["npx", "-y", "@smithery/cli@latest", "login"]
    else:
        login_cmd = ["smithery", "login"]
    
    login_result = subprocess.run(login_cmd, capture_output=False)
    
    if login_result.returncode != 0:
        console.print("[red]登录失败，请检查网络连接或重新登录[/red]")
        return False
    
    # 部署到Smithery
    console.print("[bold]正在部署到Smithery.ai平台...[/bold]")
    
    if has_npx:
        deploy_cmd = ["npx", "-y", "@smithery/cli@latest", "deploy", ROOT_DIR]
    else:
        deploy_cmd = ["smithery", "deploy", ROOT_DIR]
    
    deploy_result = subprocess.run(deploy_cmd, capture_output=False)
    
    if deploy_result.returncode != 0:
        console.print("[red]部署失败，请检查错误信息[/red]")
        return False
    
    # 获取包名
    with open(yaml_path, 'r', encoding='utf-8') as f:
        for line in f:
            if line.startswith('name:'):
                package_name = line.split(':', 1)[1].strip()
                break
        else:
            package_name = "ugui-knowledge-base"
    
    console.print(Panel("[bold green]部署成功![/bold green]"))
    console.print("\n您的UGUI知识库已成功部署到Smithery.ai平台")
    console.print("\n[bold]在Claude Desktop中使用方法:[/bold]")
    console.print("1. 打开Claude Desktop设置")
    console.print("2. 添加您的MCP服务器:")
    
    # 检查是否配置了API密钥
    api_key_config = ""
    with open(yaml_path, 'r', encoding='utf-8') as f:
        yaml_content = f.read()
        if '"key"' in yaml_content or "'key'" in yaml_content:
            api_key_config = ', "--key", "您的API密钥"'
    
    console.print(f'''
```json
{{
  "mcpServers": {{
    "ugui-kb": {{
      "command": "npx",
      "args": [
        "-y", 
        "@smithery/cli@latest", 
        "run", 
        "{package_name}"{api_key_config}
      ],
      "env": {{}}
    }}
  }}
}}
```
''')
    
    console.print("3. 重启Claude Desktop")
    console.print("4. 使用以下命令与知识库交互:")
    console.print("   - 获取所有文档分类: `categories()`")
    console.print("   - 获取指定分类下的文档: `documents(\"architecture\")`")
    console.print("   - 获取指定文档: `document(\"architecture\", \"basic\")`")
    console.print("   - 搜索文档: `search(\"Canvas\")`")
    console.print("   - 获取所有可视化图表: `visualizations()`")
    console.print("   - 获取可视化图表: `visualize(\"architecture\")`")
    
    # 添加本地运行说明
    console.print("\n[bold]本地运行MCP服务器:[/bold]")
    if has_npx:
        console.print("使用npx运行:")
        console.print(f"npx -y @smithery/cli@latest run {package_name}")
        if api_key_config:
            console.print(f"npx -y @smithery/cli@latest run {package_name} --key 您的API密钥")
    else:
        console.print("使用smithery CLI运行:")
        console.print(f"smithery run {package_name}")
        if api_key_config:
            console.print(f"smithery run {package_name} --key 您的API密钥")
    
    console.print("\n或直接运行Python脚本:")
    console.print(f"python {server_path}")
    
    return True

def setup_uv_venv():
    """设置uv虚拟环境"""
    # 检查uv是否可用
    try:
        result = subprocess.run(["uv", "--version"], capture_output=True, text=True)
        if result.returncode != 0:
            console.print("[red]错误: uv包管理器未安装或不可用[/red]")
            return False
    except FileNotFoundError:
        console.print("[red]错误: uv包管理器未安装[/red]")
        console.print("请先安装uv: https://github.com/astral-sh/uv")
        return False
    
    # 检查是否已在虚拟环境中
    if os.environ.get('VIRTUAL_ENV'):
        console.print("[green]已检测到虚拟环境: [/green]" + os.environ.get('VIRTUAL_ENV'))
        return True
    
    # 询问用户是否创建虚拟环境
    console.print("[yellow]未检测到虚拟环境[/yellow]")
    console.print("使用uv安装包需要虚拟环境或--system参数")
    choice = input("选择操作: [1]创建虚拟环境 [2]使用--system参数安装 [3]取消: ")
    
    if choice == "1":
        # 创建虚拟环境
        console.print("[bold]正在创建虚拟环境...[/bold]")
        venv_path = os.path.join(os.getcwd(), ".venv")
        try:
            result = subprocess.run(["uv", "venv"], capture_output=True, text=True)
            if result.returncode == 0:
                console.print("[green]虚拟环境创建成功![/green]")
                
                # 提示用户激活虚拟环境
                console.print("[yellow]请激活虚拟环境后再运行此脚本:[/yellow]")
                if sys.platform == "win32":
                    console.print(f".venv\\Scripts\\activate")
                else:
                    console.print(f"source .venv/bin/activate")
                
                return False  # 返回False以便用户激活环境后重新运行
            else:
                console.print("[red]虚拟环境创建失败:[/red]")
                console.print(result.stderr)
                return False
        except Exception as e:
            console.print(f"[red]创建虚拟环境时出错: {str(e)}[/red]")
            return False
    elif choice == "2":
        # 使用--system参数安装
        console.print("[yellow]将使用--system参数安装依赖[/yellow]")
        try:
            console.print("[bold]正在安装fastmcp...[/bold]")
            result = subprocess.run(["uv", "pip", "install", "--system", "fastmcp"], capture_output=True, text=True)
            if result.returncode == 0:
                console.print("[green]fastmcp安装成功![/green]")
                return True
            else:
                console.print("[red]fastmcp安装失败:[/red]")
                console.print(result.stderr)
                return False
        except Exception as e:
            console.print(f"[red]安装fastmcp时出错: {str(e)}[/red]")
            return False
    else:
        console.print("[yellow]操作已取消[/yellow]")
        return False

def create_github_workflow():
    """创建GitHub Actions工作流配置"""
    workflow_dir = os.path.join(ROOT_DIR, ".github", "workflows")
    os.makedirs(workflow_dir, exist_ok=True)
    workflow_path = os.path.join(workflow_dir, "deploy.yml")
    
    if os.path.exists(workflow_path):
        console.print(f"[yellow]工作流文件已存在: {workflow_path}[/yellow]")
        return
    
    workflow_content = '''
name: Deploy UGUI Knowledge Base

on:
  push:
    branches: [main]
    paths:
      - 'KnowledgeBase/**'
      - 'Docs/**'
      - 'WebViewer/**'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        
      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.10'
          
      - name: Install dependencies
        run: |
          python -m pip install --upgrade pip
          pip install fastmcp rich
          
      - name: Generate documentation
        run: python deploy_to_smithery.py --create-server
        
      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./docs
    '''
    
    with open(workflow_path, 'w', encoding='utf-8') as f:
        f.write(workflow_content)
    console.print(f"[green]已创建GitHub工作流文件: {workflow_path}[/green]")
    """创建GitHub Actions工作流配置文件"""
    workflow_dir = os.path.join(ROOT_DIR, ".github", "workflows")
    workflow_path = os.path.join(workflow_dir, "publish.yml")
    
    # 创建目录（如果不存在）
    if not os.path.exists(workflow_dir):
        os.makedirs(workflow_dir)
    
    # 如果文件已存在，询问是否覆盖
    if os.path.exists(workflow_path):
        console.print("[yellow]警告: GitHub Actions工作流配置文件已存在[/yellow]")
        response = input("是否覆盖? (y/n): ")
        if response.lower() != 'y':
            console.print("[yellow]跳过创建GitHub Actions工作流配置文件[/yellow]")
            return
    
    # 从配置文件中获取项目信息
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    # 获取包名
    package_name = config.get("name", "ugui-knowledge-base")
    if isinstance(package_name, str) and package_name.strip():
        package_name = package_name.strip().lower().replace(" ", "-")
    else:
        package_name = "ugui-knowledge-base"
    
    # 创建GitHub Actions工作流配置文件
    workflow_content = f'''name: Publish UGUI Knowledge Base MCP Server

on:
  push:
    branches: [ main, master ]
    tags: [ 'v*' ]
  workflow_dispatch:

jobs:
  build-and-publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.9'
      
      - name: Install dependencies
        run: |
          python -m pip install --upgrade pip
          pip install build twine fastmcp
          pip install -r KnowledgeBase/requirements.txt
      
      - name: Set up Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '16'
          registry-url: 'https://registry.npmjs.org'
      
      - name: Install Smithery CLI
        run: npm install -g @smithery/cli
      
      - name: Build and package
        run: |
          cd KnowledgeBase
          python deploy_to_smithery.py --create-server --create-dockerfile --create-smithery-yaml
      
      - name: Login to Smithery
        if: startsWith(github.ref, 'refs/tags/v')
        run: echo "${{{{ secrets.SMITHERY_TOKEN }}}}" | smithery login --token-stdin
      
      - name: Deploy to Smithery
        if: startsWith(github.ref, 'refs/tags/v')
        run: smithery deploy .
      
      - name: Create npm package.json
        run: |
          cat > package.json << EOL
          {{
            "name": "{package_name}",
            "version": "${{{{ github.ref_name }}}}" ,
            "description": "{config.get('description', 'Unity UGUI系统的结构化知识库')}",
            "bin": {{
              "{package_name}": "./KnowledgeBase/ugui_kb_server.py"
            }},
            "scripts": {{
              "start": "python ./KnowledgeBase/ugui_kb_server.py"
            }},
            "dependencies": {{
              "fastmcp": "^1.0.0"
            }},
            "keywords": [
              "unity",
              "ugui",
              "knowledge-base",
              "mcp",
              "fastmcp"
            ],
            "author": "${{{{ github.repository_owner }}}}",
            "license": "MIT"
          }}
          EOL
      
      - name: Publish to npm
        if: startsWith(github.ref, 'refs/tags/v')
        run: npm publish
        env:
          NODE_AUTH_TOKEN: ${{{{ secrets.NPM_TOKEN }}}}
'''
    
    with open(workflow_path, 'w', encoding='utf-8') as f:
        f.write(workflow_content)
    
    console.print(f"[green]已创建GitHub Actions工作流配置文件: {workflow_path}[/green]")
    console.print("[yellow]注意: 您需要在GitHub仓库设置中添加以下Secrets:[/yellow]")
    console.print("- SMITHERY_TOKEN: Smithery平台的API令牌")
    console.print("- NPM_TOKEN: npm的访问令牌（如果需要发布到npm）")

def create_installation_guide():
    """创建安装指南文档"""
    guide_path = os.path.join(ROOT_DIR, "INSTALLATION.md")
    
    # 如果文件已存在，询问是否覆盖
    if os.path.exists(guide_path):
        console.print("[yellow]警告: 安装指南文档已存在[/yellow]")
        response = input("是否覆盖? (y/n): ")
        if response.lower() != 'y':
            console.print("[yellow]跳过创建安装指南文档[/yellow]")
            return
    
    # 从配置文件中获取项目信息
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    # 获取包名
    package_name = config.get("name", "ugui-knowledge-base")
    if isinstance(package_name, str) and package_name.strip():
        package_name = package_name.strip().lower().replace(" ", "-")
    else:
        package_name = "ugui-knowledge-base"
    
    # 创建安装指南文档
    guide_content = f'''# {config.get("name", "UGUI知识库")} 安装指南

## 简介

{config.get("description", "Unity UGUI系统的结构化知识库")}是一个基于FastMCP的MCP服务器，
可以用于查询文档、搜索内容、获取可视化图表等功能。

## 安装方法

### 方法一：通过npm全局安装

```bash
npm install -g {package_name}
```

安装完成后，可以通过以下命令启动服务器：

```bash
{package_name}
```

如果需要使用API密钥进行身份验证，可以通过环境变量设置：

```bash
UGUI_KB_API_KEY=your_api_key {package_name}
```

### 方法二：通过npx直接运行

```bash
npx {package_name}
```

如果需要使用API密钥进行身份验证：

```bash
npx {package_name} --key your_api_key
```

### 方法三：通过Smithery运行

```bash
npx -y @smithery/cli@latest run {package_name}
```

如果需要使用API密钥进行身份验证：

```bash
npx -y @smithery/cli@latest run {package_name} --key your_api_key
```

### 方法四：克隆GitHub仓库并本地运行

```bash
git clone <repository_url>
cd <repository_name>
pip install -r KnowledgeBase/requirements.txt
python KnowledgeBase/ugui_kb_server.py
```

## 在Claude Desktop中使用

1. 打开Claude Desktop设置
2. 添加您的MCP服务器:

```json
{{
  "mcpServers": {{
    "ugui-kb": {{
      "command": "npx",
      "args": [
        "-y", 
        "@smithery/cli@latest", 
        "run", 
        "{package_name}"
      ],
      "env": {{}}
    }}
  }}
}}
```

如果需要使用API密钥进行身份验证，请在args数组中添加`"--key", "您的API密钥"`。

3. 重启Claude Desktop
4. 使用以下命令与知识库交互:
   - 获取所有文档分类: `categories()`
   - 获取指定分类下的文档: `documents("architecture")`
   - 获取指定文档: `document("architecture", "basic")`
   - 搜索文档: `search("Canvas")`
   - 获取所有可视化图表: `visualizations()`
   - 获取可视化图表: `visualize("architecture")`
'''
    
    with open(guide_path, 'w', encoding='utf-8') as f:
        f.write(guide_content)
    
    console.print(f"[green]已创建安装指南文档: {guide_path}[/green]")

def update_smithery_yaml():
    """更新smithery.yaml配置文件以支持npm安装"""
    yaml_path = os.path.join(ROOT_DIR, "smithery.yaml")
    
    # 如果文件不存在，先创建
    if not os.path.exists(yaml_path):
        create_smithery_yaml()
        return
    
    # 读取现有配置
    with open(yaml_path, 'r', encoding='utf-8') as f:
        yaml_content = f.read()
    
    # 检查是否已包含npm配置
    if "npm:" in yaml_content:
        console.print("[yellow]smithery.yaml已包含npm配置，跳过更新[/yellow]")
        return
    
    # 从配置文件中获取项目信息
    config_path = os.path.join(SCRIPT_DIR, "mcp_config.json")
    with open(config_path, 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    # 获取包名
    package_name = config.get("name", "ugui-knowledge-base")
    if isinstance(package_name, str) and package_name.strip():
        package_name = package_name.strip().lower().replace(" ", "-")
    else:
        package_name = "ugui-knowledge-base"
    
    # 添加npm配置
    npm_config = f'''

# npm配置
npm:
  name: {package_name}
  bin: KnowledgeBase/ugui_kb_server.py
  dependencies:
    fastmcp: "^1.0.0"
'''
    
    # 更新配置文件
    with open(yaml_path, 'a', encoding='utf-8') as f:
        f.write(npm_config)
    
    console.print(f"[green]已更新smithery.yaml配置文件: {yaml_path}[/green]")
    console.print("[yellow]已添加npm配置，支持通过npm安装[/yellow]")

def update_readme():
    """更新README.md文件，添加GitHub部署相关信息"""
    readme_path = os.path.join(ROOT_DIR, "README.md")
    
    # 如果文件不存在，跳过更新
    if not os.path.exists(readme_path):
        console.print("[yellow]警告: README.md不存在，跳过更新[/yellow]")
        return
    
    # 读取现有README内容
    with open(readme_path, 'r', encoding='utf-8') as f:
        readme_content = f.read()
    
    # 检查是否已包含GitHub部署信息
    if "GitHub部署" in readme_content:
        console.print("[yellow]README.md已包含GitHub部署信息，跳过更新[/yellow]")
        return
    
    # 添加GitHub部署信息
    github_info = '''

## GitHub部署与安装

本项目支持通过GitHub Actions自动部署到Smithery.ai平台和npm。

### 安装方法

1. **通过npm全局安装**
   ```bash
   npm install -g ugui-knowledge-base
   ```

2. **通过npx直接运行**
   ```bash
   npx ugui-knowledge-base
   ```

3. **通过Smithery运行**
   ```bash
   npx -y @smithery/cli@latest run ugui-knowledge-base
   ```

4. **克隆GitHub仓库并本地运行**
   ```bash
   git clone <repository_url>
   cd <repository_name>
   pip install -r KnowledgeBase/requirements.txt
   python KnowledgeBase/ugui_kb_server.py
   ```

详细说明请参阅 [GitHub部署指南](KnowledgeBase/README_GITHUB.md) 和 [安装指南](INSTALLATION.md)。
'''
    
    # 更新README文件
    with open(readme_path, 'a', encoding='utf-8') as f:
        f.write(github_info)
    
    console.print(f"[green]已更新README.md文件: {readme_path}[/green]")
    console.print("[yellow]已添加GitHub部署与安装信息[/yellow]")

def main():
    """主函数"""
    parser = argparse.ArgumentParser(description="UGUI知识库Smithery.ai部署工具")
    parser.add_argument("--create-server", action="store_true", help="创建FastMCP服务器脚本")
    parser.add_argument("--create-dockerfile", action="store_true", help="创建Dockerfile")
    parser.add_argument("--create-yaml", action="store_true", help="创建smithery.yaml配置文件")
    parser.add_argument("--update-yaml", action="store_true", help="更新smithery.yaml配置文件以支持npm安装")
    parser.add_argument("--create-github-workflow", action="store_true", help="创建GitHub Actions工作流配置文件")
    parser.add_argument("--create-installation-guide", action="store_true", help="创建安装指南文档")
    parser.add_argument("--setup-for-github", action="store_true", help="设置GitHub部署所需的所有文件")
    parser.add_argument("--setup-venv", action="store_true", help="设置uv虚拟环境")
    parser.add_argument("--test", action="store_true", help="测试本地MCP服务器")
    parser.add_argument("--deploy", action="store_true", help="部署到Smithery.ai平台")
    parser.add_argument("--all", action="store_true", help="执行所有步骤")
    parser.add_argument("--use-npx", action="store_true", help="使用npx运行Smithery CLI")
    
    args = parser.parse_args()
    
    # 如果没有指定任何参数，显示帮助信息
    if not any(vars(args).values()):
        parser.print_help()
        return
    
    # 如果指定了设置虚拟环境参数
    if args.setup_venv:
        if setup_uv_venv():
            console.print("[green]虚拟环境设置完成，可以继续执行其他操作[/green]")
        else:
            return
    
    # 检查依赖
    deps_result = check_dependencies()
    if isinstance(deps_result, tuple):
        deps_ok, has_uv, has_npx = deps_result
    else:
        deps_ok = deps_result
        has_uv, has_npx = False, False
    
    if not deps_ok:
        # 如果缺少依赖且检测到使用uv但没有虚拟环境，提示用户使用--setup-venv
        try:
            result = subprocess.run(["uv", "--version"], capture_output=True, text=True)
            if result.returncode == 0 and not os.environ.get('VIRTUAL_ENV'):
                console.print("[yellow]提示: 检测到uv但没有虚拟环境[/yellow]")
                console.print("可以使用 --setup-venv 参数设置虚拟环境: python deploy_to_smithery.py --setup-venv")
        except Exception:
            pass
        return
    
    # 如果指定了--use-npx参数，强制使用npx
    if args.use_npx:
        has_npx = True
    
    # 执行指定的操作
    if args.all or args.create_server:
        create_ugui_kb_server()
    
    if args.all or args.create_dockerfile:
        create_dockerfile()
    
    if args.all or args.create_yaml:
        create_smithery_yaml()
    
    if args.all or args.update_yaml:
        update_smithery_yaml()
    
    if args.all or args.create_github_workflow:
        create_github_workflow()
    
    if args.all or args.create_installation_guide:
        create_installation_guide()
    
    # 设置GitHub部署所需的所有文件
    if args.setup_for_github:
        console.print(Panel("[bold]设置GitHub部署环境[/bold]"))
        create_ugui_kb_server()
        create_dockerfile()
        create_smithery_yaml()
        update_smithery_yaml()
        create_github_workflow()
        create_installation_guide()
        console.print(Panel("[bold green]GitHub部署环境设置完成![/bold green]"))
        console.print("\n[bold]下一步操作:[/bold]")
        console.print("1. 将项目推送到GitHub仓库")
        console.print("2. 在GitHub仓库设置中添加以下Secrets:")
        console.print("   - SMITHERY_TOKEN: Smithery平台的API令牌")
        console.print("   - NPM_TOKEN: npm的访问令牌（如果需要发布到npm）")
        console.print("3. 创建版本标签（例如v1.0.0）并推送到GitHub，触发自动部署")
    
    if args.all or args.test:
        test_local_server(has_npx)
    
    if args.all or args.deploy:
        deploy_to_smithery(has_npx)

if __name__ == "__main__":
    try:
        import time  # 导入time模块用于测试服务器时的延迟
        main()
    except KeyboardInterrupt:
        console.print("\n[yellow]操作已取消[/yellow]")
    except Exception as e:
        console.print(f"\n[red]发生错误: {str(e)}[/red]")