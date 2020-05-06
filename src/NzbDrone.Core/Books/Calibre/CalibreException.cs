using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Books.Calibre
{
    public class CalibreException : NzbDroneException
    {
        public CalibreException(string message)
            : base(message)
        {
        }

        public CalibreException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
