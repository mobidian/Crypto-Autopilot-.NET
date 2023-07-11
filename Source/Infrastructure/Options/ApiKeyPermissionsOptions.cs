namespace Infrastructure.Options;

public class ApiKeyPermissionsOptions
{
    public const string SectionName = "Bybit:ApiKeyPermissions";
    
    public required bool ReadOnlyKey { get; init; }
    public required IEnumerable<string> Required { get; init; }
    public required IEnumerable<string> IpAddressesWhitelist { get; init; }
}