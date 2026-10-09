# LightDeploy Linux 部署说明

本文档介绍如何将 LightDeploy 的 agent 部署到 Linux 目标机，并通过 server 发布程序到 Linux。

## 一、原理与总体架构

- **server / server_web**：部署控制台，运行在 Windows 上（端口 31003）
- **agent**：部署代理，运行在目标机上（端口 9002）。同一个 agent 二进制可按操作系统自适应：
  - Windows：使用系统服务控制管理器（SCM）
  - Linux：使用 **systemd**（`systemctl`）
- server 通过 HTTP 调用 agent 完成“安装服务 / 部署 / 启动 / 停止 / 状态查询 / 删除服务”，agent 内部按平台选择实现，server 无需区分目标机操作系统

## 二、目标机环境要求

| 项目 | 要求 |
|------|------|
| 操作系统 | 已安装 systemd 的主流发行版（CentOS 7+ / Ubuntu 16+ / Debian 9+ 等） |
| 权限 | agent 需要 **root** 权限（写 `/etc/systemd/system`、执行 `systemctl`） |
| 架构 | 默认构建 `linux/amd64`；其他架构请自行交叉编译 |
| .NET 程序 | 若程序为 `.dll`，需安装对应版本的 **.NET Runtime**（框架依赖发布） |
| 网络 | 目标机需放通 agent 端口（默认 9002）与业务端口 |

## 三、构建 Linux 版 agent

在 Windows 开发机上执行：

```bat
build\build_agent_linux.bat
```

该脚本等价于：

```bat
set GOOS=linux
set GOARCH=amd64
go build -o bin/agent_linux/agent -ldflags="-s -w" ../agent
```

产物路径：`build\bin\agent_linux\agent`

## 四、安装 agent 为 systemd 服务

1. 将 `bin/agent_linux/agent` 上传到目标机，保持目录结构（脚本默认从脚本同级目录的 `bin/agent_linux/agent` 读取程序）：

```
/opt/ld-build/
├── install_agent_linux_service.sh
├── uninstall_agent_linux_service.sh
└── bin/
    └── agent_linux/
        └── agent
```

2. 以 root 执行安装脚本：

```bash
cd /opt/ld-build
sudo bash install_agent_linux_service.sh
```

3. 脚本会生成 `/etc/systemd/system/LightDeployAgentV2.service`，并执行 `daemon-reload`、`enable`、`restart`。

常用命令：

```bash
systemctl status LightDeployAgentV2     # 查看状态
systemctl restart LightDeployAgentV2    # 重启
journalctl -u LightDeployAgentV2 -f     # 查看日志
```

4. 卸载：

```bash
sudo bash uninstall_agent_linux_service.sh
```

> 提示：如需自定义监听端口，可修改单元文件中的 `ExecStart=... -p 9002` 后执行 `systemctl daemon-reload && systemctl restart LightDeployAgentV2`。

## 五、在 Web 中配置 Linux 目标机

进入“服务配置”页面，选择服务后点击“配置发布目标”，新增目标时：

| 字段 | 说明 | 示例 |
|------|------|------|
| 主机 | 目标机 IP / 域名 | `192.168.1.100` |
| 端口 | agent 端口 | `9002` |
| 操作系统 | 选择 **Linux** | Linux |
| 服务路径 | 程序部署目录 | `/opt/myapp` |
| 程序执行路径 | 程序完整路径 | `/opt/myapp/myapp` 或 `/opt/myapp/MyApp.dll` |
| 程序执行参数 | 启动参数（可选） | `--urls http://0.0.0.0:5000` |
| 备注 | 说明（可选） | 生产环境 |

配置完成后：

- 服务配置页填写“项目路径”（待发布的文件夹，或 .NET 发布产物目录）与“忽略文件正则”
- 点击“安装服务”生成并启用 systemd 服务
- 点击“部署”进行发布

## 六、agent 各接口在 Linux 下的行为

| 功能 | 接口 | Linux 实现 |
|------|------|-----------|
| 安装服务 | `POST /api/service/install-service` | 解压压缩包 → 生成 unit 文件 → `daemon-reload` → `enable` |
| 部署 | `POST /api/deploy/deploy-windows-service` | 停服务 → 解压 → `chmod +x` → 启服务 |
| 启动 | `POST /api/deploy/start-service` | `systemctl start <服务名>` |
| 停止 | `POST /api/deploy/stop-service` | `systemctl stop <服务名>` |
| 状态 | `GET /api/service/get-windows-service-status` | `systemctl is-active <服务名>`，映射为“正在运行/已停止”等 |
| 删除 | `POST /api/service/delete-service` | `stop` → `disable` → 删除 unit 文件 → `daemon-reload` |

> 说明：`deploy-windows-service`、`get-windows-service-status` 为兼容旧版保留的名称，Windows/Linux 通用；同时提供通用别名 `deploy-service`、`get-service-status`。

## 七、生成的 systemd 单元文件示例

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

- `ExecStart` 由“程序执行路径 + 程序执行参数”生成
- 若程序执行路径以 `.dll` 结尾，则自动使用 `dotnet <dll> <参数>` 启动
- 服务默认 `Restart=always`，异常退出后自动拉起

## 八、agent 自更新（Linux）

通过 Web 的“Agent 更新”功能上传新版本 agent 时，Linux 下会：

1. 将新程序保存到 agent 所在目录的 `temp/agent_new`
2. 生成并后台执行 shell 脚本：`systemctl stop` → `cp` 覆盖 → `chmod +x` → `systemctl start`
3. agent 退出后由脚本完成替换与重启

服务名称需与安装时一致（默认 `LightDeployAgentV2`）。

## 九、常见问题

| 现象 | 原因与解决 |
|------|-----------|
| 安装服务失败、提示 `systemctl ... 执行失败` | agent 非 root 运行，或目标机无 systemd；确认以 root 运行且 `systemctl` 可用 |
| 部署后服务启动失败 | 查看 `journalctl -u <服务名> -e`；常见为缺少可执行权限（应自动 chmod）、.NET Runtime 缺失或端口被占用 |
| .NET 程序无法启动 | 确认 `exePath` 指向 `.dll`，且目标机已安装对应 .NET Runtime |
| 状态显示“服务不存在” | systemd unit 未生成，先执行“安装服务” |
| 找不到可执行文件 | 确认路径与“服务路径/程序执行路径”一致，注意 Linux 路径大小写敏感 |