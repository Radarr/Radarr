using System;

namespace NzbDrone.Common.Exceptions
{
    public class ReadarrStartupException : NzbDroneException
    {
        public ReadarrStartupException(string message, params object[] args)
            : base("Readarr failed to start: " + string.Format(message, args))
        {
        }

        public ReadarrStartupException(string message)
            : base("Readarr failed to start: " + message)
        {
        }

        public ReadarrStartupException()
            : base("Readarr failed to start")
        {
        }

        public ReadarrStartupException(Exception innerException, string message, params object[] args)
            : base("Readarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public ReadarrStartupException(Exception innerException, string message)
            : base("Readarr failed to start: " + message, innerException)
        {
        }

        public ReadarrStartupException(Exception innerException)
            : base("Readarr failed to start: " + innerException.Message)
        {
        }
    }
}
