using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.IndexerSearch
{
    public class MovieSearchService : IExecute<MoviesSearchCommand>, IExecute<MissingMoviesSearchCommand>, IExecute<CutoffUnmetMoviesSearchCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IMovieCutoffService _movieCutoffService;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public MovieSearchService(IMovieService movieService,
                                   IMovieCutoffService movieCutoffService,
                                   ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   IQueueService queueService,
                                   Logger logger)
        {
            _movieService = movieService;
            _movieCutoffService = movieCutoffService;
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _queueService = queueService;
            _logger = logger;
        }

        public void Execute(MoviesSearchCommand message)
        {
            var downloadedCount = 0;
            foreach (var movieId in message.MovieIds)
            {
                var movie = _movieService.GetMovie(movieId);

                if (!movie.Monitored && message.Trigger != CommandTrigger.Manual)
                {
                    _logger.Debug("Movie {0} is not monitored, skipping search", movie.Title);
                    continue;
                }

                var decisions = _releaseSearchService.MovieSearch(movieId, false, false);
                downloadedCount += _processDownloadDecisions.ProcessDecisions(decisions).Grabbed.Count;
            }

            _logger.ProgressInfo("Movie search completed. {0} reports downloaded.", downloadedCount);
        }

        public void Execute(MissingMoviesSearchCommand message)
        {
            var pagingSpec = new PagingSpec<Movie>
            {
                Page = 1,
                PageSize = 100000,
                SortDirection = SortDirection.Ascending,
                SortKey = "Id"
            };

            pagingSpec.FilterExpressions.Add(v => v.Monitored == true);
            List<Movie> movies = _movieService.MoviesWithoutFiles(pagingSpec).Records.ToList();

            var queue = _queueService.GetQueue().Where(q => q.Movie != null).Select(q => q.Movie.Id);
            var missing = movies.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingMovies(missing, message.Trigger == CommandTrigger.Manual);
        }

        public void Execute(CutoffUnmetMoviesSearchCommand message)
        {
            var pagingSpec = new PagingSpec<Movie>
            {
                Page = 1,
                PageSize = 100000,
                SortDirection = SortDirection.Ascending,
                SortKey = "Id"
            };

            pagingSpec.FilterExpressions.Add(v => v.Monitored == true);

            List<Movie> movies = _movieCutoffService.MoviesWhereCutoffUnmet(pagingSpec).Records.ToList();

            var queue = _queueService.GetQueue().Where(q => q.Movie != null).Select(q => q.Movie.Id);
            var missing = movies.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingMovies(missing, message.Trigger == CommandTrigger.Manual);
        }

        private void SearchForMissingMovies(List<Movie> movies, bool userInvokedSearch)
        {
            _logger.ProgressInfo("Performing missing search for {0} movies", movies.Count);
            var downloadedCount = 0;

            foreach (var movieId in movies.GroupBy(e => e.Id))
            {
                List<DownloadDecision> decisions;

                try
                {
                    decisions = _releaseSearchService.MovieSearch(movieId.Key, userInvokedSearch, false);
                }
                catch (Exception ex)
                {
                    var message = string.Format("Unable to search for missing movie {0}", movieId.Key);
                    _logger.Error(ex, message);
                    continue;
                }

                var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed missing search for {0} movies. {1} reports downloaded.", movies.Count, downloadedCount);
        }
    }
}
