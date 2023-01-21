using Microsoft.Extensions.Configuration;

namespace Infrastructure.Tests.Integration.Common;

public class SecretsManager
{
    private readonly IConfiguration Configuration;

    public SecretsManager()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<SecretsManager>();
        this.Configuration = builder.Build();
    }

    public string GetSecret(string key) => this.Configuration[key]!;
    public string GetConnectionString(string name) => this.Configuration.GetConnectionString(name)!;
}
