package config

import (
	"os"

	"gopkg.in/yaml.v3"
	"lightdeploy.agentclient/log"
)

type Config struct {
	Server struct {
		Host    string `yaml:"host"`
		Port    int    `yaml:"port"`
		ApiKey  string `yaml:"apiKey"`
		Version string `yaml:"version"`
	} `yaml:"server"`
}

var cfg *Config

func GetConfig() *Config {
	return cfg
}

func InitConfig(path string) error {
	// 读取配置
	cfg = &Config{}
	configFile, err := os.ReadFile(path)
	if err != nil {
		log.Error("读取配置文件失败: %s", err.Error())
		return err
	}
	err = yaml.Unmarshal(configFile, cfg)
	if err != nil {
		log.Error(err.Error())
		return err
	}
	return nil
}
