using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DapperUtcConverter : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value.ToUniversalTime();
        }

        public override DateTime Parse(object value)
        {
            return (DateTime)value;
        }
    }

    public class UtcConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
        }
    }
}
