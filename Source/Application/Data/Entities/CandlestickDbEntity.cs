﻿using Application.Data.Entities.Common;

namespace Application.Data.Entities;

public class CandlestickDbEntity : BaseEntity
{
    public required string CurrencyPair { get; set; }
    public required DateTime DateTime { get; set; }
    public required decimal Open { get; set; }
    public required decimal High { get; set; }
    public required decimal Low { get; set; }
    public required decimal Close { get; set; }
    public required decimal Volume { get; set; }

    public IEnumerable<FuturesOrderDbEntity> FuturesOrders { get; set; } = default!;
}
