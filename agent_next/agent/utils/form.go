package utils

import (
	"encoding/json"
	"net/http"
)

// ParseFormData 解析表单数据到结构体
func ParseFormData(r *http.Request, v interface{}) error {
	if err := r.ParseForm(); err != nil {
		return err
	}

	// 创建一个map来存储表单数据
	formData := make(map[string]interface{})
	for key, values := range r.Form {
		if len(values) > 0 {
			formData[key] = values[0]
		}
	}

	// 将map转换为JSON
	jsonData, err := json.Marshal(formData)
	if err != nil {
		return err
	}

	// 将JSON解析到目标结构体
	return json.Unmarshal(jsonData, v)
} 