using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles.Metadata;
using Readarr.Http;

namespace Readarr.Api.V1.Profiles.Metadata
{
    public class MetadataProfileModule : ReadarrRestModule<MetadataProfileResource>
    {
        private readonly IMetadataProfileService _profileService;

        public MetadataProfileModule(IMetadataProfileService profileService)
        {
            _profileService = profileService;
            SharedValidator.RuleFor(c => c.Name).NotEqual("None").WithMessage("'None' is a reserved profile name").NotEmpty();

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
