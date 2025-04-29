# UGUI知识库部署指南

## GitHub部署方式
1. 确保已配置GitHub Actions Secrets
2. 将代码推送至main分支后自动触发构建
3. 生成的Docker镜像会被推送到GitHub Packages

## MCP客户端安装
```bash
npm install -g https://github.com/YOUR_ORG/ugui-knowledge-base
```

## Claude Desktop配置
在`claude_desktop_config.json`中添加：
```json
{
  "mcpServers": {
    "ugui-kb": {
      "command": "npx",
      "args": [
        "-y",
        "@smithery/cli@latest",
        "run",
        "ugui-kb-github"
      ]
    }
  }
}
```

详细使用文档请参考[官方指引](https://modelcontextprotocol.io/quickstart/server)
