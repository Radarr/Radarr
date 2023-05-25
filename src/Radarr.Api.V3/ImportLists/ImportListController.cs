using FluentValidation;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Radarr.Http;

namespace Radarr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, ImportListBulkResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ImportListResourceMapper();
        public static readonly ImportListBulkResourceMapper BulkResourceMapper = new ImportListBulkResourceMapper();

        public ImportListController(IImportListFactory importListFactory, ProfileExistsValidator profileExistsValidator)
            : base(importListFactory, "importlist", ResourceMapper, BulkResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            SharedValidator.RuleFor(c => c.QualityProfileId).ValidId();
            SharedValidator.RuleFor(c => c.QualityProfileId).SetValidator(profileExistsValidator);
        }
    }
}
