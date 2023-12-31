﻿using System.Text.Json.Serialization;

using Skender.Stock.Indicators;

namespace Domain.Models.Common;

public class Candlestick : IQuote
{
    public required CurrencyPair CurrencyPair { get; init; } = default!;

    public required DateTime Date { get; init; }
    public required decimal Open { get; init; }
    public required decimal High { get; init; }
    public required decimal Low { get; init; }
    public required decimal Close { get; init; }
    public required decimal Volume { get; init; }


    [JsonIgnore]
    public bool IsBullish => this.Close > this.Open;

    [JsonIgnore]
    public bool IsBearish => this.Close < this.Open;

    [JsonIgnore]
    public bool IsDoji => this.Close == this.Open;
}
