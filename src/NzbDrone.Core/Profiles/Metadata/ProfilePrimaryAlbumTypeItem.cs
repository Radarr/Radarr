using NzbDrone.Core.Datastore;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Profiles.Metadata
{
    public class ProfilePrimaryAlbumTypeItem : IEmbeddedDocument
    {
        public PrimaryAlbumType PrimaryAlbumType { get; set; }
        public bool Allowed { get; set; }
    }
}
