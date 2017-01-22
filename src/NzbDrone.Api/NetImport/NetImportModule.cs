using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Api.NetImport
{
    public class NetImportModule : ProviderModuleBase<NetImportResource, INetImport, NetImportDefinition>
    {
        private readonly IProfileService _profileService;
        public NetImportModule(NetImportFactory indexerFactory, IProfileService profileService)
            : base(indexerFactory, "netimport")
        {
            _profileService = profileService;
        }

        protected override void MapToResource(NetImportResource resource, NetImportDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enabled = definition.Enabled;
            Field theField = null;
            int index = 0;
            foreach (var field in resource.Fields)
            {
                if (field.Label == "Quality Profile")
                {
                    index = resource.Fields.FindIndex(f => f.Label == field.Label);
                    field.SelectOptions =
                        _profileService.All().ConvertAll(p => new SelectOption {Name = p.Name, Value = p.Id});

                    theField = field;
                }
            }

        }

        protected override void MapToModel(NetImportDefinition definition, NetImportResource resource)
        {
            base.MapToModel(definition, resource);

            resource.Enabled = definition.Enabled;
        }

        protected override void Validate(NetImportDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}