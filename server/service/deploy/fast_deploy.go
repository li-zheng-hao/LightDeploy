package deploy

import (
	"fmt"
	"ld_server/model"
	"ld_shared/dto"
	"ld_shared/filehash"
	"ld_shared/sse"
	"os"
	"path/filepath"
)

var sourceFileHashes = filehash.NewCache(10000)

// FastDeploySource 在一次发布中共用源文件清单和按需计算的哈希。
type FastDeploySource struct {
	sourceDir string
	fileInfos []dto.CompareFileInfo
	files     map[string]dto.CompareFileInfo
	hashes    *filehash.Cache
}

func NewFastDeploySource(sourceDir, ignoreFileRegex string) (*FastDeploySource, error) {
	fileInfos, err := CollectSourceFileInfos(sourceDir, ignoreFileRegex)
	if err != nil {
		return nil, fmt.Errorf("收集文件信息失败: %w", err)
	}
	files := make(map[string]dto.CompareFileInfo, len(fileInfos))
	for _, info := range fileInfos {
		files[info.FileRelativePath] = info
	}
	return &FastDeploySource{
		sourceDir: sourceDir,
		fileInfos: fileInfos,
		files:     files,
		hashes:    sourceFileHashes,
	}, nil
}

func (s *FastDeploySource) CompareWithTarget(target model.DeployTarget) (*dto.CompareFilesResponse, error) {
	if len(s.fileInfos) == 0 {
		return &dto.CompareFilesResponse{}, nil
	}
	// 第一轮只发送元数据，不计算或传输内容哈希。
	result, err := CompareFilesWithTarget(target, s.fileInfos)
	if err != nil {
		return nil, err
	}
	if !result.SupportsContentHash {
		sse.SendMessage(fmt.Sprintf("目标机器 %s 的 agent 不支持内容校验，使用原有大小和时间比较；升级 agent 后可避免时间变化导致的重复复制", target.Host))
		return result, nil
	}
	if len(result.HashRequiredFileInfos) == 0 {
		return result, nil
	}

	// 只为目标端需要确认的文件计算哈希；后续目标通过缓存复用。
	fileInfos := make([]dto.CompareFileInfo, 0, len(result.HashRequiredFileInfos))
	for _, requested := range result.HashRequiredFileInfos {
		info, ok := s.files[requested.FileRelativePath]
		if !ok {
			return nil, fmt.Errorf("目标机器请求校验未知文件: %s", requested.FileRelativePath)
		}
		path := filepath.Join(s.sourceDir, info.FileRelativePath)
		if err := validateSourceFile(path, info); err != nil {
			return nil, err
		}
		hash, err := s.hashes.Hash(path)
		if err != nil {
			return nil, fmt.Errorf("计算源文件哈希失败 %s: %w", info.FileRelativePath, err)
		}
		if err := validateSourceFile(path, info); err != nil {
			return nil, err
		}
		info.ContentHash = hash
		fileInfos = append(fileInfos, info)
	}
	hashResult, err := CompareFilesWithTarget(target, fileInfos)
	if err != nil {
		return nil, err
	}
	if !hashResult.SupportsContentHash || len(hashResult.HashRequiredFileInfos) != 0 {
		return nil, fmt.Errorf("目标机器 %s 未完成文件内容校验", target.Host)
	}
	result.FileInfos = append(result.FileInfos, hashResult.FileInfos...)
	result.HashRequiredFileInfos = nil
	return result, nil
}

func validateSourceFile(path string, expected dto.CompareFileInfo) error {
	info, err := os.Stat(path)
	if err != nil {
		return err
	}
	if !info.Mode().IsRegular() || info.Size() != expected.FileSize || info.ModTime().UnixNano() != expected.ModifyTimeNano {
		return fmt.Errorf("收集文件信息后源文件发生变化，请重试发布: %s", expected.FileRelativePath)
	}
	return nil
}
