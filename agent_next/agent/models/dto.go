package models

// FileInfoDto 文件信息
type FileInfoDto struct {
	Path              string `json:"path"`
	LastModified      int64  `json:"lastModified"`
	FileSize          int64  `json:"fileSize"`
	MD5               string `json:"md5"`
	AbsoluteDirectory string `json:"absoluteDirectory"` // 绝对目录路径
	RelativeDirectory string `json:"relativeDirectory"` // 相对目录路径
	FileName          string `json:"fileName"`          // 文件名
}

// InstallWindowsServiceDto 安装Windows服务请求
type InstallWindowsServiceDto struct {
	File               []byte `json:"file"`                         // 上传的文件内容
	ServiceName        string `json:"serviceName"`                  // 服务名称
	Params             string `json:"params,omitempty"`             // 运行参数
	ExeFullPath        string `json:"exeFullPath"`                  // 可执行文件完整路径
	ServiceDescription string `json:"serviceDescription,omitempty"` // 服务描述
}
