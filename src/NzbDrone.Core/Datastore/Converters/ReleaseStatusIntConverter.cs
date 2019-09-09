using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using NzbDrone.Core.Music;
using System;

namespace NzbDrone.Core.Datastore.Converters
{
    public class ReleaseStatusIntConverter : JsonConverter, IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return ReleaseStatus.Official;
            }

            var val = Convert.ToInt32(context.DbValue);

            return (ReleaseStatus) val;
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext {ColumnMap = map, DbValue = dbValue});
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == DBNull.Value)
            {
                return 0;
            }

            if (clrValue as ReleaseStatus == null)
            {
                throw new InvalidOperationException("Attempted to save a release status that isn't really a release status");
            }

            var releaseStatus = (ReleaseStatus) clrValue;
            return (int) releaseStatus;
        }

        public Type DbType => typeof(int);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ReleaseStatus);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var item = reader.Value;
            return (ReleaseStatus) Convert.ToInt32(item);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
