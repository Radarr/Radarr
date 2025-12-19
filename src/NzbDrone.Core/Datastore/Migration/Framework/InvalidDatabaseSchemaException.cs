using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class InvalidDatabaseSchemaException : NzbDroneException
    {
        public InvalidDatabaseSchemaException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidDatabaseSchemaException(string message)
            : base(message)
        {
        }

        public InvalidDatabaseSchemaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
