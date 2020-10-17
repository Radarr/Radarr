using FluentValidation;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.ImportList
{
    public class ImportListModule : ProviderModuleBase<ImportListResource, IImportList, ImportListDefinition>
    {
        public ImportListModule(ImportListFactory importListFactory, ProfileExistsValidator profileExistsValidator)
            : base(importListFactory, "netimport")
        {
            PostValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            PostValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            SharedValidator.RuleFor(c => c.ProfileId).ValidId();
            SharedValidator.RuleFor(c => c.ProfileId).SetValidator(profileExistsValidator);
        }

        protected override void MapToResource(ImportListResource resource, ImportListDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enabled = definition.Enabled;
            resource.EnableAuto = (int)definition.EnableAuto;
            resource.ProfileId = definition.ProfileId;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.MinimumAvailability = definition.MinimumAvailability;
            resource.Tags = definition.Tags;
        }

        protected override void MapToModel(ImportListDefinition definition, ImportListResource resource)
        {
            base.MapToModel(definition, resource);

            definition.Enabled = resource.Enabled;
            definition.EnableAuto = (ImportListType)resource.EnableAuto;
            definition.ProfileId = resource.ProfileId;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.MinimumAvailability = resource.MinimumAvailability;
            definition.Tags = resource.Tags;
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
