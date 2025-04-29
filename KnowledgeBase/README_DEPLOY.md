# UGUI知识库 Smithery.ai 部署指南

本文档提供了如何使用 `deploy_to_smithery.py` 脚本将UGUI知识库部署到Smithery.ai平台的详细说明。

## 前提条件

在开始部署之前，请确保您已安装以下依赖：

1. **Python 3.6+**：用于运行部署脚本
2. **FastMCP**：用于构建MCP服务器
   ```bash
   uv pip install fastmcp
   # 或使用pip
   pip install fastmcp
   ```
3. **Node.js**：用于运行Smithery CLI
   - 从 [Node.js官网](https://nodejs.org/) 下载并安装
4. **Smithery CLI**：用于部署到Smithery.ai平台
   ```bash
   npm install -g @smithery/cli
   ```

## 部署步骤

### 1. 创建必要文件

使用部署脚本创建所有必要的文件：

```bash
cd /path/to/LearnUnityUGUI/KnowledgeBase
python deploy_to_smithery.py --all
```

这将执行以下操作：
- 创建FastMCP服务器脚本 (`ugui_kb_server.py`)
- 创建Dockerfile
- 创建smithery.yaml配置文件
- 测试本地MCP服务器
- 部署到Smithery.ai平台

### 2. 单独执行各步骤

如果您想单独执行各个步骤，可以使用以下命令：

```bash
# 创建FastMCP服务器脚本
python deploy_to_smithery.py --create-server

# 创建Dockerfile
python deploy_to_smithery.py --create-dockerfile

# 创建smithery.yaml配置文件
python deploy_to_smithery.py --create-yaml

# 测试本地MCP服务器
python deploy_to_smithery.py --test

# 部署到Smithery.ai平台
python deploy_to_smithery.py --deploy
```

## 在Claude Desktop中使用

成功部署后，您可以在Claude Desktop中使用UGUI知识库：

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

## 故障排除

### 常见问题

1. **依赖安装失败**
   - 确保您有正确的Python和Node.js版本
   - 尝试使用管理员权限安装依赖

2. **服务器测试失败**
   - 检查`ugui_kb_server.py`文件是否正确创建
   - 确保`mcp_config.json`文件存在且格式正确
   - 检查控制台输出的错误信息

3. **部署失败**
   - 确保您已登录Smithery.ai平台
   - 检查网络连接
   - 确保Dockerfile和smithery.yaml文件格式正确

### 调试方法

如果遇到问题，可以尝试以下调试方法：

1. 手动运行服务器脚本：
   ```bash
   python ugui_kb_server.py
   ```

2. 检查生成的文件内容是否正确

3. 使用`--test`参数测试服务器

## 注意事项

1. 确保您的知识库内容（文档、图表等）已经准备好并放在正确的目录中
2. 部署前请确保您已经登录Smithery.ai平台
3. 部署过程可能需要一些时间，请耐心等待
4. 如果您修改了知识库内容，需要重新部署才能生效

## 参考资源

- [FastMCP官方文档](https://github.com/jlowin/fastmcp)
- [Smithery.ai部署指南](https://smithery.ai/docs/config)
- [MCP规范文档](https://mcp.so)
- [Claude Desktop MCP集成指南](https://claude.ai/docs/mcp)