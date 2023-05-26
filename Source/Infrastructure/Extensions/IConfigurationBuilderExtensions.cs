using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class IConfigurationBuilderExtensions
{
    public static void AddAzureKeyVault(this IConfigurationBuilder configuration)
    {
        var root = configuration.Build();

        configuration.AddAzureKeyVault(
            new SecretClient(new Uri(root["KeyVaultConfig:Url"]!),
            new ClientSecretCredential(
                root["KeyVaultConfig:TenantId"],
                root["KeyVaultConfig:ClientId"],
                root["KeyVaultConfig:ClientSecretId"])),
            new AzureKeyVaultConfigurationOptions());
    }
}
