namespace NzbDrone.Core.Notifications.Subsonic
{
    public class SubsonicAuthenticationException : SubsonicException
    {
        public SubsonicAuthenticationException(string message)
            : base(message)
        {
        }

        public SubsonicAuthenticationException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
