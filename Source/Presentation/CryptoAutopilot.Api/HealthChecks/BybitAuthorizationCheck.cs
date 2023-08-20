using Application.Interfaces.Services.Bybit;

using Infrastructure.Options;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CryptoAutopilot.Api.HealthChecks;

public class BybitAuthorizationCheck : IHealthCheck
{
    private readonly ApiKeyPermissionsOptions PermissionsOptions;
    private readonly IBybitFuturesAccountDataProvider Account;

    public BybitAuthorizationCheck(IOptions<ApiKeyPermissionsOptions> permissionsOptions, IBybitFuturesAccountDataProvider account)
    {
        this.PermissionsOptions = permissionsOptions.Value;
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

            var readOnlyValueMatch = this.PermissionsOptions.ReadOnlyKey == key.Readonly;
            var hasPermissions = this.PermissionsOptions.Required.All(x => key.Permissions.Contains(x));

            if (!readOnlyValueMatch)
            {
                var str1 = this.PermissionsOptions.ReadOnlyKey ? "read-write" : "read-only";
                var str2 = this.PermissionsOptions.ReadOnlyKey ? "read-only" : "read-write";
                return HealthCheckResult.Unhealthy($"The api key is {str1} when it should be {str2}");
            }

            if (!hasPermissions)
            {
                return HealthCheckResult.Unhealthy("The api key does not have all required permissions");
            }

            var str = this.PermissionsOptions.ReadOnlyKey ? "read-only" : "read-write";
            return HealthCheckResult.Healthy($"The api key is {str} and has all required permissions");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
