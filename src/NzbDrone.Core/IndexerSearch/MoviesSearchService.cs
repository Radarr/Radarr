using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.IndexerSearch
{
    public class MovieSearchService : IExecute<MoviesSearchCommand>, IExecute<MissingMoviesSearchCommand>
    {
        private readonly IMovieService _movieService;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public MovieSearchService(IMovieService movieService,
                                   ISearchForNzb nzbSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _movieService = movieService;
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(MoviesSearchCommand message)
        {
            var downloadedCount = 0;
            foreach (var movieId in message.MovieIds)
            {
                var series = _movieService.GetMovie(movieId);
                
                if (!series.Monitored)
                {
                    _logger.Debug("Movie {0} is not monitored, skipping search", series.Title);
                }

                var decisions = _nzbSearchService.MovieSearch(movieId, false);//_nzbSearchService.SeasonSearch(message.MovieId, season.SeasonNumber, false, message.Trigger == CommandTrigger.Manual);
                downloadedCount += _processDownloadDecisions.ProcessDecisions(decisions).Grabbed.Count;

            }
            _logger.ProgressInfo("Movie search completed. {0} reports downloaded.", downloadedCount);
        }

        public void Execute(MissingMoviesSearchCommand message)
        {
            var movies = _movieService.MoviesWithoutFiles(new PagingSpec<Movie>
                                                            {
                                                                Page = 1,
                                                                PageSize = 100000,
                                                                SortDirection = SortDirection.Ascending,
                                                                SortKey = "Id",
                                                                FilterExpression =
                                                                    v =>
                                                                    v.Monitored == true
                                                            }).Records.ToList();
        }
    }
}
