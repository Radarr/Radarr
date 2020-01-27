using System;

namespace Radarr.Host
{
    public class TerminateApplicationException : ApplicationException
    {
        public TerminateApplicationException(string reason)
            : base("Application is being terminated. Reason : " + reason)
        {
        }
    }
}
