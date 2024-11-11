using Blazored.LocalStorage;
using LightApi.Infra;
using LightDeploy.Common;
using LightDeploy.Server.Core;
using LightDeploy.Server.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using App = LightDeploy.Server.Components.App;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogSetup();
builder.Services.AddLightApiSetup(it => { });
builder.Services.AddInfrastructureEfCoreSqlite<LdDbContext>((sp, op) =>
{
    op.UseSqlite("Data Source=lightdeploy.db");
},typeof(AppEntityInfo));

builder.Host.AddAutofacSetup("LightDeploy");
// Add services to the container.
builder.Services.AddRazorComponents(options => 
                                        options.DetailedErrors = true)
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddTransient<AgentService>();
builder.Services.AddTransient<OperationService>();
builder.Services.AddTransient<NotifyService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMvc(it =>
{

}).AddNewtonsoftJson(it =>
{
    it.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

using (var dbContext= app.Services.GetRequiredService<LdDbContext>())
{
    dbContext.Database.Migrate();
}

app.UseInfrastructure();
app.UseStaticFiles();
app.UseAntiforgery();
// app.UseInfrastructure();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();