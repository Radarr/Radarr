using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Datastore.Converters
{
    public class SecondaryAlbumTypeIntConverter : JsonConverter, IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return SecondaryAlbumType.Studio;
            }

            var val = Convert.ToInt32(context.DbValue);

            return (SecondaryAlbumType) val;
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

            if (clrValue as SecondaryAlbumType == null)
            {
                throw new InvalidOperationException("Attempted to save an album type that isn't really an album type");
            }

            var secType = (SecondaryAlbumType) clrValue;
            return (int) secType;
        }

        public Type DbType => typeof(int);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SecondaryAlbumType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var item = reader.Value;
            return (SecondaryAlbumType) Convert.ToInt32(item);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
