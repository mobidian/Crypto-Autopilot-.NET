using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests.Integration.Bybit.Abstract;

public abstract class BybitServicesTestBase
{
    protected readonly ConfigurationManager Configuration;
    protected readonly IServiceCollection Services;
    
    protected BybitServicesTestBase()
    {
        this.Services = new ServiceCollection();


        var configuration = new ConfigurationManager();
        configuration.AddJsonFile("appsettings.test.json", optional: false);
        configuration.AddUserSecrets<BybitServicesTestBase>();

        configuration.AddAzureKeyVault(
            new SecretClient(new Uri(configuration["KeyVaultConfig:Url"]!),
            new ClientSecretCredential(
                configuration["KeyVaultConfig:TenantId"],
                configuration["KeyVaultConfig:ClientId"],
                configuration["KeyVaultConfig:ClientSecretId"])),
            new AzureKeyVaultConfigurationOptions());
        
        this.Configuration = configuration;
    }
}
