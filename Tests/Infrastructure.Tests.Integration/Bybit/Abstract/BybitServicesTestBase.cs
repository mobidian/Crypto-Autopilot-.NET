using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Bybit.Net.Clients;
using Bybit.Net.Objects;

using CryptoExchange.Net.Authentication;

using CryptoExchange.Net.Objects;

namespace Infrastructure.Tests.Integration.Bybit.Abstract;

public abstract class BybitServicesTestBase
{
    protected const string ConnectionString = """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";
    protected readonly BybitClient BybitClient;

    protected BybitServicesTestBase()
    {
        var testSecretsClient = new SecretClient(new Uri("https://cryptoautopilottestvault.vault.azure.net"),
            new ClientSecretCredential(
                "83f7176e-c8fc-4168-b2a8-143ba58db998",
                "ff76a9d5-9683-4198-8023-71181db1c4dc",
                "laL8Q~MLKdCKLD4V5OeA_VMxZuWO0iH5znXNXaUc"));

        var publicKey = testSecretsClient.GetSecret("BybitApiCredentials--key").Value.Value;
        var privateKey = testSecretsClient.GetSecret("BybitApiCredentials--secret").Value.Value;


        var options = new BybitClientOptions
        {
            UsdPerpetualApiOptions = new RestApiClientOptions
            {
                ApiCredentials = new ApiCredentials(publicKey, privateKey),
                BaseAddress = "https://api-testnet.bybit.com"
            }
        };
        
        this.BybitClient = new BybitClient(options);
    }
}
