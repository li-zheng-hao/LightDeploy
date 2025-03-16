package router

import (
	"net/http"

	"ld_server/controller/agent"
	"ld_server/controller/deploy"
	"ld_server/controller/history"
	"ld_server/controller/service"
	"ld_server/controller/sse"
	"ld_server/controller/target"

	"github.com/gin-gonic/gin"
)

func RegisterRoutes(r *gin.Engine) {
	r.GET("/", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"message": "pong",
		})
	})
	sseGroup := r.Group("/api/sse")
	sseGroup.GET("/connect", sse.HandleSSE)
	sseGroup.GET("/send", sse.SendMessage)

	serviceGroup := r.Group("/api/service")
	serviceGroup.GET("/status/:serviceId", service.GetServiceStatus)
	serviceGroup.GET("/list", service.GetDeployServices)
	serviceGroup.POST("/create", service.CreateDeployService)
	serviceGroup.POST("/update", service.UpdateDeployService)
	serviceGroup.POST("/delete/:id", service.DeleteDeployService)

	targetGroup := r.Group("/api/target")
	targetGroup.GET("/list", target.ListTargets)
	targetGroup.POST("/create", target.CreateTarget)
	targetGroup.POST("/update", target.UpdateTarget)
	targetGroup.POST("/delete/:id", target.DeleteTarget)

	deployGroup := r.Group("/api/deploy")
	deployGroup.POST("/deploy-service", deploy.DeployService)

	historyGroup := r.Group("/api/history")
	historyGroup.GET("/:serviceId", history.GetHistory)

	agentGroup := r.Group("/api/agent")
	agentGroup.GET("/version", agent.GetVersion)
	agentGroup.GET("/all", agent.GetAllAgent)
	agentGroup.POST("/update", agent.UpdateAgent)

}