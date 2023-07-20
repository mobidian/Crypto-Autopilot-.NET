using Bybit.Net;

namespace Infrastructure.Options;

public class BybitEnvironmentOptions
{
    public const string SectionName = "Bybit";

    public required string Environment { get; init; }

    public BybitEnvironment GetEnvironment()
    {
        if (this.Environment == "Live")
        {
            return BybitEnvironment.Live;
        }
        else if (this.Environment == "Testnet")
        {
            return BybitEnvironment.Testnet;
        }
        else
        {
            throw new ArgumentException($"Invalid Bybit environment: {this.Environment}");
        }
    }
}
