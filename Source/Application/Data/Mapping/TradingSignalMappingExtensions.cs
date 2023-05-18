using Application.Data.Entities.Signals;

using Domain.Models.Common;
using Domain.Models.Signals;

namespace Application.Data.Mapping;

public static class TradingSignalMappingExtensions
{
    public static TradingSignalDbEntity ToDbEntity(this TradingSignal signal) => new TradingSignalDbEntity
    {
        CryptoAutopilotId = signal.CryptoAutopilotId,
        Source = signal.Source,
        CurrencyPair = signal.CurrencyPair.Name,
        Time = signal.Time,
        Info = signal.Info.DeepClone(), // prevents unexpected mutations on TradingSignalDbEntity
    };
    
    public static TradingSignal ToDomainObject(this TradingSignalDbEntity entity) => new TradingSignal
    {
        CryptoAutopilotId = entity.CryptoAutopilotId,
        Source = entity.Source,
        CurrencyPair = new CurrencyPair(entity.CurrencyPair),
        Time = entity.Time,
        Info = entity.Info.DeepClone(), // prevents unexpected mutations on TradingSignal
    };
}
