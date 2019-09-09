using NzbDrone.Core.Datastore;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Profiles.Metadata
{
    public class ProfileSecondaryAlbumTypeItem : IEmbeddedDocument
    {
        public SecondaryAlbumType SecondaryAlbumType { get; set; }
        public bool Allowed { get; set; }
    }
}
