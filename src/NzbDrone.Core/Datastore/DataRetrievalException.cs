using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Datastore
{
    public class DataRetrievalException : NzbDroneException
    {
        public DataRetrievalException(string message, params object[] args)
            : base(message, args)
        {
        }

        public DataRetrievalException(string message)
            : base(message)
        {
        }

        public DataRetrievalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
