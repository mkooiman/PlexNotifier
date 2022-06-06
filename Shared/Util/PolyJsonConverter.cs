
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexNotifier.Shared.Util;

public sealed class PolyJsonConverter<T>: JsonConverter<T>
{
    private readonly string _propertyKey;
    private readonly Func<string, Type> _resolver;

    public PolyJsonConverter(
        string propertyKey,
        Func<string, Type> resolver)
    {
        _propertyKey = propertyKey ?? throw new ArgumentNullException(nameof(propertyKey));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }
    
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var copy = reader;

        if (copy.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var depth = copy.CurrentDepth;

        while (copy.Read() && copy.TokenType != JsonTokenType.EndObject && copy.CurrentDepth > depth)
        {
            if (copy.TokenType == JsonTokenType.PropertyName &&
                copy
                    .GetString()!
                    .Equals(_propertyKey, StringComparison.InvariantCultureIgnoreCase))
            {
                copy.Read();
                var value = copy.GetString();
                var type = _resolver.Invoke(value!);

                return (T?) JsonSerializer.Deserialize(ref reader, type, options);
            }
            copy.TrySkip();
        }

        throw new JsonException("Type not found");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer,value, value.GetType(), options);
    }
}