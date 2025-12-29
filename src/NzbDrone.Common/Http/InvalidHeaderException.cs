using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common.Http
{
    public class InvalidHeaderException : NzbDroneException
    {
        public InvalidHeaderException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidHeaderException(string message)
            : base(message)
        {
        }

        public InvalidHeaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
