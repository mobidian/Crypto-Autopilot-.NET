using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", optional: false);
        configuration.AddUserSecrets<Program>();
        
        var root = configuration.Build();
        configuration.AddAzureKeyVault(
            new SecretClient(new Uri(root["KeyVaultConfig:Url"]!),
            new ClientSecretCredential(
                root["KeyVaultConfig:TenantId"],
                root["KeyVaultConfig:ClientId"],
                root["KeyVaultConfig:ClientSecretId"])),
            new AzureKeyVaultConfigurationOptions());

        configuration.AddEnvironmentVariables();
    })
    .ConfigureServices((host, services) => services.AddServices(host.Configuration))
    .Build();

await host.RunAsync();
