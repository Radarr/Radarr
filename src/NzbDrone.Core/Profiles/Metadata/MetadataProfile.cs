using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Profiles.Metadata
{
    public class MetadataProfile : ModelBase
    {
        public string Name { get; set; }
        public List<ProfilePrimaryAlbumTypeItem> PrimaryAlbumTypes { get; set; }
        public List<ProfileSecondaryAlbumTypeItem> SecondaryAlbumTypes { get; set; }
        public List<ProfileReleaseStatusItem> ReleaseStatuses { get; set; }

    }
}
