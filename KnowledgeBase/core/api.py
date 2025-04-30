#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库API服务器

这个脚本实现了一个基于Flask的API服务器，用于提供UGUI知识库的访问接口。
"""

import os
import sys
import json
import glob
import markdown
from typing import Dict, List, Any
from flask import Flask, request, jsonify
from flask_cors import CORS

# 创建Flask应用
app = Flask(__name__)
CORS(app)  # 启用跨域资源共享

# 获取项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(os.path.dirname(SCRIPT_DIR))
DOCS_DIR = os.path.join(ROOT_DIR, 'Docs')

# 知识库索引缓存
doc_index = {}


def load_doc_index():
    """加载知识库索引"""
    global doc_index
    index_file = os.path.join(os.path.dirname(SCRIPT_DIR), 'docs', '知识库索引.md')
    
    if os.path.exists(index_file):
        with open(index_file, 'r', encoding='utf-8') as f:
            content = f.read()
            # 解析索引文件，提取文档ID和路径信息
            # 简化实现，实际应用中可能需要更复杂的解析逻辑
            doc_index = {}
            for line in content.split('\n'):
                if '|' in line and '[' in line and ']' in line:
                    parts = line.split('|')
                    if len(parts) >= 4:
                        doc_id = parts[1].strip()
                        doc_path = parts[3].strip()
                        if '(' in doc_path and ')' in doc_path:
                            path = doc_path[doc_path.find('(')+1:doc_path.find(')')]
                            doc_index[doc_id] = path


@app.route('/api/docs', methods=['GET'])
def get_docs():
    """获取所有文档列表"""
    if not doc_index:
        load_doc_index()
    return jsonify(list(doc_index.keys()))


@app.route('/api/docs/<doc_id>', methods=['GET'])
def get_doc(doc_id):
    """获取指定ID的文档内容"""
    if not doc_index:
        load_doc_index()
    
    if doc_id not in doc_index:
        return jsonify({'error': f'Document {doc_id} not found'}), 404
    
    doc_path = os.path.join(ROOT_DIR, doc_index[doc_id])
    if not os.path.exists(doc_path):
        return jsonify({'error': f'Document file not found: {doc_path}'}), 404
    
    with open(doc_path, 'r', encoding='utf-8') as f:
        content = f.read()
        html = markdown.markdown(content)
        return jsonify({
            'id': doc_id,
            'content': content,
            'html': html
        })


@app.route('/api/search', methods=['GET'])
def search_docs():
    """搜索文档"""
    query = request.args.get('q', '')
    if not query:
        return jsonify({'error': 'Query parameter is required'}), 400
    
    if not doc_index:
        load_doc_index()
    
    results = []
    for doc_id, path in doc_index.items():
        doc_path = os.path.join(ROOT_DIR, path)
        if os.path.exists(doc_path):
            with open(doc_path, 'r', encoding='utf-8') as f:
                content = f.read()
                if query.lower() in content.lower():
                    results.append({
                        'id': doc_id,
                        'path': path,
                        'preview': content[:200] + '...' if len(content) > 200 else content
                    })
    
    return jsonify(results)


@app.route('/api/visualize/<viz_id>', methods=['GET'])
def get_visualization(viz_id):
    """获取可视化图表"""
    viz_dir = os.path.join(DOCS_DIR, 'Visualizations')
    viz_file = os.path.join(viz_dir, f'{viz_id}.json')
    
    if not os.path.exists(viz_file):
        return jsonify({'error': f'Visualization {viz_id} not found'}), 404
    
    with open(viz_file, 'r', encoding='utf-8') as f:
        viz_data = json.load(f)
        return jsonify(viz_data)


def start_api_server(host='0.0.0.0', port=5000, debug=False):
    """启动API服务器"""
    app.run(host=host, port=port, debug=debug)


if __name__ == '__main__':
    start_api_server(debug=True)