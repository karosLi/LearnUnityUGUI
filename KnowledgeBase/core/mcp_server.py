#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库MCP服务器

这个脚本实现了一个基于FastAPI的MCP服务器，用于提供UGUI知识库的访问接口。
"""

import os
import sys
import json
import glob
import markdown
from typing import Dict, List, Optional, Any
from fastapi import FastAPI, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
import uvicorn

# 获取文档目录
DOCS_DIR = os.environ.get("DOCS_DIR", "../Docs")
if not os.path.exists(DOCS_DIR):
    print(f"错误: 文档目录不存在: {DOCS_DIR}")
    print("请设置正确的DOCS_DIR环境变量")
    sys.exit(1)

print(f"使用文档目录: {DOCS_DIR}")

# 创建FastAPI应用
app = FastAPI(title="UGUI知识库MCP服务器")

# 配置CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 定义模型
class ToolInput(BaseModel):
    args: Dict[str, Any] = Field(default={})

class ToolOutput(BaseModel):
    result: Any

# 工具函数
def get_categories() -> List[str]:
    """获取所有文档分类"""
    categories = []
    for dir_path in glob.glob(os.path.join(DOCS_DIR, "*")):
        if os.path.isdir(dir_path):
            categories.append(os.path.basename(dir_path))
    return categories

def get_documents(category: str) -> List[Dict[str, str]]:
    """获取指定分类下的所有文档"""
    documents = []
    category_path = os.path.join(DOCS_DIR, category)
    if not os.path.exists(category_path):
        return []
    
    for file_path in glob.glob(os.path.join(category_path, "*.md")):
        doc_name = os.path.basename(file_path).replace(".md", "")
        documents.append({
            "name": doc_name,
            "path": file_path.replace(DOCS_DIR, "").lstrip("/\\"),
        })
    return documents

def get_document_content(doc_path: str) -> str:
    """获取文档内容"""
    full_path = os.path.join(DOCS_DIR, doc_path)
    if not os.path.exists(full_path):
        return ""
    
    with open(full_path, "r", encoding="utf-8") as f:
        content = f.read()
    return content

# API路由
@app.post("/mcp/tools/get_categories")
async def api_get_categories(input_data: ToolInput) -> ToolOutput:
    """获取所有文档分类"""
    categories = get_categories()
    return ToolOutput(result=categories)

@app.post("/mcp/tools/get_documents")
async def api_get_documents(input_data: ToolInput) -> ToolOutput:
    """获取指定分类下的所有文档"""
    category = input_data.args.get("category", "")
    if not category:
        raise HTTPException(status_code=400, detail="缺少category参数")
    
    documents = get_documents(category)
    return ToolOutput(result=documents)

@app.post("/mcp/tools/get_document_content")
async def api_get_document_content(input_data: ToolInput) -> ToolOutput:
    """获取文档内容"""
    doc_path = input_data.args.get("path", "")
    if not doc_path:
        raise HTTPException(status_code=400, detail="缺少path参数")
    
    content = get_document_content(doc_path)
    return ToolOutput(result=content)

@app.post("/mcp/tools/search_documents")
async def api_search_documents(input_data: ToolInput) -> ToolOutput:
    """搜索文档"""
    query = input_data.args.get("query", "")
    if not query:
        raise HTTPException(status_code=400, detail="缺少query参数")
    
    results = []
    for category in get_categories():
        for doc in get_documents(category):
            doc_path = doc["path"]
            content = get_document_content(doc_path)
            if query.lower() in content.lower():
                results.append({
                    "category": category,
                    "name": doc["name"],
                    "path": doc_path,
                    "preview": content[:200] + "..."
                })
    
    return ToolOutput(result=results)

# MCP服务器配置路由
@app.get("/mcp/config")
async def get_mcp_config():
    """获取MCP服务器配置"""
    return {
        "name": "UGUI知识库MCP服务器",
        "description": "提供UGUI知识库的访问接口",
        "tools": [
            {
                "name": "get_categories",
                "description": "获取所有文档分类",
                "inputSchema": {
                    "type": "object",
                    "properties": {}
                }
            },
            {
                "name": "get_documents",
                "description": "获取指定分类下的所有文档",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "category": {
                            "type": "string",
                            "description": "文档分类名称"
                        }
                    },
                    "required": ["category"]
                }
            },
            {
                "name": "get_document_content",
                "description": "获取文档内容",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "path": {
                            "type": "string",
                            "description": "文档路径"
                        }
                    },
                    "required": ["path"]
                }
            },
            {
                "name": "search_documents",
                "description": "搜索文档",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "query": {
                            "type": "string",
                            "description": "搜索关键词"
                        }
                    },
                    "required": ["query"]
                }
            }
        ]
    }

# 主函数
if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)