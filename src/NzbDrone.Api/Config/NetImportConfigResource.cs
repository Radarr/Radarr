using NzbDrone.Core.Configuration;
using Radarr.Http.REST;

namespace NzbDrone.Api.Config
{
    public class ImportListConfigResource : RestResource
    {
        public int ImportListSyncInterval { get; set; }
        public string ListSyncLevel { get; set; }
        public string ImportExclusions { get; set; }
    }

    public static class ImportListConfigResourceMapper
    {
        public static ImportListConfigResource ToResource(IConfigService model)
        {
            return new ImportListConfigResource
            {
                ImportListSyncInterval = model.ImportListSyncInterval,
                ListSyncLevel = model.ListSyncLevel,
                ImportExclusions = model.ImportExclusions,
            };
        }
    }
}
