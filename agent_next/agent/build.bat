@echo off
echo 开始构建项目...

:: 创建dist目录（如果不存在）
if not exist "dist" mkdir dist

:: 创建config目录（如果不存在）
if not exist "dist\config" mkdir dist\config

:: 构建项目
go build -o ./dist/agent.exe

if %errorlevel% equ 0 (
    echo 构建成功！
    echo 输出文件位置: %cd%\dist\agent.exe
    
    :: 复制配置文件
    echo 正在复制配置文件...
    copy config\config.yaml dist\config\config.yaml
    if %errorlevel% equ 0 (
        echo 配置文件复制成功！
    ) else (
        echo 配置文件复制失败！
    )
) else (
    echo 构建失败！
)

pause 