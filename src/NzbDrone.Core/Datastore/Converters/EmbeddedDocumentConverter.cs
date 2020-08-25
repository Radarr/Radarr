using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter<T> : SqlMapper.TypeHandler<T>
    {
        protected readonly JsonSerializerOptions SerializerSettings;

        public EmbeddedDocumentConverter()
        {
            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
            serializerSettings.Converters.Add(new TimeSpanConverter());
            serializerSettings.Converters.Add(new UtcConverter());

            SerializerSettings = serializerSettings;
        }

        public EmbeddedDocumentConverter(params JsonConverter[] converters)
            : this()
        {
            foreach (var converter in converters)
            {
                SerializerSettings.Converters.Add(converter);
            }
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            // Cast to object to get all properties written out
            // https://github.com/dotnet/corefx/issues/38650
            parameter.Value = JsonSerializer.Serialize((object)value, SerializerSettings);
        }

        public override T Parse(object value)
        {
            return JsonSerializer.Deserialize<T>((string)value, SerializerSettings);
        }
    }
}
