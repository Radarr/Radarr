using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using NzbDrone.Core.CustomFormats;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DapperQualityTagStringConverter : SqlMapper.TypeHandler<FormatTag>
    {
        public override void SetValue(IDbDataParameter parameter, FormatTag value)
        {
            parameter.Value = value.Raw;
        }

        public override FormatTag Parse(object value)
        {
            if (value == null || value is DBNull)
            {
                return new FormatTag(""); //Will throw argument exception!
            }

            return new FormatTag(Convert.ToString(value));
        }
    }

    public class QualityTagStringConverter : JsonConverter<FormatTag>
    {
        public override FormatTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item = reader.GetString();
            return new FormatTag(Convert.ToString(item));
        }

        public override void Write(Utf8JsonWriter writer, FormatTag value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Raw);
        }
    }
}
