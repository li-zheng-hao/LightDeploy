package dto

type CompareFilesRequest struct {
	FileInfos   []CompareFileInfo `json:"fileInfos"`
	ServicePath string            `json:"servicePath"`
}
type CompareFileInfo struct {
	FileRelativePath string
	FileName         string
	FileSize         int64
	ModifyTimeStamp  int64
}
type CompareFilesResponse struct {
	FileInfos []CompareFileInfo `json:"fileInfos"`
}
