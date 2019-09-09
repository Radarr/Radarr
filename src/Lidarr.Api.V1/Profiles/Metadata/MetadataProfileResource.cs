using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Metadata;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Profiles.Metadata
{
    public class MetadataProfileResource : RestResource
    {
        public string Name { get; set; }
        public List<ProfilePrimaryAlbumTypeItemResource> PrimaryAlbumTypes { get; set; }
        public List<ProfileSecondaryAlbumTypeItemResource> SecondaryAlbumTypes { get; set; }
        public List<ProfileReleaseStatusItemResource> ReleaseStatuses { get; set; }
    }

    public class ProfilePrimaryAlbumTypeItemResource : RestResource
    {
        public NzbDrone.Core.Music.PrimaryAlbumType AlbumType { get; set; }
        public bool Allowed { get; set; }
    }

    public class ProfileSecondaryAlbumTypeItemResource : RestResource
    {
        public NzbDrone.Core.Music.SecondaryAlbumType AlbumType { get; set; }
        public bool Allowed { get; set; }
    }

    public class ProfileReleaseStatusItemResource : RestResource
    {
        public NzbDrone.Core.Music.ReleaseStatus ReleaseStatus { get; set; }
        public bool Allowed { get; set; }
    }

    public static class MetadataProfileResourceMapper
    {
        public static MetadataProfileResource ToResource(this MetadataProfile model)
        {
            if (model == null) return null;

            return new MetadataProfileResource
            {
                Id = model.Id,
                Name = model.Name,
                PrimaryAlbumTypes = model.PrimaryAlbumTypes.ConvertAll(ToResource),
                SecondaryAlbumTypes = model.SecondaryAlbumTypes.ConvertAll(ToResource),
                ReleaseStatuses = model.ReleaseStatuses.ConvertAll(ToResource)
            };
        }

        public static ProfilePrimaryAlbumTypeItemResource ToResource(this ProfilePrimaryAlbumTypeItem model)
        {
            if (model == null) return null;

            return new ProfilePrimaryAlbumTypeItemResource
            {
                AlbumType = model.PrimaryAlbumType,
                Allowed = model.Allowed
            };
        }

        public static ProfileSecondaryAlbumTypeItemResource ToResource(this ProfileSecondaryAlbumTypeItem model)
        {
            if (model == null)
            {
                return null;
            }

            return new ProfileSecondaryAlbumTypeItemResource
            {
                AlbumType = model.SecondaryAlbumType,
                Allowed = model.Allowed
            };
        }

        public static ProfileReleaseStatusItemResource ToResource(this ProfileReleaseStatusItem model)
        {
            if (model == null)
            {
                return null;
            }

            return new ProfileReleaseStatusItemResource
            {
                ReleaseStatus = model.ReleaseStatus,
                Allowed = model.Allowed
            };
        }

        public static MetadataProfile ToModel(this MetadataProfileResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new MetadataProfile
            {
                Id = resource.Id,
                Name = resource.Name,
                PrimaryAlbumTypes = resource.PrimaryAlbumTypes.ConvertAll(ToModel),
                SecondaryAlbumTypes = resource.SecondaryAlbumTypes.ConvertAll(ToModel),
                ReleaseStatuses = resource.ReleaseStatuses.ConvertAll(ToModel)
            };
        }

        public static ProfilePrimaryAlbumTypeItem ToModel(this ProfilePrimaryAlbumTypeItemResource resource)
        {
            if (resource == null) return null;

            return new ProfilePrimaryAlbumTypeItem
            {
                PrimaryAlbumType = (NzbDrone.Core.Music.PrimaryAlbumType)resource.AlbumType.Id,
                Allowed = resource.Allowed
            };
        }

        public static ProfileSecondaryAlbumTypeItem ToModel(this ProfileSecondaryAlbumTypeItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileSecondaryAlbumTypeItem
            { 
                SecondaryAlbumType = (NzbDrone.Core.Music.SecondaryAlbumType)resource.AlbumType.Id,
                Allowed = resource.Allowed
            };
        }

        public static ProfileReleaseStatusItem ToModel(this ProfileReleaseStatusItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileReleaseStatusItem
            {
                ReleaseStatus = (NzbDrone.Core.Music.ReleaseStatus)resource.ReleaseStatus.Id,
                Allowed = resource.Allowed
            };
        }

    public static List<MetadataProfileResource> ToResource(this IEnumerable<MetadataProfile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
