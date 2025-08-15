package version

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

var (
	VERSION = "1.1.1"
)

func GetVersion(c *gin.Context) {
	c.JSON(http.StatusOK, gin.H{
		"version": VERSION,
	})
}
