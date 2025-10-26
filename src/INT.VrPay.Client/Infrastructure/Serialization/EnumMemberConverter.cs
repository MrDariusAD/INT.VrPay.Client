using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace INT.VrPay.Client.Converters;

/// <summary>
/// JSON converter that serializes enums using their EnumMember attribute values.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
public class EnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, string> _enumToString = new();
    private readonly Dictionary<string, TEnum> _stringToEnum = new();

    public EnumMemberConverter()
    {
        var type = typeof(TEnum);
        foreach (var value in Enum.GetValues<TEnum>())
        {
            var memberInfo = type.GetMember(value.ToString())[0];
            var enumMemberAttr = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            
            var stringValue = enumMemberAttr?.Value ?? value.ToString();
            _enumToString[value] = stringValue;
            _stringToEnum[stringValue] = value;
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (stringValue != null && _stringToEnum.TryGetValue(stringValue, out var enumValue))
        {
            return enumValue;
        }

        throw new JsonException($"Unable to convert \"{stringValue}\" to enum {typeof(TEnum).Name}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_enumToString[value]);
    }
}

/// <summary>
/// JSON converter that serializes nullable enums using their EnumMember attribute values.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
public class NullableEnumMemberConverter<TEnum> : JsonConverter<TEnum?> where TEnum : struct, Enum
{
    private readonly EnumMemberConverter<TEnum> _innerConverter = new();

    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return _innerConverter.Read(ref reader, typeof(TEnum), options);
    }

    public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            _innerConverter.Write(writer, value.Value, options);
        }
    }
}
