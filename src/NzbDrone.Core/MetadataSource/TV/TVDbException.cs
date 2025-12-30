using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.MetadataSource.TV
{
    public class TVDbException : NzbDroneException
    {
        public TVDbException(string message)
            : base(message)
        {
        }

        public TVDbException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TVDbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
