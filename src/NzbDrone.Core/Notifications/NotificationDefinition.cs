using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public class NotificationDefinition : ProviderDefinition
    {
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnDelete { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool SupportsOnDelete { get; set; }
        public bool IncludeHealthWarnings { get; set; }

        public override bool Enable => OnGrab || OnDownload || (OnDownload && OnUpgrade) || OnHealthIssue || OnDelete;
    }
}
