using LightDeploy.ClientAgent.Auth;
using LightDeploy.ClientAgent.Services;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
string logTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}]  {Message:lj}{NewLine}{Exception}";
var config = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .WriteTo.Console(outputTemplate: logTemplate)
    .WriteTo.File("logs/lightdeploy_.log"
        , rollingInterval: RollingInterval.Day, outputTemplate: logTemplate);
var logger = config.CreateLogger();
builder.Host.UseSerilog(logger, dispose: true);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWindowsService();
builder.Services.AddScoped<DeployService>();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; 
});
builder.WebHost.ConfigureKestrel(it => it.Limits.MaxRequestBodySize =long.MaxValue);
builder.Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = KeyAuthenticationOptions.Scheme;
        option.DefaultChallengeScheme = KeyAuthenticationOptions.Scheme;
 
    })
    .AddKeyAuthentication(options => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();