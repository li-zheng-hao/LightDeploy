#!/bin/bash
# LightDeploy Agent Linux 安装脚本（systemd）
# 用法：sudo bash install_agent_linux_service.sh
set -e

if [ "$(id -u)" -ne 0 ]; then
  echo "请使用 root 权限运行此脚本 (sudo bash install_agent_linux_service.sh)"
  exit 1
fi

SERVICE_NAME="LightDeployAgentV2"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BINARY_PATH="${SCRIPT_DIR}/bin/agent_linux/agent"
UNIT_PATH="/etc/systemd/system/${SERVICE_NAME}.service"

if [ ! -f "$BINARY_PATH" ]; then
  echo "未找到 agent 可执行文件: ${BINARY_PATH}"
  echo "请先执行 build_agent_linux.bat 生成 Linux 版 agent"
  exit 1
fi

chmod +x "$BINARY_PATH"

cat > "$UNIT_PATH" <<EOF
[Unit]
Description=LightDeploy Agent Service
After=network.target

[Service]
Type=simple
ExecStart=${BINARY_PATH} -p 9002
Restart=always
RestartSec=5
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable "$SERVICE_NAME"
systemctl restart "$SERVICE_NAME"

echo "安装成功，服务名: ${SERVICE_NAME}"
echo "查看状态: systemctl status ${SERVICE_NAME}"