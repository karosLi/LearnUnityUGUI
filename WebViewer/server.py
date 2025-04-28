#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import http.server
import socketserver
import os
import sys

# 默认端口
PORT = 8080

def run_server(port=PORT):
    """运行HTTP服务器"""
    # 获取当前脚本所在目录
    current_dir = os.path.dirname(os.path.abspath(__file__))
    # 将工作目录设置为项目根目录（即Assets的上一级目录）
    os.chdir(os.path.join(current_dir, '..'))
    
    handler = http.server.SimpleHTTPRequestHandler
    with socketserver.TCPServer(("", port), handler) as httpd:
        print(f"服务器运行在 http://localhost:{port}/")
        print("在浏览器中访问: http://localhost:{}/WebViewer/UGUIArchitectureViewer.html".format(port))
        print("按 Ctrl+C 停止服务器")
        httpd.serve_forever()

if __name__ == "__main__":
    # 检查命令行参数是否提供了端口
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except ValueError:
            print(f"错误: 无效的端口号 '{sys.argv[1]}'")
            sys.exit(1)
    else:
        port = PORT
    
    try:
        run_server(port)
    except KeyboardInterrupt:
        print("\n服务器已停止")
        sys.exit(0)
    except Exception as e:
        print(f"错误: {e}")
        sys.exit(1) 