using Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests.Integration.Bybit.Abstract;

public abstract class BybitServicesTestBase
{
    protected readonly ConfigurationManager Configuration;
    protected readonly IServiceCollection Services;
    
    protected BybitServicesTestBase()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationManager();

        configuration.AddJsonFile("appsettings.test.json", optional: false);
        configuration.AddUserSecrets<BybitServicesTestBase>();
        configuration.AddAzureKeyVault();
        

        this.Services = services;
        this.Configuration = configuration;
    }
}
