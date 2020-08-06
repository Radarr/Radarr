using FluentValidation;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace Radarr.Api.V3.ImportLists
{
    public class ImportListModule : ProviderModuleBase<ImportListResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ImportListResourceMapper();

        public ImportListModule(ImportListFactory importListFactory, ProfileExistsValidator profileExistsValidator)
            : base(importListFactory, "importlist", ResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            SharedValidator.RuleFor(c => c.QualityProfileId).ValidId();
            SharedValidator.RuleFor(c => c.QualityProfileId).SetValidator(profileExistsValidator);
        }

        protected override void Validate(ImportListDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable)
            {
                return;
            }

            base.Validate(definition, includeWarnings);
        }
    }
}
