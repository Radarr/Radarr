using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public class NotificationDefinition : ProviderDefinition
    {
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnMovieDelete { get; set; }
        public bool OnMovieFileDelete { get; set; }
        public bool OnMovieFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnApplicationUpdate { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnMovieDelete { get; set; }
        public bool SupportsOnMovieFileDelete { get; set; }
        public bool SupportsOnMovieFileDeleteForUpgrade { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public bool SupportsOnApplicationUpdate { get; set; }

        public override bool Enable => OnGrab || OnDownload || (OnDownload && OnUpgrade) || OnMovieDelete || OnMovieFileDelete || OnMovieFileDeleteForUpgrade  || OnHealthIssue || OnApplicationUpdate;
    }
}
