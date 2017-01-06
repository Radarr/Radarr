using System;
using NzbDrone.Common.Exceptions;

namespace Radarr.Host.Owin
{
    public class PortInUseException : NzbDroneException
    {
        public PortInUseException(string message, Exception innerException, params object[] args) : base(message, innerException, args)
        {
        }
    }
}
