using NzbDrone.Core.ImportLists;

namespace Lidarr.Api.V1.ImportLists
{
    public class ImportListModule : ProviderModuleBase<ImportListResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ImportListResourceMapper();

        public ImportListModule(ImportListFactory importListFactory)
            : base(importListFactory, "importlist", ResourceMapper)
        {
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
