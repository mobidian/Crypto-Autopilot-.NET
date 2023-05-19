using System.Text.Json;
using System.Text.Json.Serialization;

using Domain.Exceptions;
using Domain.Models.Signals;

namespace Domain.Converters;

/// <summary>
/// <para>Converts <see cref="SignalInfo"/> and its derived types to or from JSON. The converter requires a predefined set of <see cref="SignalInfo.TypeIdentifier"/> and <see cref="Type"/> key-value pairs.</para>
/// <para>These pairs define which <see cref="SignalInfo"/> types the converter can handle.</para>
/// </summary>
public class JsonSignalnformationConverter : JsonConverter<SignalInfo>
{
    public static readonly JsonSignalnformationConverter Empty = new(new Dictionary<string, Type>());


    private readonly IDictionary<string, Type> SignalInfoTypes;
    
    public JsonSignalnformationConverter(IDictionary<string, Type> signalInfoTypes)
    {
        this.SignalInfoTypes = signalInfoTypes;
    }
    

    public override SignalInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unable to parse JSON data. Expected a token of type 'StartObject', but encountered a token of type '{reader.TokenType}' instead.");
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;

        if (!jsonObject.TryGetProperty("TypeIdentifier", out var typeIdentifierProperty))
        {
            throw new JsonException("The provided JSON object does not contain the mandatory 'TypeIdentifier' property. Please ensure the object is in the correct format.");
        }
        
        var typeIdentifier = typeIdentifierProperty.GetString()!;
        if (!this.SignalInfoTypes.TryGetValue(typeIdentifier, out var actualType))
        {
            var keyNotFoundException = new KeyNotFoundException($"The given key '{typeIdentifier}' was not present in the dictionary.");
            var signalTypeNotFoundException = new SignalTypeNotFoundException("The 'TypeIdentifier' property value does not correspond to a registered type.", keyNotFoundException);
            throw new JsonException($"Unable to identify the type of the JSON object.", signalTypeNotFoundException);
        }


        return (SignalInfo)JsonSerializer.Deserialize(jsonObject.GetRawText(), actualType, options)!;
    }

    public override void Write(Utf8JsonWriter writer, SignalInfo value, JsonSerializerOptions options)
    {
        if (!this.SignalInfoTypes.ContainsKey(value.TypeIdentifier))
        {
            var keyNotFoundException = new KeyNotFoundException($"The given key '{value.TypeIdentifier}' was not present in the dictionary.");
            var signalTypeNotFoundException = new SignalTypeNotFoundException("The 'TypeIdentifier' property value does not correspond to a registered type.", keyNotFoundException);
            throw new SignalTypeNotFoundException($"The type '{value.GetType().Name}' of the {typeof(SignalInfo).Name} object is not registered in the dictionary of valid types.", signalTypeNotFoundException);
        }

        
        writer.WriteStartObject();

        foreach (var property in value.GetType().GetProperties())
        {
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, property.GetValue(value), options);
        }

        writer.WriteEndObject();
    }
}
