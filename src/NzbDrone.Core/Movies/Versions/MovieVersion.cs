using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies.Versions
{
    public class MovieVersion : ModelBase
    {
        public int MovieId { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public int MovieFileId { get; set; }

        public bool HasFile => MovieFileId > 0;

        public override string ToString()
        {
            return string.Format("[{0}] Movie {1} ({2})", Id, MovieId, Path);
        }
    }
}
