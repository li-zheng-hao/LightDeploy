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
	serviceGroup.Get("/get-windows-service-status", deploy.GetWindowsServiceStatus)
	serviceGroup.Post("/install-service", deploy.InstallService)
	serviceGroup.Post("/delete-service", deploy.DeleteService)

	deployGroup := app.Group("/api/deploy")
	deployGroup.Post("/deploy-windows-service", deploy.DeployWindowsService)
	deployGroup.Post("/compare-files", deploy.CompareFiles)
	deployGroup.Post("/start-service", deploy.StartService)
	deployGroup.Post("/stop-service", deploy.StopService)

	versionGroup := app.Group("/api/version")
	versionGroup.Get("", version.GetVersion)
	versionGroup.Post("/update", version.UpdateAgent)
}