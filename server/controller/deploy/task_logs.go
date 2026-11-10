package deploy

import (
	"ld_shared/sse"
	"strconv"

	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

func GetTaskLogs(c *fiber.Ctx) error {
	taskID := c.Params("taskId")
	if _, err := uuid.Parse(taskID); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "部署任务编号无效"})
	}
	after, err := strconv.ParseUint(c.Query("after", "0"), 10, 64)
	if err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "日志序号无效"})
	}
	history, err := sse.DeploymentLogs.Snapshot(taskID, after)
	if err != nil {
		return c.Status(fiber.StatusGone).JSON(fiber.Map{"message": err.Error()})
	}
	c.Set("Cache-Control", "no-store")
	return c.JSON(history)
}
