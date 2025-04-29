# UGUI知识库 - 本地模式与MCP平台集成指南

## 1. 本地模式简介

UGUI知识库现在支持本地模式运行，无需启动API服务器，直接通过本地文件系统访问知识库内容。本地模式具有以下优势：

- **无需网络连接**：所有数据都存储在本地文件系统中，不依赖网络API
- **更快的访问速度**：直接读取本地文件，减少网络延迟
- **更容易分发**：可以将知识库打包成独立的目录，方便分发和部署
- **MCP平台集成**：可以作为MCP平台工具使用

## 2. 使用本地模式

### 2.1 命令行参数

在使用MCP客户端工具时，可以通过`--mode`参数指定运行模式：

```bash
# 以本地模式运行
python KnowledgeBase/mcp_client.py --mode local categories

# 以服务器模式运行（默认）
python KnowledgeBase/mcp_client.py --mode server categories
```

### 2.2 本地模式下的命令

本地模式支持与服务器模式相同的所有命令：

```bash
# 查看所有文档分类
python KnowledgeBase/mcp_client.py --mode local categories

# 查看指定分类下的文档
python KnowledgeBase/mcp_client.py --mode local documents architecture

# 查看指定文档
python KnowledgeBase/mcp_client.py --mode local document architecture basic

# 搜索文档
python KnowledgeBase/mcp_client.py --mode local search "Canvas"

# 查看所有可视化图表
python KnowledgeBase/mcp_client.py --mode local visualizations

# 打开可视化图表
python KnowledgeBase/mcp_client.py --mode local visualize architecture
```

## 3. 打包为MCP平台分发包

### 3.1 使用打包命令

UGUI知识库提供了打包命令，可以将知识库打包成可分发的MCP平台格式：

```bash
# 打包知识库
python KnowledgeBase/mcp_client.py pack

# 指定输出目录
python KnowledgeBase/mcp_client.py pack --output my_knowledge_pack
```

打包后的目录结构如下：

```
mcp_knowledge_pack/
├── Docs/                  # 文档目录
├── KnowledgeBase/         # 知识库核心文件
│   ├── api.py             # API接口（支持本地模式）
│   ├── mcp_client.py      # MCP客户端工具
│   ├── mcp_config.json    # 配置文件（已设置为本地模式）
│   ├── requirements.txt   # 依赖项
│   ├── 使用说明.md        # 使用说明
│   └── 知识库索引.md      # 知识库索引
├── WebViewer/             # Web可视化工具
├── README.md              # 说明文档
└── mcp_platform.json      # MCP平台配置文件
```

### 3.2 MCP平台配置

打包后的知识库包含`mcp_platform.json`文件，定义了在MCP平台上的配置：

```json
{
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
```

## 4. 在MCP平台上使用

### 4.1 安装到MCP平台

1. 将打包后的目录上传到MCP平台的工具目录
2. 在MCP平台上注册此工具
3. 配置工具别名（例如：`ugui-kb`）

### 4.2 MCP平台命令

在MCP平台上，可以使用以下命令访问知识库：

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

## 5. 本地模式技术说明

### 5.1 工作原理

本地模式通过以下方式实现：

1. `api.py`中添加了`LocalAPI`类，提供与API服务器相同的功能，但直接访问本地文件系统
2. `mcp_client.py`中添加了对本地模式的支持，可以通过命令行参数切换模式
3. 打包脚本`pack_for_mcp.py`用于创建MCP平台分发包

### 5.2 文件依赖

本地模式依赖以下文件：

- `api.py`：提供本地API接口
- `mcp_client.py`：MCP客户端工具
- `mcp_config.json`：配置文件
- `Docs/`目录：存储文档文件
- `WebViewer/`目录：存储可视化工具

## 6. 故障排除

### 6.1 常见问题

- **找不到文档**：确保`Docs/`目录结构正确，与`api.py`中的`doc_index`定义一致
- **可视化图表无法打开**：确保`WebViewer/`目录存在，且包含正确的HTML文件
- **依赖项缺失**：运行`pip install -r KnowledgeBase/requirements.txt`安装所有依赖

### 6.2 调试模式

如果遇到问题，可以启用调试输出：

```bash
# 启用调试输出
DEBUG=1 python KnowledgeBase/mcp_client.py --mode local categories
```

## 7. 更新与维护

### 7.1 更新知识库内容

更新知识库内容后，需要重新打包：

```bash
python KnowledgeBase/mcp_client.py pack
```

### 7.2 分发更新

将打包后的目录上传到MCP平台，替换原有版本。