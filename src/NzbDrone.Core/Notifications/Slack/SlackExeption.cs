using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Slack
{
    internal class SlackExeption : NzbDroneException
    {
        public SlackExeption(string message)
            : base(message)
        {
        }

        public SlackExeption(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
