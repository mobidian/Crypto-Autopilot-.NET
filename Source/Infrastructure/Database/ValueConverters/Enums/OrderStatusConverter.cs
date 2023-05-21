﻿using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.ValueConverters.Enums;

public class OrderStatusConverter : ValueConverter<OrderStatus, string>
{
    public OrderStatusConverter() : base(
        @enum => @enum.ToString(),
        @string => Enum.Parse<OrderStatus>(@string))
    {
    }
}
