using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using CryptoAutopilot.Api.Endpoints;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(configuration =>
    {
        var root = configuration.Build();

        configuration.AddAzureKeyVault(
            new SecretClient(new Uri("https://cryptoautopilot-keyvault.vault.azure.net"),
            new ClientSecretCredential(
                "83f7176e-c8fc-4168-b2a8-143ba58db998",
                "ff76a9d5-9683-4198-8023-71181db1c4dc",
                "laL8Q~MLKdCKLD4V5OeA_VMxZuWO0iH5znXNXaUc")),
            new AzureKeyVaultConfigurationOptions());

        configuration.AddEnvironmentVariables();
    })
    .ConfigureServices((host, services) => services.AddServices(host.Configuration))
    .Build();

await host.RunAsync();
