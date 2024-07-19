using Blazored.LocalStorage;
using LightDeploy.Server.Components;
using LightDeploy.Server.Core;
using LightDeploy.Server.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogSetup();
builder.Services.AddSqlSugarSetup();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<OperationService>();
builder.Services.AddScoped<NotifyService>();
builder.Services.AddBlazoredLocalStorage();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();