using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Movies;
using System.Linq;
using NzbDrone.Common.TPL;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForNzb
    {
        List<DownloadDecision> MovieSearch(int movieId, bool userInvokedSearch);
        List<DownloadDecision> MovieSearch(Movie movie, bool userInvokedSearch);
    }

    public class NzbSearchService : ISearchForNzb
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public NzbSearchService(IIndexerFactory indexerFactory,
                                IMakeDownloadDecision makeDownloadDecision,
                                IMovieService movieService,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _makeDownloadDecision = makeDownloadDecision;
            _movieService = movieService;
            _logger = logger;
        }

        public List<DownloadDecision> MovieSearch(int movieId, bool userInvokedSearch)
        {
            var movie = _movieService.GetMovie(movieId);

            return MovieSearch(movie, userInvokedSearch);
        }

        public List<DownloadDecision> MovieSearch(Movie movie, bool userInvokedSearch)
        {
            var searchSpec = Get<MovieSearchCriteria>(movie, userInvokedSearch);

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private TSpec Get<TSpec>(Movie movie, bool userInvokedSearch) where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec()
            {
                Movie = movie,

                UserInvokedSearch = userInvokedSearch
            };
            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReleaseInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = _indexerFactory.SearchEnabled();
            var reports = new List<ReleaseInfo>();

            _logger.ProgressInfo("Searching {0} indexers for {1}", indexers.Count, criteriaBase);

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var indexer in indexers)
            {
                var indexerLocal = indexer;

                taskList.Add(taskFactory.StartNew(() =>
                {
                    try
                    {
                        var indexerReports = searchAction(indexerLocal);

                        lock (reports)
                        {
                            reports.AddRange(indexerReports);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error while searching for {0}", criteriaBase);
                    }
                }).LogExceptions());
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }
    }
}
