package error_response

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

type ErrorResponse struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
}

func NewErrorResponse(c *gin.Context, message string)  {
	c.JSON(http.StatusBadRequest, &ErrorResponse{
		Code:    400,
		Message: message,
	})
}

func NewErrorResponseWithCode(c *gin.Context, code int, message string)  {
	c.JSON(http.StatusBadRequest, &ErrorResponse{
		Code:    code,
		Message: message,
	})
}
