//go:build linux

package main

// run Linux 平台直接以交互模式运行（通常由 systemd 托管）
func run() error {
	return runInteractive()
}