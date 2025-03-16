package command

import (
	"flag"
	"os"
)

type Command struct {
	Port    int
	Release bool
	NssmPath string
}

// Parse 解析命令行参数
func (c *Command) Parse() {
	// 首先检查环境变量
	env := os.Getenv("GO_ENV")
	defaultRelease := env == "Production"
	// 使用自定义的 FlagSet 来支持短参数和长参数
	flag.IntVar(&c.Port, "p", 9001, "服务器端口号 (简写: -p)")
	flag.BoolVar(&c.Release, "r", defaultRelease, "是否为发布模式 (简写: -r)")
	flag.StringVar(&c.NssmPath, "n", "", "nssm路径,默认为当前目录 (简写: -n)")
	flag.Parse()
}

// 创建一个单例实例
var Cmd = &Command{}
