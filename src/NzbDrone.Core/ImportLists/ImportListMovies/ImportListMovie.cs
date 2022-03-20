using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.ImportLists.ImportListMovies
{
    public class ImportListMovie : ModelBase
    {
        public ImportListMovie()
        {
            MovieMetadata = new MovieMetadata();
        }

        public int ListId { get; set; }
        public int MovieMetadataId { get; set; }
        public LazyLoaded<MovieMetadata> MovieMetadata { get; set; }

        public string Title
        {
            get { return MovieMetadata.Value.Title; }
            set { MovieMetadata.Value.Title = value; }
        }

        public int TmdbId
        {
            get { return MovieMetadata.Value.TmdbId; }
            set { MovieMetadata.Value.TmdbId = value; }
        }

        public string ImdbId
        {
            get { return MovieMetadata.Value.ImdbId; }
            set { MovieMetadata.Value.ImdbId = value; }
        }

        public int Year
        {
            get { return MovieMetadata.Value.Year; }
            set { MovieMetadata.Value.Year = value; }
        }
    }
}
