# UGUI知识库系统部署指南

## 概述

本文档提供了UGUI知识库系统的部署指南，支持两种部署方式：

1. **Docker部署**：通过Docker容器化部署，支持GitHub Actions自动构建和发布
2. **FastMCP部署**：通过FastMCP框架部署到Smithery.ai平台

## 目录结构

为了更好地支持两种部署方式，我们对目录结构进行了优化：

```
KnowledgeBase/
├── core/                  # 核心功能代码（两种部署方式共用）
│   ├── api.py             # API服务器
│   ├── mcp_server.py      # MCP服务器
│   └── requirements.txt   # 共用依赖
├── docker/                # Docker部署相关文件
│   ├── Dockerfile         # Docker构建文件
│   ├── start.py           # Docker启动脚本
│   └── README.md          # Docker部署说明
├── fastmcp/               # FastMCP部署相关文件
│   ├── mcp_config.json    # FastMCP配置
│   ├── deploy.py          # FastMCP部署脚本
│   └── README.md          # FastMCP部署说明
├── docs/                  # 文档目录
│   ├── 使用说明.md         # 通用使用说明
│   └── 知识库索引.md        # 知识库索引
└── README.md              # 主README文件
```

## Docker部署

### 本地部署

1. 构建Docker镜像：

```bash
docker build -t ugui-kb -f KnowledgeBase/docker/Dockerfile --build-arg DOCS_PATH=./Docs .
```

2. 运行Docker容器：

```bash
docker run -p 5000:5000 -p 8000:8000 ugui-kb
```

### GitHub Actions自动部署

项目已配置GitHub Actions工作流，当推送到main分支时，会自动构建并发布Docker镜像到GitHub Container Registry。

详细说明请参考 `KnowledgeBase/docker/README.md`。

## FastMCP部署

### 本地测试

1. 安装依赖：

```bash
pip install -r KnowledgeBase/core/requirements.txt
pip install fastmcp
```

2. 运行部署脚本：

```bash
python KnowledgeBase/fastmcp/deploy.py --test
```

### 部署到Smithery.ai

1. 运行部署脚本：

```bash
python KnowledgeBase/fastmcp/deploy.py --deploy
```

详细说明请参考 `KnowledgeBase/fastmcp/README.md`。

## 使用方法

无论使用哪种部署方式，UGUI知识库系统都提供以下功能：

1. **文档查询**：查询UGUI架构、组件、最佳实践等文档
2. **可视化**：查看UGUI架构图和交互式动画
3. **搜索**：搜索知识库内容

详细使用说明请参考 `KnowledgeBase/docs/使用说明.md`。

## 实施计划

1. 创建新的目录结构
2. 迁移和整合文件
3. 更新GitHub Actions工作流
4. 测试两种部署方式
5. 移除未使用的文件

详细实施方案请参考 `KnowledgeBase/目录结构优化方案.md`。