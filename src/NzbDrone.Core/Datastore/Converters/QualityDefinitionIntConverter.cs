using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Core.Qualities;
using Newtonsoft.Json;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityDefinitionIntConverter : JsonConverter, IConverter
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionIntConverter(IQualityDefinitionService qualityDefinitionService)
        {
            _qualityDefinitionService = qualityDefinitionService;
        }

        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return null;
            }

            var val = Convert.ToInt32(context.DbValue);

            return _qualityDefinitionService.GetById(val);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if(clrValue == DBNull.Value) return null;

            if(!(clrValue is QualityDefinition))
            {
                throw new InvalidOperationException("Attempted to save a quality definition that isn't really a quality definition");
            }

            var quality = (QualityDefinition) clrValue;
            return quality.Id;
        }

        public Type DbType => typeof(int);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quality);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = reader.Value;
            return _qualityDefinitionService.GetById(Convert.ToInt32(item));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
