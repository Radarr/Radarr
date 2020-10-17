using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListDefinition : ProviderDefinition
    {
        public ImportListDefinition()
        {
            Tags = new HashSet<int>();
        }

        public bool Enabled { get; set; }
        public ImportListType EnableAuto { get; set; }
        public bool ShouldMonitor { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public int ProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public bool SearchOnAdd { get; set; }
        public override bool Enable => Enabled;

        public ImportListSource ListType { get; set; }
    }
}
