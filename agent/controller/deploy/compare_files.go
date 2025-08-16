package deploy

import (
	"ld_shared/dto"
	"ld_shared/error_response"
	"math"
	"os"
	"path/filepath"

	"log/slog"

	"github.com/gofiber/fiber/v2"
)

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
	for _, fileInfo := range request.FileInfos {
		filePath := filepath.Join(request.ServicePath, fileInfo.FileRelativePath)
		localFileInfo, err := os.Stat(filePath)
		if err != nil {
			// 本地文件不存在，认为是不同的
			differentFiles = append(differentFiles, fileInfo)
			continue
		}

		// 对比文件大小
		if localFileInfo.Size() != fileInfo.FileSize {
			differentFiles = append(differentFiles, fileInfo)
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
		FileInfos: differentFiles,
	})
}