using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public class ImportListExclusion : ModelBase
    {
        public string ForeignId { get; set; }
        public string Name { get; set; }
    }
}
