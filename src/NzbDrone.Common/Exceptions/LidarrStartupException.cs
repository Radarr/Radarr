using System;

namespace NzbDrone.Common.Exceptions
{
    public class LidarrStartupException : NzbDroneException
    {
        public LidarrStartupException(string message, params object[] args)
            : base("Lidarr failed to start: " + string.Format(message, args))
        {

        }

        public LidarrStartupException(string message)
            : base("Lidarr failed to start: " + message)
        {

        }

        public LidarrStartupException()
            : base("Lidarr failed to start")
        {

        }

        public LidarrStartupException(Exception innerException, string message, params object[] args)
            : base("Lidarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public LidarrStartupException(Exception innerException, string message)
            : base("Lidarr failed to start: " + message, innerException)
        {
        }

        public LidarrStartupException(Exception innerException)
            : base("Lidarr failed to start: " + innerException.Message)
        {

        }
    }
}
