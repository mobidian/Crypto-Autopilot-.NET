using Application.Data.Entities.Common;

using Domain.Models.Signals;

namespace Application.Data.Entities.Signals;

public class TradingSignalDbEntity : BaseEntity
{
    public required string Source { get; init; }

    public required string Contract { get; init; }

    public required DateTime Time { get; init; }

    public required SignalInfo Info { get; init; }
}