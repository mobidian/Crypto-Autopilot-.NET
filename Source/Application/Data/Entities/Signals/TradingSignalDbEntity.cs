using Application.Data.Entities.Common;

using Domain.Models.Signals;

namespace Application.Data.Entities.Signals;

public class TradingSignalDbEntity : DbEntityBase
{
    public required Guid CryptoAutopilotId { get; init; }

    public required string Source { get; set; }

    public required string Contract { get; set; }

    public required DateTime Time { get; set; }

    public required SignalInfo Info { get; set; }
}
