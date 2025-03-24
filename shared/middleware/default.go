package middleware

import (
	"github.com/gin-contrib/requestid"
	"github.com/gin-gonic/gin"
)

func RegisterDefaultMiddleware(r *gin.Engine) {
	r.Use(requestid.New())
	r.Use(LogMiddleware())
	r.Use(ErrorMiddleware())
}
