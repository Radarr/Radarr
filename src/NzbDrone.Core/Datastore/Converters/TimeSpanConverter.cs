using System;
using System.Data;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DapperTimeSpanConverter : SqlMapper.TypeHandler<TimeSpan>
    {
        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.Value = value.ToString();
        }

        public override TimeSpan Parse(object value)
        {
            return TimeSpan.Parse((string)value);
        }
    }
}
