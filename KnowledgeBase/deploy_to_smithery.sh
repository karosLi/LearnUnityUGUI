#!/bin/bash

# UGUI知识库Smithery.ai部署脚本
# 这个脚本用于简化部署流程，提供一键部署功能

# 获取脚本所在目录
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# 显示彩色输出
GREEN="\033[0;32m"
YELLOW="\033[0;33m"
RED="\033[0;31m"
NC="\033[0m" # No Color

# 显示标题
echo -e "${GREEN}====================================${NC}"
echo -e "${GREEN}  UGUI知识库 Smithery.ai 部署工具  ${NC}"
echo -e "${GREEN}====================================${NC}"
echo 

# 检查Python是否安装
if ! command -v python3 &> /dev/null; then
    echo -e "${RED}错误: 未找到Python3${NC}"
    echo "请先安装Python3"
    exit 1
fi

# 检查部署脚本是否存在
if [ ! -f "$SCRIPT_DIR/deploy_to_smithery.py" ]; then
    echo -e "${RED}错误: 未找到部署脚本 deploy_to_smithery.py${NC}"
    echo "请确保脚本文件存在"
    exit 1
fi

# 显示菜单
echo "请选择要执行的操作:"
echo "1. 全部执行 (创建文件、测试并部署)"
echo "2. 仅创建必要文件"
echo "3. 测试FastMCP服务器"
echo "4. 部署到Smithery.ai平台"
echo "5. 退出"
echo 
read -p "请输入选项 [1-5]: " choice

case $choice in
    1)
        echo -e "${YELLOW}执行全部步骤...${NC}"
        python3 "$SCRIPT_DIR/deploy_to_smithery.py" --all
        ;;
    2)
        echo -e "${YELLOW}创建必要文件...${NC}"
        python3 "$SCRIPT_DIR/deploy_to_smithery.py" --create-server --create-dockerfile --create-yaml
        ;;
    3)
        echo -e "${YELLOW}测试FastMCP服务器...${NC}"
        python3 "$SCRIPT_DIR/test_fastmcp_server.py"
        ;;
    4)
        echo -e "${YELLOW}部署到Smithery.ai平台...${NC}"
        python3 "$SCRIPT_DIR/deploy_to_smithery.py" --deploy
        ;;
    5)
        echo "退出"
        exit 0
        ;;
    *)
        echo -e "${RED}无效的选项${NC}"
        exit 1
        ;;
esac

echo -e "\n${GREEN}操作完成!${NC}"