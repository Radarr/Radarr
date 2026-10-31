using NzbDrone.Core.Download;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateMovie : IAggregateLocalMovie
    {
        public int Order => 1;

        private readonly IMovieService _movieService;
        private readonly IMovieVersionService _movieVersionService;

        public AggregateMovie(IMovieService movieService, IMovieVersionService movieVersionService)
        {
            _movieService = movieService;
            _movieVersionService = movieVersionService;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var movie = _movieService.GetMovie(localMovie.Movie.Id);

            // Keep importing into the version being scanned, not into the movie's own folder
            localMovie.Movie = localMovie.Movie.IsVersionView
                ? _movieVersionService.GetVersionView(movie, localMovie.Movie.MovieVersionId)
                : movie;

            return localMovie;
        }
    }
}
