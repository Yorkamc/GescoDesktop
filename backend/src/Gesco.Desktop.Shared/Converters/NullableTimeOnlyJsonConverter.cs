using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gesco.Desktop.Shared.Converters
{
    /// <summary>
    /// JSON Converter que acepta strings vacíos, null o tiempos válidos para TimeOnly?
    /// </summary>
    public class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
    {
        private const string TimeFormat = "HH:mm:ss";
        private const string ShortTimeFormat = "HH:mm";

        public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.String:
                    var stringValue = reader.GetString();

                    // ✅ Si es vacío, whitespace o null → retornar null
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return null;
                    }

                    // Intentar parsear formatos comunes
                    if (TimeOnly.TryParse(stringValue, out var timeValue))
                    {
                        return timeValue;
                    }

                    // Si no se puede parsear, retornar null en lugar de error
                    // Esto hace que sea más tolerante
                    return null;

                default:
                    throw new JsonException($"Unexpected token parsing TimeOnly. Expected String or Null, got {reader.TokenType}.");
            }
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(TimeFormat));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}