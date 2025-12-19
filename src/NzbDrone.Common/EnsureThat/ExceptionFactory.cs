using System;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.EnsureThat
{
    internal static class ExceptionFactory
    {
        private static readonly Logger Logger = LogManager.GetLogger("ArgumentValidator");

        internal static ArgumentException CreateForParamValidation(string paramName, string message)
        {
            Logger.Warn(message.SanitizeForLog());
            return new ArgumentException(message, paramName);
        }

        internal static ArgumentNullException CreateForParamNullValidation(string paramName, string message)
        {
            Logger.Warn(message.SanitizeForLog());
            return new ArgumentNullException(paramName, message);
        }
    }
}
