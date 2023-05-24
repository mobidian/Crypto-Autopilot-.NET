using System.Text.Json;

using Domain.Converters;
using Domain.Models.Signals;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Services.DataAccess.Database.ValueConverters;

public class SignalInfoConverter : ValueConverter<SignalInfo, string>
{
    internal SignalInfoConverter() : base(
        v => SerializeToJson(JsonSignalnformationConverter.Empty, v),
        v => DeserIalizeFromJson(JsonSignalnformationConverter.Empty, v))
    {
    }
    public SignalInfoConverter(JsonSignalnformationConverter jsonConverter) : base(
        v => SerializeToJson(jsonConverter, v),
        v => DeserIalizeFromJson(jsonConverter, v))
    {
    }

    private static string SerializeToJson(JsonSignalnformationConverter jsonConverter, SignalInfo v)
    {
        var options = new JsonSerializerOptions { Converters = { jsonConverter } };
        return JsonSerializer.Serialize(v, options);
    }

    private static SignalInfo DeserIalizeFromJson(JsonSignalnformationConverter jsonConverter, string v)
    {
        var options = new JsonSerializerOptions { Converters = { jsonConverter } };
        return JsonSerializer.Deserialize<SignalInfo>(v, options)!;
    }
}
