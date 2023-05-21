using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.ValueConverters.Enums;

public class OrderTypeConverter : ValueConverter<OrderType, string>
{
    public OrderTypeConverter() : base(
        @enum => @enum.ToString(),
        @string => Enum.Parse<OrderType>(@string))
    {
    }
}
