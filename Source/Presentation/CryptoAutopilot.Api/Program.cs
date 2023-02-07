using CryptoAutopilot.Api.Endpoints;
using CryptoAutopilot.Api.Endpoints.Internal.Automation.General;
using CryptoAutopilot.Api.Endpoints.Internal.Automation.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddServices(builder.Configuration);
builder.Services.AddServices<Program>(builder.Configuration);
builder.Services.AddStrategies<Program>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapEndpoints();
app.MapEndpoints<Program>();
app.MapStrategyEndpoints<Program>();

app.Run();
