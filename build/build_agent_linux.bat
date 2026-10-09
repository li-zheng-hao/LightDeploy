@echo off
setlocal
REM Build Linux agent (amd64)
set GOOS=linux
set GOARCH=amd64
set GOAMD64=v1
go build -o bin/agent_linux/agent -ldflags="-s -w" ../agent
echo "agent linux build success"
endlocal