using Application.Interfaces.Services.Bybit;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CryptoAutopilot.Api.HealthChecks;

public class BybitAuthorizationCheck : IHealthCheck
{
    private static readonly bool ReadOnlyKey = false;
    private static readonly List<string> RequiredPermissions = new List<string>
    {
        "SpotTrade",
        "Order",
        "Position",
        "OptionsTrade",
        "DerivativesTrade",
        "ExchangeHistory"
    };


    private readonly IBybitFuturesAccountDataProvider Account;
    public BybitAuthorizationCheck(IBybitFuturesAccountDataProvider account)
    {
        this.Account = account;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var keys = await this.Account.GetAllApiKeysInfoAsync();
            if (keys.Count() > 1)
            {
                return HealthCheckResult.Unhealthy("There is more than one api key");
            }


            var key = keys.Single();
            
            var IsReadWrite = ReadOnlyKey == key.Readonly;
            var hasPermissions = RequiredPermissions.All(x => key.Permissions.Contains(x));

            if (!IsReadWrite)
            {
                return HealthCheckResult.Unhealthy("The api key is read-only");
            }
            
            if (!hasPermissions)
            {
                return HealthCheckResult.Unhealthy("The api key does not have all required permissions");
            }

            return HealthCheckResult.Healthy("The api key is read-write and has all required permissions");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
