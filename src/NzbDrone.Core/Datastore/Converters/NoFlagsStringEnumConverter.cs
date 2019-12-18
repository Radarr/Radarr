using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.Datastore.Converters
{
    public class NoFlagsStringEnumConverter : JsonConverterFactory
    {
        private static JsonStringEnumConverter s_stringEnumConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false);

        public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsEnum && !typeToConvert.IsDefined(typeof(FlagsAttribute), inherit: false);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => s_stringEnumConverter.CreateConverter(typeToConvert, options);
    }
}
