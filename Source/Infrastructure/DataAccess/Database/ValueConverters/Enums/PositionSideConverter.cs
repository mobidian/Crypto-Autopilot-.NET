using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.DataAccess.Database.ValueConverters.Enums;

public class PositionSideConverter : ValueConverter<PositionSide, string>
{
    public PositionSideConverter() : base(
        @enum => @enum.ToString(),
        @string => Enum.Parse<PositionSide>(@string))
    {
    }
}
