using CryptoExchange.Net.Authentication;

namespace Infrastructure.Options;

public class HmacApiCredentialsOptions
{
    public const string SectionName = "Bybit:ApiCredentials";

    public required string Key { get; init; }
    public required string Secret { get; init; }


    public ApiCredentials GetApiCredentials() => new ApiCredentials(this.Key, this.Secret, ApiCredentialsType.Hmac);
}
