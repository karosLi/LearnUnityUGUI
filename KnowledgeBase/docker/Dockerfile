# 基于官方 Python 镜像
FROM python:3.10-slim

# 设置工作目录
WORKDIR /app

# 设置构建参数
ARG DOCS_PATH=./Docs

# 复制依赖文件
COPY KnowledgeBase/requirements.txt ./

# 安装依赖
RUN pip install --no-cache-dir -r requirements.txt

# 复制知识库代码
COPY KnowledgeBase/ ./

# 复制文档资料
COPY ${DOCS_PATH}/ ./docs/

# 设置环境变量
ENV DOCS_DIR="/app/docs"
ENV MCP_SERVER_ENABLED=true

# 暴露端口（API服务器端口）
EXPOSE 5000
# 暴露MCP服务器端口
EXPOSE 8000

# 启动命令
CMD ["python", "docker/start.py", "--mcp-server"]