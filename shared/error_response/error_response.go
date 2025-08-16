package error_response

import (
	"github.com/gofiber/fiber/v2"
)

type ErrorResponse struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
}

func (e *ErrorResponse) Error() string {
	return e.Message
}

func NewErrorResponse(c *fiber.Ctx, message string) error {
	return c.Status(fiber.StatusBadRequest).JSON(&ErrorResponse{
		Code:    400,
		Message: message,
	})
}