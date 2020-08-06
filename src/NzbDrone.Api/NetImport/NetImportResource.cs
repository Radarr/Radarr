using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Api.ImportList
{
    public class ImportListResource : ProviderResource
    {
        public bool Enabled { get; set; }
        public bool EnableAuto { get; set; }
        public bool ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int ProfileId { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public HashSet<int> Tags { get; set; }
    }
}
