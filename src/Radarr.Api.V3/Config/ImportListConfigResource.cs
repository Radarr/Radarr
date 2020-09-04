using System.Collections.Generic;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tags;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Config
{
    public class ImportListConfigResource : RestResource
    {
        public int ImportListSyncInterval { get; set; }
        public string ListSyncLevel { get; set; }
        public string ImportExclusions { get; set; }
        public HashSet<int> CleanLibraryTags { get; set; }
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
                CleanLibraryTags = model.CleanLibraryTags
            };
        }
    }
}
