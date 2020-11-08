using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.DiscordNotifier
{
    public class DiscordNotifierException : NzbDroneException
    {
        public DiscordNotifierException(string message)
            : base(message)
        {
        }

        public DiscordNotifierException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
