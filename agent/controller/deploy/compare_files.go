package deploy

import (
	"fmt"
	"ld_shared/dto"
	"ld_shared/error_response"
	"ld_shared/filehash"
	"math"
	"os"
	"path/filepath"

	"log/slog"

	"github.com/gofiber/fiber/v2"
)

var targetFileHashes interface {
	Hash(string) (string, error)
} = filehash.NewCache(10000)

func CompareFiles(c *fiber.Ctx) error {
	slog.Info("开始比较文件")
	var request dto.CompareFilesRequest
	if err := c.BodyParser(&request); err != nil {
		slog.Error("请求参数绑定失败", "error", err)
		return error_response.NewErrorResponse(c, err.Error())
	}

	slog.Info("开始文件对比",
		"servicePath", request.ServicePath,
		"fileCount", len(request.FileInfos))

	if request.ServicePath == "" {
		return error_response.NewErrorResponse(c, "服务路径不能为空")
	}
	if len(request.FileInfos) == 0 {
		return error_response.NewErrorResponse(c, "文件信息不能为空")
	}
	var differentFiles []dto.CompareFileInfo
	var hashRequiredFiles []dto.CompareFileInfo
	for _, fileInfo := range request.FileInfos {
		filePath := filepath.Join(request.ServicePath, fileInfo.FileRelativePath)
		localFileInfo, err := os.Stat(filePath)
		if os.IsNotExist(err) {
			// 本地文件不存在，认为是不同的
			differentFiles = append(differentFiles, fileInfo)
			continue
		}
		if err != nil {
			return error_response.NewErrorResponse(c, fmt.Sprintf("读取目标文件信息失败 %s: %v", fileInfo.FileRelativePath, err))
		}

		// 对比文件大小
		if !localFileInfo.Mode().IsRegular() || localFileInfo.Size() != fileInfo.FileSize {
			differentFiles = append(differentFiles, fileInfo)
			continue
		}

		if request.UseContentHash {
			if fileInfo.ContentHash != "" {
				localHash, err := targetFileHashes.Hash(filePath)
				if err != nil {
					// 目标文件可能被运行中的程序写入；无法确认一致时直接更新。
					slog.Warn("目标文件内容校验失败，标记为需要更新", "file", fileInfo.FileRelativePath, "error", err)
					differentFiles = append(differentFiles, fileInfo)
					continue
				}
				if localHash != fileInfo.ContentHash {
					differentFiles = append(differentFiles, fileInfo)
				}
			} else if !sameModifyTime(localFileInfo, fileInfo) {
				// 第一轮只筛选，不读取文件内容；第二轮收到源哈希后再校验。
				hashRequiredFiles = append(hashRequiredFiles, fileInfo)
			}
			continue
		}

		// 对比修改时间，如果差距大于5秒则认为不同
		localModTime := localFileInfo.ModTime().Unix()
		if math.Abs(float64(localModTime-fileInfo.ModifyTimeStamp)) > 5 {
			differentFiles = append(differentFiles, fileInfo)
			continue
		}
	}

	return c.JSON(dto.CompareFilesResponse{
		FileInfos:             differentFiles,
		HashRequiredFileInfos: hashRequiredFiles,
		SupportsContentHash:   true,
	})
}

func sameModifyTime(localInfo os.FileInfo, sourceInfo dto.CompareFileInfo) bool {
	// ZIP 解压会丢失小数秒；目标时间只有秒精度时按秒比较。
	if sourceInfo.ModifyTimeNano != 0 && localInfo.ModTime().Nanosecond() != 0 {
		return localInfo.ModTime().UnixNano() == sourceInfo.ModifyTimeNano
	}
	return localInfo.ModTime().Unix() == sourceInfo.ModifyTimeStamp
}
