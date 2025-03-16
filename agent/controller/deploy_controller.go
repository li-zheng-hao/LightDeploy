package controller

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

func DeployWindowsService(c *gin.Context) {
	c.JSON(http.StatusOK, gin.H{
		"message": "deploy success",
	})
}
