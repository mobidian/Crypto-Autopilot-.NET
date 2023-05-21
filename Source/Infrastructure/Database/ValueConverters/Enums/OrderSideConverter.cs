using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.ValueConverters.Enums;

public class OrderSideConverter : ValueConverter<OrderSide, string>
{
    public OrderSideConverter() : base(
        @enum => @enum == OrderSide.Buy ? "Buy" : "Sell",
        @string => @string == "Buy" ? OrderSide.Buy : OrderSide.Sell)
    {
    }
}
