package router

import (
	"ld_agent/controller/deploy"
	"ld_agent/controller/service"
	"ld_agent/controller/sse"
	"ld_agent/controller/version"
	"net/http"

	"github.com/gin-gonic/gin"
)

func RegisterRoutes(r *gin.Engine) {
	r.GET("/", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"message": "pong",
		})
	})
	sseGroup := r.Group("/api/sse")
	sseGroup.GET("/sse", sse.HandleSSE)

	serviceGroup := r.Group("/api/service")
	serviceGroup.GET("/get-windows-service-status", service.GetWindowsServiceStatus)

	deployGroup := r.Group("/api/deploy")
	deployGroup.POST("/deploy-windows-service", deploy.DeployWindowsService)
	deployGroup.POST("/compare-files", deploy.CompareFiles)
	deployGroup.POST("/start-service", deploy.StartService)
	deployGroup.POST("/stop-service", deploy.StopService)

	versionGroup := r.Group("/api/version")
	versionGroup.GET("", version.GetVersion)
	versionGroup.POST("/update", version.UpdateAgent)
}
