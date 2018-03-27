using System.Linq;
using NzbDrone.Core.Profiles.Metadata;
using Lidarr.Http;

namespace Lidarr.Api.V1.Profiles.Metadata
{
    public class MetadataProfileSchemaModule : LidarrRestModule<MetadataProfileResource>
    {

        public MetadataProfileSchemaModule()
            : base("/metadataprofile/schema")
        {
            GetResourceSingle = GetAll;
        }

        private MetadataProfileResource GetAll()
        {
            var orderedPrimTypes = NzbDrone.Core.Music.PrimaryAlbumType.All
                .OrderByDescending(l => l.Id)
                .ToList();

            var orderedSecTypes = NzbDrone.Core.Music.SecondaryAlbumType.All
                .OrderByDescending(l => l.Id)
                .ToList();

            var orderedRelStatuses = NzbDrone.Core.Music.ReleaseStatus.All
                .OrderByDescending(l => l.Id)
                .ToList();

            var primTypes = orderedPrimTypes
                .Select(v => new ProfilePrimaryAlbumTypeItem {PrimaryAlbumType = v, Allowed = false})
                .ToList();

            var secTypes = orderedSecTypes
                .Select(v => new ProfileSecondaryAlbumTypeItem {SecondaryAlbumType = v, Allowed = false})
                .ToList();

            var relStatuses = orderedRelStatuses
                .Select(v => new ProfileReleaseStatusItem {ReleaseStatus = v, Allowed = false})
                .ToList();

            var profile = new MetadataProfile
                          {
                              PrimaryAlbumTypes = primTypes,
                              SecondaryAlbumTypes = secTypes,
                              ReleaseStatuses = relStatuses
                          };

            return profile.ToResource();
        }
    }
}
