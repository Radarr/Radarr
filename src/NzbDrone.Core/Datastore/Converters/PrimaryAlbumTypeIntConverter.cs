using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using NzbDrone.Core.Music;
using System;

namespace NzbDrone.Core.Datastore.Converters
{
    public class PrimaryAlbumTypeIntConverter : JsonConverter, IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return PrimaryAlbumType.Album;
            }

            var val = Convert.ToInt32(context.DbValue);

            return (PrimaryAlbumType) val;
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

            if (clrValue as PrimaryAlbumType == null)
            {
                throw new InvalidOperationException("Attempted to save an album type that isn't really an album type");
            }

            var primType = (PrimaryAlbumType) clrValue;
            return (int) primType;
        }

        public Type DbType => typeof(int);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PrimaryAlbumType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var item = reader.Value;
            return (PrimaryAlbumType) Convert.ToInt32(item);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
