using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Exceptions
{
    public class RadarrStartupException : NzbDroneException
    {
        public RadarrStartupException(string message, params object[] args)
            : base("Radarr failed to start: " + string.Format(message, args))
        {

        }

        public RadarrStartupException(string message)
            : base("Radarr failed to start: " + message)
        {

        }

        public RadarrStartupException()
            : base("Radarr failed to start")
        {

        }

        public RadarrStartupException(Exception innerException, string message, params object[] args)
            : base("Radarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public RadarrStartupException(Exception innerException, string message)
            : base("Radarr failed to start: " + message, innerException)
        {
        }

        public RadarrStartupException(Exception innerException)
            : base("Radarr failed to start: " + innerException.Message)
        {

        }
    }
}
