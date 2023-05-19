﻿using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Bybit.Net.Clients;
using Bybit.Net.Objects;

using CryptoExchange.Net.Authentication;

using CryptoExchange.Net.Objects;

using Microsoft.Extensions.Configuration;

namespace Infrastructure.Tests.Integration.Bybit.Abstract;

public abstract class BybitServicesTestBase
{
    protected const string ConnectionString = """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";
    protected readonly BybitClient BybitClient;

    protected BybitServicesTestBase()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<BybitServicesTestBase>()
            .Build();


        var testSecretsClient = new SecretClient(new Uri(configuration["KeyVaultConfig:Url"]!),
            new ClientSecretCredential(
                configuration["KeyVaultConfig:TenantId"],
                configuration["KeyVaultConfig:ClientId"],
                configuration["KeyVaultConfig:ClientSecretId"]));

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