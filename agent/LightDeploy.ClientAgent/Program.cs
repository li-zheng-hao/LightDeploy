using LightApi.Infra;
using LightDeploy.ClientAgent;
using LightDeploy.ClientAgent.Auth;
using LightDeploy.ClientAgent.Domain;
using LightDeploy.ClientAgent.Hubs;
using LightDeploy.ClientAgent.Services;
using LightDeploy.Common;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogSetup();
builder.Services.AddLightApiSetup(it =>
{
});
builder.Host.AddAutofacSetup(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWindowsService();
builder.Services.AddScoped<DeployService>();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddInfrastructureEfCoreSqlite<LdAgentDbContext>((sp, op) =>
{
    op.UseSqlite("Data Source=lightdeploy_agent_v2.db");
},typeof(LdAgentAppDbEntityInfo));
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

builder.Services.AddMvc(it =>
{

}).AddNewtonsoftJson(it =>
{
    it.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var dbContext= app.Services.GetRequiredService<LdAgentDbContext>())
{
    dbContext.Database.Migrate();
}
app.AddConnectionIdMiddleware();
app.UseInfrastructure();
app.MapHub<DeployHub>("/agent");
app.MapControllers();
app.Run();