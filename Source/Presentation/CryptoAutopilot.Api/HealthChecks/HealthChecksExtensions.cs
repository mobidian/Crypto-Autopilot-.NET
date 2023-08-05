using Azure.Identity;

using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CryptoAutopilot.Api.HealthChecks;

public static class HealthChecksExtensions
{
    public static void AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<BybitAuthorizationCheck>("Bybit Authorization Check")
            .AddSqlServer(configuration.GetConnectionString("TradingHistoryDB")!)
            .AddAzureKeyVault(configuration);
    }
    private static void AddAzureKeyVault(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var tokenCredential = new ClientSecretCredential(
               configuration["KeyVaultConfig:TenantId"],
               configuration["KeyVaultConfig:ClientId"],
               configuration["KeyVaultConfig:ClientSecretId"]);


        var secrets = new List<string>
        {
            "Bybit--ApiCredentials--Key",
            "Bybit--ApiCredentials--Secret",
            "ConnectionStrings--TradingHistoryDB"
        };

        builder.AddAzureKeyVault(new Uri(configuration["KeyVaultConfig:Url"]!), tokenCredential, options => secrets.ForEach(x => options.AddSecret(x)));
    }

    public static void MapHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("_health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
    }
}
