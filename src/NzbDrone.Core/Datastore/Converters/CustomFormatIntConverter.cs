using System;
using System.ServiceModel;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Core.Qualities;
using Newtonsoft.Json;
using NzbDrone.Core.CustomFormats;

namespace NzbDrone.Core.Datastore.Converters
{
    public class CustomFormatIntConverter : JsonConverter, IConverter
    {
        //TODO think of something better.
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return null;
            }

            var val = Convert.ToInt32(context.DbValue);

            if (val == 0)
            {
                return CustomFormat.None;
            }

            if (CustomFormatService.AllCustomFormats == null)
            {
                throw new Exception("***FATAL*** WE TRIED ACCESSING ALL CUSTOM FORMATS BEFORE IT WAS INITIALIZED. PLEASE SAVE THIS LOG AND OPEN AN ISSUE ON GITHUB.");
            }

            return CustomFormatService.AllCustomFormats[val];
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if(clrValue == DBNull.Value) return null;

            if(!(clrValue is CustomFormat))
            {
                throw new InvalidOperationException("Attempted to save a quality definition that isn't really a quality definition");
            }

            var quality = (CustomFormat) clrValue;

            if (CustomFormatService.AllCustomFormats?.ContainsKey(quality.Id) == false)
            {
                //throw new Exception("Attempted to save an unknown custom format! Make sure you do not have stale custom formats lying around!");
            }
            
            return quality.Id;
        }

        public Type DbType => typeof(int);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CustomFormat);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = reader.Value;

            var val = Convert.ToInt32(item);

            if (val == 0)
            {
                return CustomFormat.None;
            }

            if (CustomFormatService.AllCustomFormats == null)
            {
                throw new Exception("***FATAL*** WE TRIED ACCESSING ALL CUSTOM FORMATS BEFORE IT WAS INITIALIZED. PLEASE SAVE THIS LOG AND OPEN AN ISSUE ON GITHUB.");
            }

            return CustomFormatService.AllCustomFormats[val];
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
