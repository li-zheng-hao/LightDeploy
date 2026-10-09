package service

// Manager 跨平台服务管理器接口
// Windows 使用系统服务控制管理器(SCM)，Linux 使用 systemd
type Manager interface {
	// Install 安装服务
	//   serviceName 服务名称
	//   exePath     程序完整执行路径
	//   exeParams   程序执行参数
	//   workDir     服务工作目录
	Install(serviceName, exePath, exeParams, workDir string) error

	// Start 启动服务
	Start(serviceName string) error

	// Stop 停止服务
	Stop(serviceName string) error

	// Delete 删除服务
	Delete(serviceName string) error

	// IsRunning 判断服务是否正在运行
	IsRunning(serviceName string) (bool, error)

	// GetStatusText 获取可读的服务状态文本（如：正在运行、已停止）
	GetStatusText(serviceName string) (string, error)
}

// NewManager 根据当前运行的操作系统返回对应的服务管理器实现
func NewManager() Manager {
	return newManager()
}