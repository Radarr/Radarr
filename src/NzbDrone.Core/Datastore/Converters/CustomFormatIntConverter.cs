using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.CustomFormats;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DapperCustomFormatIntConverter : SqlMapper.TypeHandler<CustomFormat>
    {
        public override void SetValue(IDbDataParameter parameter, CustomFormat value)
        {
            parameter.Value = value.Id;
        }

        public override CustomFormat Parse(object value)
        {
            Console.WriteLine(value.ToJson());

            if (value is DBNull)
            {
                return null;
            }

            var val = Convert.ToInt32(value);

            if (val == 0)
            {
                return CustomFormat.None;
            }

            return CustomFormatService.AllCustomFormats[val];
        }
    }

    public class CustomFormatIntConverter : JsonConverter<CustomFormat>
    {
        public override CustomFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetInt32();

            if (val == 0)
            {
                return CustomFormat.None;
            }

            return CustomFormatService.AllCustomFormats[val];
        }

        public override void Write(Utf8JsonWriter writer, CustomFormat value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Id);
        }
    }
}
