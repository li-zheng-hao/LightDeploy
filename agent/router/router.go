package router

import (
	"ld_agent/controller/deploy"
	"ld_agent/controller/sse"
	"ld_agent/controller/version"

	"github.com/gofiber/fiber/v2"
)

func RegisterRoutes(app *fiber.App) {
	app.Get("/", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{
			"message": "pong",
		})
	})
	sseGroup := app.Group("/api/sse")
	sseGroup.Get("/sse", sse.HandleSSE)

	serviceGroup := app.Group("/api/service")
	// get-windows-service-status 为兼容旧接口保留，get-service-status 为通用别名
	serviceGroup.Get("/get-windows-service-status", deploy.GetWindowsServiceStatus)
	serviceGroup.Get("/get-service-status", deploy.GetWindowsServiceStatus)
	serviceGroup.Post("/install-service", deploy.InstallService)
	serviceGroup.Post("/delete-service", deploy.DeleteService)

	deployGroup := app.Group("/api/deploy")
	// deploy-windows-service 为兼容旧接口保留，deploy-service 为通用别名
	deployGroup.Post("/deploy-windows-service", deploy.DeployWindowsService)
	deployGroup.Post("/deploy-service", deploy.DeployWindowsService)
	deployGroup.Post("/compare-files", deploy.CompareFiles)
	deployGroup.Post("/start-service", deploy.StartService)
	deployGroup.Post("/stop-service", deploy.StopService)

	versionGroup := app.Group("/api/version")
	versionGroup.Get("", version.GetVersion)
	versionGroup.Post("/update", version.UpdateAgent)
}