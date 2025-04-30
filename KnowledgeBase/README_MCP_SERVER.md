# UGUI知识库 MCP 服务器部署指南

本文档提供了关于如何部署和使用UGUI知识库MCP服务器的详细说明。

## 概述

UGUI知识库MCP服务器是一个基于FastAPI的服务，用于提供对UGUI文档资源的访问。它可以部署在GitHub Actions上，并通过Docker容器运行。

## 功能特点

- 提供文档分类查询
- 获取指定分类下的所有文档
- 获取文档内容
- 搜索文档内容
- 与MCP平台集成

## 部署方式

### 本地部署

1. 安装依赖：

```bash
pip install -r KnowledgeBase/requirements.txt
```

2. 启动服务器：

```bash
python KnowledgeBase/start_knowledge_base.py --mcp-server
```

### Docker部署

1. 构建Docker镜像：

```bash
docker build -t ugui-knowledge-base -f KnowledgeBase/Dockerfile .
```

2. 运行Docker容器：

```bash
docker run -p 5000:5000 -p 8000:8000 ugui-knowledge-base
```

### GitHub Actions部署

项目已配置GitHub Actions工作流，当推送到main分支时，会自动构建并发布Docker镜像到GitHub Container Registry。

工作流文件位于：`.github/workflows/deploy_mcp_server.yml`

## 使用方法

### MCP服务器API

MCP服务器运行在 `http://localhost:8000`，提供以下API：

- `/mcp/config` - 获取MCP服务器配置
- `/mcp/tools/get_categories` - 获取所有文档分类
- `/mcp/tools/get_documents` - 获取指定分类下的所有文档
- `/mcp/tools/get_document_content` - 获取文档内容
- `/mcp/tools/search_documents` - 搜索文档

### 使用MCP客户端

可以使用提供的MCP客户端工具访问服务器：

```bash
python KnowledgeBase/mcp_client.py categories
```

## 环境变量

- `DOCS_DIR` - 文档目录路径，默认为 `../Docs`
- `MCP_SERVER_ENABLED` - 是否启用MCP服务器，设置为 `true` 启用

## 目录结构

```
KnowledgeBase/
  ├── Dockerfile            # Docker构建文件
  ├── requirements.txt      # 依赖列表
  ├── start_knowledge_base.py # 启动脚本
  ├── mcp_server.py         # MCP服务器实现
  └── mcp_client.py         # MCP客户端工具
```

## 故障排除

1. 如果遇到端口冲突，可以修改 `mcp_server.py` 中的端口配置。
2. 确保 `DOCS_DIR` 环境变量指向正确的文档目录。
3. 检查依赖是否正确安装。

## 注意事项

- MCP服务器需要访问 `Docs` 目录中的文档资源。
- 在Docker环境中，文档会被复制到容器内的 `/app/docs` 目录。
- GitHub Actions部署时，会使用 `DOCS_PATH` 构建参数指定文档目录。