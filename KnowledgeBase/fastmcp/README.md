# FastMCP部署说明

## 概述

本文档提供了使用FastMCP框架部署UGUI知识库系统到Smithery.ai平台的详细说明。

## 前提条件

- 已安装Python 3.8+
- 已安装FastMCP库
- 已注册Smithery.ai账号并获取API密钥

## 本地测试

在部署到Smithery.ai平台前，建议先在本地测试MCP服务器：

1. 安装依赖：

```bash
pip install -r KnowledgeBase/core/requirements.txt
pip install fastmcp
```

2. 运行测试脚本：

```bash
python KnowledgeBase/fastmcp/deploy.py --test
```

3. 测试MCP服务器功能：

```bash
python KnowledgeBase/fastmcp/test_fastmcp_server.py
```

## 部署到Smithery.ai

### 部署步骤

1. 配置环境变量：

```bash
export SMITHERY_API_KEY=your_api_key_here
```

2. 运行部署脚本：

```bash
python KnowledgeBase/fastmcp/deploy.py --deploy
```

3. 验证部署结果：

```bash
python KnowledgeBase/fastmcp/deploy.py --verify
```

## MCP配置说明

`mcp_config.json`文件包含了MCP服务器的配置信息，主要包括：

- 服务器名称和描述
- 工具列表和参数定义
- 权限设置

## 注意事项

- 部署前确保`mcp_config.json`配置正确
- 部署过程中可能需要输入Smithery.ai账号密码
- 部署完成后，可以在Smithery.ai平台上查看和管理MCP服务器