@echo off
setlocal
go build -o bin/agent/agent.exe -ldflags="-s -w" ../agent
echo "agent build success"
endlocal