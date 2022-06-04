using System;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Notifications
{
    public class ApplicationUpdateMessage
    {
        public string Message { get; set; }
        public Version PreviousVersion { get; set; }
        public Version NewVersion { get; set; }
        public UpdateChanges Changes { get; set; }

        public override string ToString()
        {
            return NewVersion.ToString();
        }
    }
}
