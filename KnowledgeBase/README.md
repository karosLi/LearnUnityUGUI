# UGUI 知识库

这是一个关于Unity UGUI系统的结构化知识库，通过MCP工具提供访问。

## 知识库结构

本知识库基于LearnUnityUGUI项目，将其文档内容进行了系统化整理，主要包含以下几个部分：

### 1. UGUI架构与原理

- [UGUI基础架构](../Docs/UGUI/UGUIArchitecture.md) - UGUI系统的基础架构和组件关系
- [UGUI详细架构](../Docs/UGUI/UGUIDetailedArchitecture.md) - 深入了解UGUI的重建和合批机制
- [UGUI完整架构](../Docs/UGUI/UGUICompleteArchitecture.md) - UGUI系统的完整架构设计
- [UGUI系统关系](../Docs/UGUI/UGUISystemRelations.md) - UGUI各子系统之间的关系
- [UGUI事件系统](../Docs/UGUI/UGUI事件系统.md) - UGUI的事件处理机制

### 2. UGUI交互式动画

- [交互式动画索引](../Docs/UGUI/UGUI交互式动画索引.md) - 交互式动画文档的总索引
- [架构交互式动画](../Docs/UGUI/UGUI架构交互式动画.md) - UGUI架构的交互式可视化
- [时序交互式动画](../Docs/UGUI/UGUI时序交互式动画.md) - UGUI系统的时序交互动画

### 3. UGUI使用指南

- [组件使用指南](../Docs/Guide/UGUI组件使用指南.md) - UGUI各组件的使用方法
- [交互式动画指南](../Docs/Guide/UGUI交互式动画指南.md) - 交互式动画的基础指南
- [交互式动画高级指南](../Docs/Guide/UGUI交互式动画指南_高级部分.md) - 交互式动画的高级技巧
- [交互式动画优化](../Docs/Guide/UGUI交互式动画指南_优化.md) - 交互式动画的性能优化

### 4. UGUI最佳实践

- [优化指南](../Docs/UGUI/UGUI优化指南.md) - UGUI系统的性能优化建议
- [Cursor规则](../Docs/Guide/CursorRules.md) - 指针交互规则

## 通过MCP工具访问

### MCP工具接口

本知识库提供以下接口供MCP工具访问：

1. **文档查询接口**：通过文档ID或关键词查询相关文档
2. **架构图可视化接口**：获取UGUI架构的可视化图表
3. **交互式动画接口**：访问交互式动画资源
4. **代码示例接口**：获取UGUI相关的代码示例

### 使用方法

1. **通过API访问**：
   ```
   GET /api/docs/{docId}
   GET /api/search?keyword={keyword}
   GET /api/visualize/{diagramId}
   GET /api/examples/{exampleId}
   ```

2. **通过WebViewer访问**：
   - 启动本地服务器：`python WebViewer/server.py`
   - 浏览器访问：`http://localhost:8080/WebViewer/UGUIDocumentationViewer.html`

## 贡献指南

欢迎贡献代码或改进文档：

1. Fork 项目
2. 创建特性分支
3. 提交更改
4. 推送到分支
5. 创建 Pull Request

## 许可证

本知识库基于MIT许可证开源。