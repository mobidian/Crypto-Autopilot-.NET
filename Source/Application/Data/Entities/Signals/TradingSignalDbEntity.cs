using Application.Data.Entities.Common;

namespace Application.Data.Entities.Signals;

public class TradingSignalDbEntity : DbEntityBase
{
    public required Guid CryptoAutopilotId { get; init; }
    public required string Source { get; set; }
    public required string CurrencyPair { get; set; }
    public required DateTime Time { get; set; }
    public required string Info { get; set; }
}
