using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles.Metadata;
using Lidarr.Http;

namespace Lidarr.Api.V1.Profiles.Metadata
{
    public class MetadataProfileModule : LidarrRestModule<MetadataProfileResource>
    {
        private readonly IMetadataProfileService _profileService;

        public MetadataProfileModule(IMetadataProfileService profileService)
        {
            _profileService = profileService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.PrimaryAlbumTypes).MustHaveAllowedPrimaryType();
            SharedValidator.RuleFor(c => c.SecondaryAlbumTypes).MustHaveAllowedSecondaryType();
            SharedValidator.RuleFor(c => c.ReleaseStatuses).MustHaveAllowedReleaseStatus();

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(MetadataProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(MetadataProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);
        }

        private MetadataProfileResource GetById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        private List<MetadataProfileResource> GetAll()
        {
            var profiles = _profileService.All().ToResource();

            return profiles;
        }
    }
}
