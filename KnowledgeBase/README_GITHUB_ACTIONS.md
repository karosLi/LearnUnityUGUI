# UGUI知识库 MCP 服务器 GitHub Actions 部署指南

## 概述

本文档提供了关于如何使用GitHub Actions自动部署UGUI知识库MCP服务器的详细说明。通过本指南配置的自动化流程，可以在代码推送到main分支时自动构建并发布Docker镜像到GitHub Container Registry。

## 已完成的配置

我们已经完成了以下配置工作：

1. 创建了专用于MCP服务器部署的GitHub Actions工作流文件
2. 修改了Dockerfile以适配MCP服务器部署需求
3. 更新了start_knowledge_base.py脚本以支持MCP服务器模式
4. 创建了mcp_server.py实现MCP服务器功能
5. 更新了mcp_client.py以支持与MCP服务器交互
6. 更新了requirements.txt添加MCP服务器所需依赖

## 文件说明

### 1. GitHub Actions工作流文件

文件路径：`.github/workflows/deploy_mcp_server.yml`

这个工作流文件配置了自动构建和发布流程，主要特点：

- 监听main分支的推送事件，特别是KnowledgeBase和Docs目录的变更
- 使用Docker构建并推送镜像到GitHub Container Registry
- 设置构建上下文为项目根目录，使用KnowledgeBase/Dockerfile作为构建文件
- 通过build-args传递Docs目录路径

### 2. Dockerfile

文件路径：`KnowledgeBase/Dockerfile`

修改后的Dockerfile具有以下特点：

- 使用Python 3.10作为基础镜像
- 添加DOCS_PATH构建参数，用于指定文档目录
- 分别复制依赖文件、知识库代码和文档资料
- 设置环境变量DOCS_DIR和MCP_SERVER_ENABLED
- 暴露API服务器端口(5000)和MCP服务器端口(8000)
- 启动命令添加--mcp-server参数

### 3. MCP服务器实现

文件路径：`KnowledgeBase/mcp_server.py`

这个文件实现了基于FastAPI的MCP服务器，提供以下功能：

- 获取所有文档分类
- 获取指定分类下的所有文档
- 获取文档内容
- 搜索文档
- 提供MCP服务器配置接口

### 4. 启动脚本更新

文件路径：`KnowledgeBase/start_knowledge_base.py`

对启动脚本的主要修改：

- 添加对MCP服务器模式的支持
- 添加命令行参数解析
- 根据运行模式启动不同的服务
- 支持环境变量配置

### 5. 客户端工具更新

文件路径：`KnowledgeBase/mcp_client.py`

对客户端工具的主要修改：

- 添加对MCP服务器模式的支持
- 实现与MCP服务器的交互方法
- 更新命令行参数

## 使用方法

### 本地测试

1. 安装依赖：

```bash
pip install -r KnowledgeBase/requirements.txt
```

2. 启动MCP服务器：

```bash
python KnowledgeBase/start_knowledge_base.py --mcp-server
```

3. 使用MCP客户端：

```bash
python KnowledgeBase/mcp_client.py --mode mcp categories
```

### GitHub Actions部署

1. 确保你的GitHub仓库已启用GitHub Actions
2. 推送代码到main分支，特别是KnowledgeBase或Docs目录的变更
3. GitHub Actions将自动构建并发布Docker镜像
4. 镜像将被发布到：`ghcr.io/[用户名]/[仓库名]/mcp-knowledge-base:latest`

### 使用Docker镜像

```bash
docker pull ghcr.io/[用户名]/[仓库名]/mcp-knowledge-base:latest
docker run -p 5000:5000 -p 8000:8000 ghcr.io/[用户名]/[仓库名]/mcp-knowledge-base:latest
```

## 故障排除

1. 如果GitHub Actions构建失败，请检查工作流日志以获取详细错误信息
2. 确保GitHub仓库设置了适当的权限，允许GitHub Actions发布到Container Registry
3. 如果本地测试时MCP服务器无法启动，请检查依赖是否正确安装
4. 如果文档无法访问，请检查DOCS_DIR环境变量是否正确设置

## 后续工作

- 添加自动化测试
- 实现版本控制
- 添加更多文档搜索和分析功能
- 优化Docker镜像大小