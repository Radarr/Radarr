using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.NetImport.ImportExclusions
{
    public class ImportExclusion : ModelBase
    {
        public int TmdbId { get; set; }
        public string MovieTitle { get; set; }
        public int MovieYear { get; set; }

        new public string ToString()
        {
            return string.Format("Excluded Movie: [{0}][{1} {2}]", TmdbId, MovieTitle, MovieYear);
        }
    }
}
