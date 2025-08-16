package router

import (
	"ld_server/controller/agent"
	"ld_server/controller/deploy"
	"ld_server/controller/history"
	"ld_server/controller/service"
	"ld_server/controller/sse"
	"ld_server/controller/target"

	"github.com/gofiber/fiber/v2"
)

func RegisterRoutes(app *fiber.App) {
	app.Get("/api/ping", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{
			"message": "pong",
		})
	})
	sseGroup := app.Group("/api/sse")
	sseGroup.Get("/connect", sse.HandleSSE)
	sseGroup.Get("/send", sse.SendMessage)

	serviceGroup := app.Group("/api/service")
	serviceGroup.Get("/status/:serviceId", service.GetServiceStatus)
	serviceGroup.Get("/list", service.GetDeployServices)
	serviceGroup.Post("/create", service.CreateDeployService)
	serviceGroup.Post("/update", service.UpdateDeployService)
	serviceGroup.Post("/delete/:id", service.DeleteDeployService)
	serviceGroup.Post("/install-service", service.InstallService)
	serviceGroup.Post("/delete-service", service.DeleteService)

	targetGroup := app.Group("/api/target")
	targetGroup.Get("/list", target.ListTargets)
	targetGroup.Get("/service", target.GetAllService)
	targetGroup.Get("/hosts", target.GetAllHosts)
	targetGroup.Post("/create", target.CreateTarget)
	targetGroup.Post("/update", target.UpdateTarget)
	targetGroup.Post("/delete/:id", target.DeleteTarget)
	targetGroup.Get("/:id", target.GetTarget)

	deployGroup := app.Group("/api/deploy")
	deployGroup.Post("/deploy-service", deploy.DeployService)
	deployGroup.Post("/start-service", deploy.StartService)
	deployGroup.Post("/stop-service", deploy.StopService)

	historyGroup := app.Group("/api/history")
	historyGroup.Get("/page-list", history.GetHistoryPageList)
	historyGroup.Get("/:serviceId", history.GetHistory)

	agentGroup := app.Group("/api/agent")
	agentGroup.Get("/version", agent.GetVersion)
	agentGroup.Get("/all", agent.GetAllAgent)
	agentGroup.Post("/update", agent.UpdateAgent)

}
