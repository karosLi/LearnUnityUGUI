# 基于FastMCP实现MCP工具并分发到Smithery.ai平台的实施方案

## 1. 概述

本文档提供了如何基于FastMCP实现MCP工具并将其分发到Smithery.ai平台的详细方案。通过这个方案，您可以将现有的UGUI知识库转换为符合Model Context Protocol (MCP)规范的工具，并在Smithery.ai平台上分享和部署。

## 2. FastMCP简介

FastMCP是一个用于构建MCP服务器和客户端的Python库，它提供了一种高级、Pythonic的接口，使开发者能够轻松地构建和交互MCP服务器。FastMCP处理了所有复杂的协议细节和服务器管理，让您可以专注于构建优秀的工具。

主要特点：
- 使用装饰器创建工具和资源
- 最小化样板代码
- 完整实现MCP规范
- 支持工具、资源和提示模板

## 3. 改造现有MCP知识库的步骤

### 3.1 安装必要的依赖

```bash
# 推荐使用uv安装FastMCP
uv pip install fastmcp

# 安装Smithery CLI工具
npm install -g @smithery/cli
```

### 3.2 创建FastMCP服务器

创建一个新的Python文件`ugui_kb_server.py`，实现基于FastMCP的MCP服务器：

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库MCP服务器

这个模块提供了基于FastMCP的UGUI知识库服务器，
可以用于查询文档、搜索内容、获取可视化图表等功能。
"""

import os
import json
from fastmcp import FastMCP

# 获取脚本目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

# 创建MCP服务器
mcp = FastMCP("UGUI知识库", dependencies=["flask", "flask_cors", "markdown", "requests", "rich"])

# 加载配置
with open(os.path.join(SCRIPT_DIR, 'mcp_config.json'), 'r', encoding='utf-8') as f:
    config = json.load(f)

# 定义工具：获取文档分类
@mcp.tool()
def categories() -> list:
    """获取所有文档分类"""
    return list(config['categories'].keys())

# 定义工具：获取指定分类下的文档
@mcp.tool()
def documents(category: str) -> list:
    """获取指定分类下的所有文档
    
    Args:
        category: 文档分类名称
    """
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
    return list(config['visualizations'].keys())

# 定义工具：获取可视化图表
@mcp.tool()
def visualize(diagram_id: str) -> str:
    """获取可视化图表
    
    Args:
        diagram_id: 图表ID
    """
    if diagram_id not in config['visualizations']:
        return f"图表不存在: {diagram_id}"
    
    # 返回图表HTML路径
    return os.path.join(SCRIPT_DIR, '..', 'WebViewer', f"{diagram_id}.html")

# 主函数
if __name__ == "__main__":
    mcp.run()
```

### 3.3 创建Dockerfile

在项目根目录创建`Dockerfile`文件：

```dockerfile
FROM python:3.9-slim

WORKDIR /app

# 复制必要的文件
COPY KnowledgeBase/ /app/KnowledgeBase/
COPY Docs/ /app/Docs/
COPY WebViewer/ /app/WebViewer/

# 安装依赖
RUN pip install --no-cache-dir fastmcp flask flask_cors markdown requests rich

# 设置工作目录
WORKDIR /app/KnowledgeBase

# 命令将由smithery.yaml提供
CMD ["python", "ugui_kb_server.py"]
```

### 3.4 创建smithery.yaml配置文件

在项目根目录创建`smithery.yaml`文件：

```yaml
name: ugui-knowledge-base
description: Unity UGUI系统的结构化知识库
version: 1.0.0
authors:
  - name: Your Name
    email: your.email@example.com

startCommand:
  type: stdio
  configSchema:
    type: object
    properties: {}
    required: []
  commandFunction: |
    function getCommand(config) {
      return {
        command: "python",
        args: ["KnowledgeBase/ugui_kb_server.py"],
        env: {}
      };
    }

build:
  dockerfile: Dockerfile
  dockerBuildPath: .
```

## 4. 部署到Smithery.ai平台

### 4.1 准备工作

1. 确保您的代码已经上传到GitHub公共仓库
2. 注册Smithery.ai账号并获取API密钥

### 4.2 使用Smithery CLI部署

```bash
# 安装Smithery CLI
npm install -g @smithery/cli

# 登录Smithery
smithery login

# 部署MCP服务器
cd /path/to/your/project
smithery deploy .
```

### 4.3 验证部署

部署完成后，您可以在Smithery.ai平台上查看您的MCP服务器。您可以通过以下方式测试：

1. 在Smithery.ai控制台中测试您的服务器
2. 使用Claude Desktop或其他支持MCP的应用程序连接到您的服务器

## 5. 使用方法

### 5.1 在Claude Desktop中使用

1. 打开Claude Desktop设置
2. 添加您的MCP服务器：

```json
{
  "mcpServers": {
    "ugui-kb": {
      "command": "npx",
      "args": ["@smithery/cli", "run", "ugui-knowledge-base"],
      "env": {}
    }
  }
}
```

3. 重启Claude Desktop
4. 使用以下命令与知识库交互：
   - 获取所有文档分类：`categories()`
   - 获取指定分类下的文档：`documents("architecture")`
   - 获取指定文档：`document("architecture", "basic")`
   - 搜索文档：`search("Canvas")`
   - 获取所有可视化图表：`visualizations()`
   - 获取可视化图表：`visualize("architecture")`

## 6. 与现有MCP知识库的区别

与原有的MCP知识库相比，基于FastMCP实现的版本有以下优势：

1. **更简洁的代码**：使用装饰器语法，减少样板代码
2. **更好的可维护性**：代码结构更清晰，易于扩展
3. **更广泛的兼容性**：符合最新的MCP规范，可以在更多平台上使用
4. **更容易部署**：通过Smithery.ai平台，可以轻松分发和管理

## 7. 注意事项

1. **依赖管理**：确保在Dockerfile中包含所有必要的依赖
2. **文件路径**：注意文件路径的处理，确保在Docker容器中能够正确访问
3. **配置文件**：确保mcp_config.json文件包含所有必要的配置
4. **权限设置**：确保服务器有足够的权限访问文档和资源

## 8. 故障排除

### 常见问题

1. **服务器无法启动**：检查依赖是否安装正确，配置文件是否存在
2. **找不到文档**：检查文件路径是否正确，文档是否存在
3. **部署失败**：检查Dockerfile和smithery.yaml是否正确配置

### 调试方法

使用FastMCP的调试工具进行本地测试：

```bash
# 本地运行服务器
python KnowledgeBase/ugui_kb_server.py

# 使用MCP Inspector测试
mcp inspect KnowledgeBase/ugui_kb_server.py
```

## 9. 结论

通过本方案，您可以将现有的UGUI知识库转换为符合MCP规范的工具，并在Smithery.ai平台上分享和部署。这不仅提高了知识库的可用性，还使其能够与更多AI工具和平台集成。

## 10. 参考资源

- [FastMCP官方文档](https://github.com/jlowin/fastmcp)
- [Smithery.ai部署指南](https://smithery.ai/docs/config)
- [MCP规范文档](https://mcp.so)
- [Claude Desktop MCP集成指南](https://claude.ai/docs/mcp)