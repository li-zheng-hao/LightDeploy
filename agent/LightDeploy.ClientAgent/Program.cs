using LightApi.Infra.DependencyInjections;
using LightDeploy.ClientAgent;
using LightDeploy.ClientAgent.Auth;
using LightDeploy.ClientAgent.Hubs;
using LightDeploy.ClientAgent.Services;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogSetup();
builder.Services.AddInfraSetup(builder.Configuration);
builder.Host.AddAutofacSetup(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWindowsService();
builder.Services.AddScoped<DeployService>();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSqlSugarSetup();
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; 
});
builder.Services.AddSignalR();
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

app.AddConnectionIdMiddleware();
app.UseInfrastructure();
app.MapHub<DeployHub>("/agent");
app.MapControllers();
app.Run();