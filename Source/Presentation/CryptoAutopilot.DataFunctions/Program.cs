using Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", optional: false);
        configuration.AddUserSecrets<Program>();
        
        configuration.AddAzureKeyVault();
        configuration.AddEnvironmentVariables();
    })
    .ConfigureServices((host, services) => services.AddServices(host.Configuration))
    .Build();

await host.RunAsync();
