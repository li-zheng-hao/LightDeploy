// 获取当前脚本路径
set CURRENT_DIR=%~dp0
nssm.exe install LightDeployAgent "%CURRENT_DIR%/LightDeployAgent/LightDeploy.ClientAgent.exe"  
nssm.exe install LightDeployUpdateService "%CURRENT_DIR%/LightDeployUpdateService/LightDeploy.UpdateService.exe" 
nssm.exe start LightDeployAgent
nssm.exe start LightDeployUpdateService