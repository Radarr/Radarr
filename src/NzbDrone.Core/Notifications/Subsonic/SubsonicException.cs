using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Subsonic
{
    public class SubsonicException : NzbDroneException
    {
        public SubsonicException(string message)
            : base(message)
        {
        }

        public SubsonicException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
