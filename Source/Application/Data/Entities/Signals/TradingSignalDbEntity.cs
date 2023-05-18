using Application.Data.Entities.Common;
using Application.Data.Entities.Orders;

using Domain.Models.Signals;

namespace Application.Data.Entities.Signals;

public class TradingSignalDbEntity : DbEntityBase
{
    public int? PositionId { get; set; }
    public virtual FuturesPositionDbEntity? Position { get; set; }


    public required Guid CryptoAutopilotId { get; init; }
    public required string Source { get; set; }
    public required string CurrencyPair { get; set; }
    public required DateTime Time { get; set; }
    public required SignalInfo Info { get; set; }
}
