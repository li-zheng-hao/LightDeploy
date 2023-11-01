using LightDeploy.ClientAgent.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWindowsService();
builder.Services.AddScoped<DeployService>();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.WebHost.ConfigureKestrel(it => it.Limits.MaxRequestBodySize = 1000*1000*1000);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();