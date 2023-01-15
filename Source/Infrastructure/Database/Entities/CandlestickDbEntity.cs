using Infrastructure.Database.Entities.Common;

namespace Infrastructure.Database.Entities;

public class CandlestickDbEntity : BaseEntity
{
    public required string CurrencyPair { get; set; }
    public required DateTime DateTime { get; set; }
    public required string Open { get; set; }
    public required string High { get; set; }
    public required string Low { get; set; }
    public required string Close { get; set; }

    public List<FuturesOrderDbEntity> FuturesOrderDbEntities { get; set; } = default!;
}
