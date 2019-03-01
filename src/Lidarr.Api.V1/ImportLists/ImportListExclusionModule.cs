using System.Collections.Generic;
using NzbDrone.Core.ImportLists.Exclusions;
using Lidarr.Http;
using FluentValidation;
using NzbDrone.Core.Validation;

namespace Lidarr.Api.V1.ImportLists
{
    public class ImportListExclusionModule : LidarrRestModule<ImportListExclusionResource>
    {
        private readonly IImportListExclusionService _importListExclusionService;

        public ImportListExclusionModule(IImportListExclusionService importListExclusionService,
                                         ImportListExclusionExistsValidator importListExclusionExistsValidator,
                                         GuidValidator guidValidator)
        {
            _importListExclusionService = importListExclusionService;

            GetResourceById = GetImportListExclusion;
            GetResourceAll = GetImportListExclusions;
            CreateResource = AddImportListExclusion;
            UpdateResource = UpdateImportListExclusion;
            DeleteResource = DeleteImportListExclusionResource;

            SharedValidator.RuleFor(c => c.ForeignId).NotEmpty().SetValidator(guidValidator).SetValidator(importListExclusionExistsValidator);
            SharedValidator.RuleFor(c => c.ArtistName).NotEmpty();
        }

        private ImportListExclusionResource GetImportListExclusion(int id)
        {
            return _importListExclusionService.Get(id).ToResource();
        }

        private List<ImportListExclusionResource> GetImportListExclusions()
        {
            return _importListExclusionService.All().ToResource();
        }

        private int AddImportListExclusion(ImportListExclusionResource resource)
        {
            var customFilter = _importListExclusionService.Add(resource.ToModel());

            return customFilter.Id;
        }

        private void UpdateImportListExclusion(ImportListExclusionResource resource)
        {
            _importListExclusionService.Update(resource.ToModel());
        }

        private void DeleteImportListExclusionResource(int id)
        {
            _importListExclusionService.Delete(id);
        }
    }
}
