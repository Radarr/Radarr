using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public class ImportListExclusion : ModelBase
    {
        public int TmdbId { get; set; }
        public string MovieTitle { get; set; }
        public int MovieYear { get; set; }

        public new string ToString()
        {
            return string.Format("Excluded Movie: [{0}][{1} {2}]", TmdbId, MovieTitle, MovieYear);
        }
    }
}
