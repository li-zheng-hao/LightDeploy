package dto

type CompareFilesRequest struct {
	FileInfos      []CompareFileInfo `json:"fileInfos"`
	ServicePath    string            `json:"servicePath"`
	UseContentHash bool              `json:"useContentHash,omitempty"`
}
type CompareFileInfo struct {
	FileRelativePath string
	FileName         string
	FileSize         int64
	ModifyTimeStamp  int64
	ModifyTimeNano   int64  `json:"ModifyTimeNano,omitempty"`
	ContentHash      string `json:"ContentHash,omitempty"`
}
type CompareFilesResponse struct {
	FileInfos             []CompareFileInfo `json:"fileInfos"`
	HashRequiredFileInfos []CompareFileInfo `json:"hashRequiredFileInfos,omitempty"`
	SupportsContentHash   bool              `json:"supportsContentHash"`
}
