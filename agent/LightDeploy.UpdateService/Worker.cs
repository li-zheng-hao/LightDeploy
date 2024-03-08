using LightDeploy.ClientAgent;
using SevenZipExtractor;

namespace LightDeploy.UpdateService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdatePackages");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                _logger.LogInformation($"查找目录:{dir}");
                // 查找dir目录下的所有文件
                var filesA = Directory.GetFiles(dir,"*.rar");
                var filesB = Directory.GetFiles(dir,"*.zip");
                var files=filesA.Concat(filesB).ToList();
                _logger.LogInformation($"查找到的文件{string.Join(",",files)}");
                if (files.Any())
                {
                    Update(files.First());
                    File.Delete(files.First());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
            await Task.Delay(10000, stoppingToken);
        }
    }

    private void Update(string file)
    {
        WindowServiceHelper.StopService("LightDeployAgent",30000);
        var location = WindowServiceHelper.GetWindowsServiceLocation("LightDeployAgent");
        
        var targetDir = Path.GetDirectoryName(location);
        using var file1 = new ArchiveFile(file);
        file1.Extract(targetDir,true);

        WindowServiceHelper.StartService("LightDeployAgent",30000);
    }
}