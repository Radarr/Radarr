using System;
using NLog;

namespace NzbDrone.Common.Instrumentation
{
    public static class NzbDroneLogger
    {
        public static Logger GetLogger(Type type)
        {
            return LogManager.GetLogger(type.Name.Replace("NzbDrone.", ""));
        }

        public static Logger GetLogger(object obj)
        {
            return GetLogger(obj.GetType());
        }
    }
}
