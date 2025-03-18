using Newtonsoft.Json;

namespace KafkaStreamsProcessor;

public class NaIntConverter : JsonConverter<int>
{
    public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // If the token is a string, check for "NA"
        if (reader.TokenType == JsonToken.String)
        {
            var value = reader.Value?.ToString();
            if (string.Equals(value, "NA", StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }
            // Otherwise, attempt to parse the string as an integer
            if (int.TryParse(value, out int result))
            {
                return result;
            }
        }
        // If the token is already an integer, return it directly
        if (reader.TokenType == JsonToken.Integer)
        {
            return Convert.ToInt32(reader.Value);
        }
        // Throw an error for unexpected token types
        throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing integer.");
    }

    public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}