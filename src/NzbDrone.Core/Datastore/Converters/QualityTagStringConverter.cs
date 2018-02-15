using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Core.Qualities;
using Newtonsoft.Json;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityTagStringConverter : JsonConverter, IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return new QualityTag(""); //Will throw argument exception!
            }

            var val = Convert.ToString(context.DbValue);

            return new QualityTag(val);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if(clrValue == DBNull.Value) return 0;

            if(!(clrValue is QualityTag))
            {
                throw new InvalidOperationException("Attempted to save a quality tag that isn't really a quality tag");
            }

            var quality = (QualityTag) clrValue;
            return quality.Raw;
        }

        public Type DbType => typeof(string);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(QualityTag);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = reader.Value;
            return new QualityTag(Convert.ToString(item));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}