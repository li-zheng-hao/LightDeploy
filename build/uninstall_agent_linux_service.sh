#!/bin/bash
# LightDeploy Agent Linux 卸载脚本
# 用法：sudo bash uninstall_agent_linux_service.sh
set -e

if [ "$(id -u)" -ne 0 ]; then
  echo "请使用 root 权限运行此脚本 (sudo bash uninstall_agent_linux_service.sh)"
  exit 1
fi

SERVICE_NAME="LightDeployAgentV2"
UNIT_PATH="/etc/systemd/system/${SERVICE_NAME}.service"

systemctl stop "$SERVICE_NAME" 2>/dev/null || true
systemctl disable "$SERVICE_NAME" 2>/dev/null || true
rm -f "$UNIT_PATH"
systemctl daemon-reload

echo "卸载完成，服务名: ${SERVICE_NAME}"