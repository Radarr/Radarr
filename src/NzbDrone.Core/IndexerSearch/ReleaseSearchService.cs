using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        List<DownloadDecision> MovieSearch(int movieId, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> MovieSearch(Movie movie, bool userInvokedSearch, bool interactiveSearch);
    }

    public class ReleaseSearchService : ISearchForReleases
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly IMovieService _movieService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IProfileService _profileService;
        private readonly Logger _logger;

        public ReleaseSearchService(IIndexerFactory indexerFactory,
                                IMakeDownloadDecision makeDownloadDecision,
                                IMovieService movieService,
                                IMovieTranslationService movieTranslationService,
                                IProfileService profileService,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _makeDownloadDecision = makeDownloadDecision;
            _movieService = movieService;
            _movieTranslationService = movieTranslationService;
            _profileService = profileService;
            _logger = logger;
        }

        public List<DownloadDecision> MovieSearch(int movieId, bool userInvokedSearch, bool interactiveSearch)
        {
            var movie = _movieService.GetMovie(movieId);
            movie.MovieMetadata.Value.Translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId);

            return MovieSearch(movie, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> MovieSearch(Movie movie, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<MovieSearchCriteria>(movie, userInvokedSearch, interactiveSearch);

            var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
            downloadDecisions.AddRange(decisions);

            return DeDupeDecisions(downloadDecisions);
        }

        private TSpec Get<TSpec>(Movie movie, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec()
            {
                Movie = movie,
                UserInvokedSearch = userInvokedSearch,
                InteractiveSearch = interactiveSearch
            };

            var wantedLanguages = _profileService.GetAcceptableLanguages(movie.ProfileId);
            var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId);

            var queryTranlations = new List<string>
            {
                movie.MovieMetadata.Value.Title,
                movie.MovieMetadata.Value.OriginalTitle
            };

            //Add Translation of wanted languages to search query
            foreach (var translation in translations.Where(a => wantedLanguages.Contains(a.Language)))
            {
                queryTranlations.Add(translation.Title);
            }

            spec.SceneTitles = queryTranlations.Distinct().Where(t => t.IsNotNullOrWhiteSpace()).ToList();

            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReleaseInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Movie.Tags).Any()).ToList();

            var reports = new List<ReleaseInfo>();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

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

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections.
            return decisions.GroupBy(d => d.RemoteMovie.Release.Guid).Select(d => d.OrderBy(v => v.Rejections.Count()).First()).ToList();
        }
    }
}
