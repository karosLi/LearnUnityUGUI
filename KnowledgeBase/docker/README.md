# Docker部署说明

## 概述

本文档提供了使用Docker部署UGUI知识库系统的详细说明。Docker部署方式支持本地部署和GitHub Actions自动构建发布。

## 本地部署

### 前提条件

- 已安装Docker
- 已克隆LearnUnityUGUI项目代码

### 部署步骤

1. 构建Docker镜像：

```bash
docker build -t ugui-kb -f KnowledgeBase/docker/Dockerfile --build-arg DOCS_PATH=./Docs .
```

2. 运行Docker容器：

```bash
docker run -p 5000:5000 -p 8000:8000 ugui-kb
```

3. 访问知识库系统：
   - API服务器：http://localhost:5000
   - WebViewer：http://localhost:8000

## GitHub Actions自动部署

项目已配置GitHub Actions工作流，当推送到main分支时，会自动构建并发布Docker镜像到GitHub Container Registry。

### 配置说明

1. 在项目根目录的`.github/workflows`目录下创建工作流配置文件
2. 配置工作流触发条件、构建步骤和发布设置
3. 推送代码到GitHub仓库，触发自动构建和发布

### 使用自动部署的镜像

```bash
docker pull ghcr.io/yourusername/ugui-kb:latest
docker run -p 5000:5000 -p 8000:8000 ghcr.io/yourusername/ugui-kb:latest
```

## 注意事项

- Dockerfile中的`DOCS_PATH`参数用于指定文档目录的路径，默认为`./Docs`
- 容器内部使用端口5000和8000，可以通过`-p`参数映射到其他端口
- 如需持久化数据，可以使用Docker卷挂载