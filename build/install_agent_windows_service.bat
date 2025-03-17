@echo off
setlocal
echo Current Path: %~dp0
REM 设置服务名称和路径
set SERVICE_NAME=LightDeployAgentV2
set SERVICE_PATH=%~dp0bin\agent\agent.exe
set SERVICE_DESC=LightDeploy Agent Service

REM 检查是否以管理员权限运行
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Please Run As Administrator!
    pause
    exit /b 1
)

REM 安装服务
sc create "%SERVICE_NAME%" binPath= "%SERVICE_PATH%" DisplayName= "%SERVICE_NAME%" start= auto
sc description "%SERVICE_NAME%" "%SERVICE_DESC%"

if %errorLevel% equ 0 (
    echo Install Successfully!
) else (
    echo Install Failed!
)

REM 启动服务
sc start "%SERVICE_NAME%"
pause