@echo off
setlocal
echo Current Path: %~dp0

REM 设置服务名称
set SERVICE_NAME=LightDeployServerV2

REM 检查是否以管理员权限运行
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Please Run As Administrator!
    pause
    exit /b 1
)

REM 停止服务
sc stop "%SERVICE_NAME%"
timeout /t 2

REM 删除服务
sc delete "%SERVICE_NAME%"

if %errorLevel% equ 0 (
    echo Uninstall Successfully!
) else (
    echo Uninstall Failed!
)

pause