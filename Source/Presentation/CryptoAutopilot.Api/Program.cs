using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using CryptoAutopilot.Api.Endpoints;
using CryptoAutopilot.Api.Endpoints.Internal.Automation.General;
using CryptoAutopilot.Api.Endpoints.Internal.Automation.Strategies;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddAzureKeyVault(
    new SecretClient(new Uri(builder.Configuration["KeyVaultConfig:Url"]!),
    new ClientSecretCredential(
        builder.Configuration["KeyVaultConfig:TenantId"],
        builder.Configuration["KeyVaultConfig:ClientId"],
        builder.Configuration["KeyVaultConfig:ClientSecretId"])),
    new AzureKeyVaultConfigurationOptions());

builder.Services.AddServices(builder.Configuration);
builder.Services.AddServices<Program>(builder.Configuration);
builder.Services.AddStrategies<Program>(builder.Configuration);

builder.Services.AddSingleton<IStrategiesTracker, StrategiesTracker>();

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
