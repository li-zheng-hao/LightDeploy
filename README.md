# LightDeploy 服务部署工具

## 服务端口

1. server：31003
2. agent：9002

## 目录结构

1. agent：发布代理服务，部署在项目发布的目标服务器上
2. server：服务端api服务
3. server_web:服务端web页面
4. shared：agent和server共享项目
5. doc：说明文件
6. build：构建与安装脚本

## 支持的操作系统

- server / server_web：仅在 Windows 上运行
- agent：支持 **Windows** 与 **Linux**，运行时自动适配，对外接口保持一致：
  - Windows：使用系统服务控制管理器（SCM，`sc` / `services.msc`）
  - Linux：使用 **systemd**（`systemctl`）

## 发布方式

统一采用**文件夹发布**（.NET 项目发布方式已移除）：

1. 将待部署的程序目录（或 .NET 的发布产物目录）作为“项目路径”
2. 支持通过忽略文件正则过滤后打包为 zip
3. 部署到目标机后按需停止 / 启动服务

## Linux 部署指南

详细说明见 [doc/Linux部署说明.md](doc/Linux部署说明.md)，快速流程如下。

### 1. 目标机环境要求

- 已安装 systemd 的主流发行版（CentOS 7+ / Ubuntu 16+ / Debian 9+ 等）
- agent 需要 **root** 权限（写入 `/etc/systemd/system`、执行 `systemctl`）
- 若部署的是 .NET 程序（`exePath` 指向 `.dll`），目标机需安装对应的 **.NET Runtime**
- 原生可执行程序需与目标机架构匹配（默认构建 `linux/amd64`）

### 2. 构建 Linux 版 agent

在 Windows 上执行：

```bat
build\build_agent_linux.bat
```

产物：`build\bin\agent_linux\agent`

### 3. 安装 agent 为 systemd 服务

将 `bin\agent_linux\agent` 与 `build\install_agent_linux_service.sh` 上传到目标机（保持相对目录：脚本期望在 `bin/agent_linux/agent`），然后：

```bash
sudo bash install_agent_linux_service.sh
```

安装的服务名为 `LightDeployAgentV2`，默认监听 `9002` 端口：

```bash
systemctl status LightDeployAgentV2
```

卸载：

```bash
sudo bash uninstall_agent_linux_service.sh
```

### 4. 在 Web 中配置 Linux 目标机

- 操作系统：选择 **Linux**
- 服务路径：目标机上的部署目录，如 `/opt/myapp`
- 程序执行路径：
  - 原生可执行文件：`/opt/myapp/myapp`
  - .NET 程序：`/opt/myapp/MyApp.dll`（自动使用 `dotnet` 启动）
- 程序执行参数：如 `--urls http://0.0.0.0:5000`

### 5. 部署流程

1. “安装服务”：上传打包产物并生成 `/etc/systemd/system/<服务名>.service`，执行 `daemon-reload` 与 `enable`
2. “部署”：停止服务 → 解压文件 →（Linux 自动 `chmod +x`）→ 启动服务
3. 状态查询 / 启动 / 停止 / 删除：均通过 `systemctl` 完成

### 6. 生成的 systemd 单元文件示例

```ini
[Unit]
Description=myapp
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/myapp
ExecStart=/opt/myapp/myapp --urls http://0.0.0.0:5000
Restart=always
RestartSec=5
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
```

## Windows 部署指南（简述）

1. 执行 `build\build_agent.bat` 构建 Windows 版 agent
2. 使用 `build\install_agent_windows_service.bat` 安装为 Windows 服务（服务名 `LightDeployAgentV2`）
3. 在 Web 中配置目标机，操作系统选择 **Windows**，程序执行路径填写 `.exe` 完整路径