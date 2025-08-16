package static

import (
	"embed"
	"io/fs"
	"net/http"
)

//go:embed all:dist
var distFS embed.FS

func GetDistFS() http.FileSystem {
	fsys, err := fs.Sub(distFS, "dist")
	if err != nil {
		panic(err)
	}
	return http.FS(fsys)
}

// GetDistPath 返回嵌入文件的根路径
func GetDistPath() string {
	return "static/dist"
}
