using LightDeploy.UpdateService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddWindowsService();
    })
    .Build();

host.Run();