using NzbDrone.Core.NetImport;

namespace NzbDrone.Api.NetImport
{
    public class NetImportModule : ProviderModuleBase<NetImportResource, INetImport, NetImportDefinition>
    {
        public NetImportModule(NetImportFactory indexerFactory)
            : base(indexerFactory, "indexer")
        {
        }

        protected override void MapToResource(NetImportResource resource, NetImportDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enabled = definition.Enabled;
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