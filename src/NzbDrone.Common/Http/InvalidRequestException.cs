using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common.Http
{
    public class InvalidRequestException : NzbDroneException
    {
        public InvalidRequestException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidRequestException(string message)
            : base(message)
        {
        }

        public InvalidRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
