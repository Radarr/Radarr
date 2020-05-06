using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.ImportLists.Goodreads
{
    public class GoodreadsException : NzbDroneException
    {
        public GoodreadsException(string message)
            : base(message)
        {
        }

        public GoodreadsException(string message, params object[] args)
            : base(message, args)
        {
        }

        public GoodreadsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class GoodreadsAuthorizationException : GoodreadsException
    {
        public GoodreadsAuthorizationException(string message)
            : base(message)
        {
        }
    }
}
