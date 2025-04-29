#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
UGUI知识库API接口

这个模块提供了访问UGUI知识库的API接口，支持文档查询、架构图可视化和交互式动画访问等功能。
可以通过MCP工具调用这些接口，获取知识库中的内容。
支持本地模式和服务器模式两种运行方式。
"""

import os
import json
import re
import markdown
import argparse
from flask import Flask, request, jsonify, send_file, render_template
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# 运行模式：local或server
RUN_MODE = 'server'

# 获取项目根目录
ROOT_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DOCS_DIR = os.path.join(ROOT_DIR, 'Docs')
WEB_VIEWER_DIR = os.path.join(ROOT_DIR, 'WebViewer')

# 文档索引
doc_index = {
    # UGUI架构与原理
    'architecture': {
        'basic': os.path.join(DOCS_DIR, 'UGUI', 'UGUIArchitecture.md'),
        'detailed': os.path.join(DOCS_DIR, 'UGUI', 'UGUIDetailedArchitecture.md'),
        'complete': os.path.join(DOCS_DIR, 'UGUI', 'UGUICompleteArchitecture.md'),
        'system_relations': os.path.join(DOCS_DIR, 'UGUI', 'UGUISystemRelations.md'),
        'event_system': os.path.join(DOCS_DIR, 'UGUI', 'UGUI事件系统.md'),
    },
    # UGUI交互式动画
    'animation': {
        'index': os.path.join(DOCS_DIR, 'UGUI', 'UGUI交互式动画索引.md'),
        'architecture': os.path.join(DOCS_DIR, 'UGUI', 'UGUI架构交互式动画.md'),
        'sequence': os.path.join(DOCS_DIR, 'UGUI', 'UGUI时序交互式动画.md'),
    },
    # UGUI使用指南
    'guide': {
        'components': os.path.join(DOCS_DIR, 'Guide', 'UGUI组件使用指南.md'),
        'animation_basic': os.path.join(DOCS_DIR, 'Guide', 'UGUI交互式动画指南.md'),
        'animation_advanced': os.path.join(DOCS_DIR, 'Guide', 'UGUI交互式动画指南_高级部分.md'),
        'animation_optimization': os.path.join(DOCS_DIR, 'Guide', 'UGUI交互式动画指南_优化.md'),
    },
    # UGUI最佳实践
    'best_practice': {
        'optimization': os.path.join(DOCS_DIR, 'UGUI', 'UGUI优化指南.md'),
        'cursor_rules': os.path.join(DOCS_DIR, 'Guide', 'CursorRules.md'),
    }
}

# 可视化图表索引
visualization_index = {
    'architecture': os.path.join(WEB_VIEWER_DIR, 'UGUIArchitectureViewer.html'),
    'animation': os.path.join(WEB_VIEWER_DIR, 'UGUIAnimation.html'),
    'documentation': os.path.join(WEB_VIEWER_DIR, 'UGUIDocumentationViewer.html'),
}

@app.route('/api/docs/<category>/<doc_id>', methods=['GET'])
def get_document(category, doc_id):
    """获取指定分类和ID的文档内容"""
    if category not in doc_index or doc_id not in doc_index[category]:
        return jsonify({'error': '文档不存在'}), 404
    
    doc_path = doc_index[category][doc_id]
    try:
        with open(doc_path, 'r', encoding='utf-8') as f:
            content = f.read()
            # 将Markdown转换为HTML
            html_content = markdown.markdown(content, extensions=['extra', 'codehilite', 'tables'])
            return jsonify({
                'id': doc_id,
                'category': category,
                'content': content,
                'html_content': html_content
            })
    except Exception as e:
        return jsonify({'error': f'读取文档失败: {str(e)}'}), 500

@app.route('/api/search', methods=['GET'])
def search_docs():
    """搜索文档内容"""
    keyword = request.args.get('keyword', '')
    if not keyword:
        return jsonify({'error': '请提供搜索关键词'}), 400
    
    results = []
    for category, docs in doc_index.items():
        for doc_id, doc_path in docs.items():
            try:
                with open(doc_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    if keyword.lower() in content.lower():
                        # 提取匹配上下文
                        lines = content.split('\n')
                        matches = []
                        for i, line in enumerate(lines):
                            if keyword.lower() in line.lower():
                                start = max(0, i - 2)
                                end = min(len(lines), i + 3)
                                context = '\n'.join(lines[start:end])
                                matches.append({
                                    'line': i + 1,
                                    'context': context
                                })
                        
                        # 提取文档标题
                        title = '未知文档'
                        for line in lines[:10]:
                            if line.startswith('# '):
                                title = line[2:].strip()
                                break
                        
                        results.append({
                            'id': doc_id,
                            'category': category,
                            'title': title,
                            'path': doc_path,
                            'matches': matches
                        })
            except Exception as e:
                print(f"搜索文档 {doc_path} 时出错: {str(e)}")
    
    return jsonify({
        'keyword': keyword,
        'count': len(results),
        'results': results
    })

@app.route('/api/visualize/<diagram_id>', methods=['GET'])
def get_visualization(diagram_id):
    """获取可视化图表"""
    if diagram_id not in visualization_index:
        return jsonify({'error': '可视化图表不存在'}), 404
    
    return send_file(visualization_index[diagram_id])

@app.route('/api/examples/<example_id>', methods=['GET'])
def get_example(example_id):
    """获取代码示例"""
    examples_dir = os.path.join(ROOT_DIR, 'Assets', 'Scripts')
    example_path = os.path.join(examples_dir, f"{example_id}.cs")
    
    if not os.path.exists(example_path):
        return jsonify({'error': '代码示例不存在'}), 404
    
    try:
        with open(example_path, 'r', encoding='utf-8') as f:
            content = f.read()
            return jsonify({
                'id': example_id,
                'content': content
            })
    except Exception as e:
        return jsonify({'error': f'读取代码示例失败: {str(e)}'}), 500

@app.route('/api/index', methods=['GET'])
def get_index():
    """获取知识库索引"""
    return jsonify({
        'categories': list(doc_index.keys()),
        'documents': doc_index,
        'visualizations': list(visualization_index.keys()),
    })

class LocalAPI:
    """本地API接口，不启动服务器，直接访问文件系统"""
    
    def __init__(self, root_dir=ROOT_DIR):
        """初始化本地API"""
        self.root_dir = root_dir
        self.docs_dir = os.path.join(root_dir, 'Docs')
        self.web_viewer_dir = os.path.join(root_dir, 'WebViewer')
        self.doc_index = doc_index
        self.visualization_index = visualization_index
    
    def get_document(self, category, doc_id):
        """获取指定分类和ID的文档内容"""
        if category not in self.doc_index or doc_id not in self.doc_index[category]:
            return {'error': '文档不存在'}
        
        doc_path = self.doc_index[category][doc_id]
        try:
            with open(doc_path, 'r', encoding='utf-8') as f:
                content = f.read()
                # 将Markdown转换为HTML
                html_content = markdown.markdown(content, extensions=['extra', 'codehilite', 'tables'])
                return {
                    'id': doc_id,
                    'category': category,
                    'content': content,
                    'html_content': html_content
                }
        except Exception as e:
            return {'error': f'读取文档失败: {str(e)}'}
    
    def search_docs(self, keyword):
        """搜索文档内容"""
        if not keyword:
            return {'error': '请提供搜索关键词'}
        
        results = []
        for category, docs in self.doc_index.items():
            for doc_id, doc_path in docs.items():
                try:
                    with open(doc_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                        if keyword.lower() in content.lower():
                            # 提取匹配上下文
                            lines = content.split('\n')
                            matches = []
                            for i, line in enumerate(lines):
                                if keyword.lower() in line.lower():
                                    start = max(0, i - 2)
                                    end = min(len(lines), i + 3)
                                    context = '\n'.join(lines[start:end])
                                    matches.append({
                                        'line': i + 1,
                                        'context': context
                                    })
                            
                            # 提取文档标题
                            title = '未知文档'
                            for line in lines[:10]:
                                if line.startswith('# '):
                                    title = line[2:].strip()
                                    break
                            
                            results.append({
                                'id': doc_id,
                                'category': category,
                                'title': title,
                                'path': doc_path,
                                'matches': matches
                            })
                except Exception as e:
                    print(f"搜索文档 {doc_path} 时出错: {str(e)}")
        
        return {
            'keyword': keyword,
            'count': len(results),
            'results': results
        }
    
    def get_index(self):
        """获取知识库索引"""
        return {
            'categories': list(self.doc_index.keys()),
            'documents': self.doc_index,
            'visualizations': list(self.visualization_index.keys()),
        }

# 创建全局本地API实例
local_api = LocalAPI()

def main():
    """启动API服务器或返回本地API实例"""
    parser = argparse.ArgumentParser(description="UGUI知识库API")
    parser.add_argument("--mode", choices=["local", "server"], default="server",
                        help="运行模式：local(本地模式)或server(服务器模式)")
    parser.add_argument("--port", type=int, default=5000, help="服务器端口号")
    
    args = parser.parse_args()
    global RUN_MODE
    RUN_MODE = args.mode
    
    if args.mode == "server":
        print(f"以服务器模式启动API，端口: {args.port}")
        app.run(host='0.0.0.0', port=args.port, debug=True)
    else:
        print("以本地模式运行API")
        return local_api

if __name__ == '__main__':
    main()