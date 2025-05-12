#!/bin/bash
# FastMCP一键安装脚本
set -e

# 解析参数
while [[ $# -gt 0 ]]; do
    case "$1" in
        --port)
        MCP_PORT="$2"
        shift 2
        ;;
        --docs-path)
        DOCS_PATH="$2"
        shift 2
        ;;
        *)
        echo "未知参数: $1"
        exit 1
        ;;
    esac
done

# 安装依赖
pip3 install -r requirements.txt

# 使用标准配置文件
cp config.env.template config.env
# 更新运行时参数
sed -i '' "s/MCP_PORT=.*/MCP_PORT=${MCP_PORT:-8000}/" config.env
sed -i '' "s|DOCS_PATH=.*|DOCS_PATH=${DOCS_PATH:-./docs}|" config.env

# 启动服务
python3 start_knowledge_base.py --mcp-server

echo "✅ MCP服务已启动，端口：${MCP_PORT} 文档路径：${DOCS_PATH}"