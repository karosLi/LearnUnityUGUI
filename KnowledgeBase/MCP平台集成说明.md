# UGUI知识库 - MCP平台集成说明

## 概述

本文档介绍如何将UGUI知识库集成到MCP平台，使其成为MCP平台的一个工具组件。通过本地模式，知识库可以在MCP平台上独立运行，无需依赖网络API服务。

## 集成步骤

### 1. 打包知识库

首先，需要将知识库打包成MCP平台可用的格式：

```bash
# 打包知识库为MCP分发包
python KnowledgeBase/mcp_client.py pack --output mcp_ugui_kb
```

这将在当前目录下创建一个名为`mcp_ugui_kb`的目录，包含所有必要的文件。

### 2. 上传到MCP平台

将打包后的目录上传到MCP平台的工具目录：

```bash
# 假设MCP平台工具目录为/path/to/mcp/tools
cp -r mcp_ugui_kb /path/to/mcp/tools/
```

### 3. 注册工具

在MCP平台上注册UGUI知识库工具：

```bash
# 注册工具（具体命令可能因MCP平台实现而异）
mcp register-tool --name ugui-kb --path /path/to/mcp/tools/mcp_ugui_kb
```

### 4. 配置工具别名

为了方便使用，可以配置一个简短的别名：

```bash
# 配置别名（具体命令可能因MCP平台实现而异）
mcp alias --name ugui-kb --command "mcp_client.py --mode local"
```

## 使用方法

注册完成后，可以通过MCP平台命令行使用UGUI知识库：

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

## 注意事项

### 依赖项

UGUI知识库依赖以下Python包：

- Flask
- Flask-CORS
- Markdown
- Requests
- Rich

确保MCP平台环境中已安装这些依赖项：

```bash
pip install -r /path/to/mcp/tools/mcp_ugui_kb/KnowledgeBase/requirements.txt
```

### 文件结构

打包后的知识库包含以下目录和文件：

```
mcp_ugui_kb/
├── Docs/                  # 文档目录
├── KnowledgeBase/         # 知识库核心文件
│   ├── api.py             # API接口（支持本地模式）
│   ├── mcp_client.py      # MCP客户端工具
│   ├── mcp_config.json    # 配置文件（已设置为本地模式）
│   ├── requirements.txt   # 依赖项
│   └── 其他文件...
├── WebViewer/             # Web可视化工具
├── README.md              # 说明文档
└── mcp_platform.json      # MCP平台配置文件
```

### 更新知识库

如需更新知识库内容，请按照以下步骤操作：

1. 更新原始知识库内容
2. 重新打包知识库
3. 替换MCP平台上的知识库目录
4. 重新注册工具（如有必要）

## 故障排除

### 常见问题

- **找不到命令**：确保工具已正确注册，并且别名配置正确
- **找不到文档**：确保`Docs/`目录结构正确，与`api.py`中的`doc_index`定义一致
- **可视化图表无法打开**：确保`WebViewer/`目录存在，且包含正确的HTML文件

### 调试

如果遇到问题，可以尝试直接在MCP平台工具目录中运行客户端：

```bash
cd /path/to/mcp/tools/mcp_ugui_kb
python KnowledgeBase/mcp_client.py --mode local categories
```

## 更多信息

更多详细信息，请参考知识库中的以下文档：

- `本地模式与MCP平台集成指南.md`：详细介绍本地模式和MCP平台集成
- `使用说明.md`：知识库的基本使用说明
- `知识库索引.md`：知识库内容索引