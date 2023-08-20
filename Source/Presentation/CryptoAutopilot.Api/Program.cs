using CryptoAutopilot.Api.Endpoints;
using CryptoAutopilot.Api.Endpoints.Strategies.Automation;
using CryptoAutopilot.Api.HealthChecks;

using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddAzureKeyVault();

builder.Services.AddServices(builder.Configuration);
builder.Services.AddStrategies<Program>(builder.Configuration);

builder.Services.AddHealthChecks(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks();

app.MapApiEndpoints();
app.MapStrategyEndpoints<Program>();

app.Run();
