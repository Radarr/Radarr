using FluentValidation;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Radarr.Http;

namespace Radarr.Api.V4.ImportLists
{
    [V4ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ImportListResourceMapper();

        public ImportListController(IImportListFactory importListFactory,
                                ProfileExistsValidator profileExistsValidator)
            : base(importListFactory, "importlist", ResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            SharedValidator.RuleForEach(c => c.QualityProfileIds).ValidId();
            SharedValidator.RuleForEach(c => c.QualityProfileIds).SetValidator(profileExistsValidator);
        }
    }
}
