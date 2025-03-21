cd ../server_web
call pnpm run build
echo "server_web build success"
cd ../build
go build -o bin/server/server.exe -ldflags="-s -w" ../server/main.go
echo "server build success"
