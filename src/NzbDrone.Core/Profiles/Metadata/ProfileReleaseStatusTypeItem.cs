using NzbDrone.Core.Datastore;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Profiles.Metadata
{
    public class ProfileReleaseStatusItem : IEmbeddedDocument
    {
        public ReleaseStatus ReleaseStatus { get; set; }
        public bool Allowed { get; set; }
    }
}
