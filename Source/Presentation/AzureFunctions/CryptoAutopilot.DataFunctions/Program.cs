using CryptoAutopilot.Api.Endpoints;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(configuration => configuration.AddUserSecrets<Program>())
    .ConfigureServices((host, services) => services.AddServices(host.Configuration))
    .Build();

host.Run();
