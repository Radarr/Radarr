using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common.Disk
{
    public class PathCombinationException : NzbDroneException
    {
        public PathCombinationException(string message, params object[] args)
            : base(message, args)
        {
        }

        public PathCombinationException(string message)
            : base(message)
        {
        }
    }
}
