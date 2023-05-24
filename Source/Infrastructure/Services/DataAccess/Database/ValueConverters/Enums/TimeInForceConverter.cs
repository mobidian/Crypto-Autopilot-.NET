using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Services.DataAccess.Database.ValueConverters.Enums;

public class TimeInForceConverter : ValueConverter<TimeInForce, string>
{
    public TimeInForceConverter() : base(
        @enum => @enum.ToString(),
        @string => Enum.Parse<TimeInForce>(@string))
    {
    }
}
