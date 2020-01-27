using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MetadataSource.PreDB
{
    public interface IPreDBService
    {
        bool HasReleases(Movie movie);
    }

    public class PreDBService : IPreDBService, IExecute<PreDBSyncCommand>
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMovieService _movieService;
        private readonly IHttpClient _httpClient;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public PreDBService(
                              IFetchAndParseRss rssFetcherAndParser,
                              IMakeDownloadDecision downloadDecisionMaker,
                              IProcessDownloadDecisions processDownloadDecisions,
                              IPendingReleaseService pendingReleaseService,
                              IEventAggregator eventAggregator,
                              IMovieService movieService,
                              IHttpClient httpClient,
                              IParsingService parsingService,
                              Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            _processDownloadDecisions = processDownloadDecisions;
            _pendingReleaseService = pendingReleaseService;
            _eventAggregator = eventAggregator;
            _movieService = movieService;
            _httpClient = httpClient;
            _parsingService = parsingService;
            _logger = logger;
        }

        private List<PreDBResult> GetResults(string category = "", string search = "")
        {
            return new List<PreDBResult>();

            /* PreDB is blocked
            var builder = new HttpRequestBuilder("http://predb.me").AddQueryParam("rss", "1");
            if (category.IsNotNullOrWhiteSpace())
            {
                builder.AddQueryParam("cats", category);
            }

            if (search.IsNotNullOrWhiteSpace())
            {
                builder.AddQueryParam("search", search);
            }

            var request = builder.Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.Warn("Non 200 StatusCode {0} encountered while searching PreDB.", response.StatusCode);
                return new List<PreDBResult>();
            }

            try
            {
                var reader = XmlReader.Create(new StringReader(response.Content));

                var items = SyndicationFeed.Load(reader);

                var results = new List<PreDBResult>();

                foreach (SyndicationItem item in items.Items)
                {
                    var result = new PreDBResult();
                    result.Title = item.Title.Text;
                    result.Link = item.Links[0].Uri.ToString();
                    results.Add(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while searching PreDB.");
            }

            return new List<PreDBResult>(); */
        }

        private List<Movie> FindMatchesToResults(List<PreDBResult> results)
        {
            var matches = new List<Movie>();

            foreach (PreDBResult result in results)
            {
                var parsedInfo = Parser.Parser.ParseMovieTitle(result.Title, true);

                if (parsedInfo != null)
                {
                    var movie = _movieService.FindByTitle(parsedInfo.MovieTitle, parsedInfo.Year);

                    if (movie != null)
                    {
                        matches.Add(movie);
                    }
                }
            }

            return matches;
        }

        private List<Movie> Sync()
        {
            _logger.ProgressInfo("Starting PreDB Sync");

            var results = GetResults("movies");

            var matches = FindMatchesToResults(results);

            return matches;
        }

        public void Execute(PreDBSyncCommand message)
        {
            var haveNewReleases = Sync();

            foreach (Movie movie in haveNewReleases)
            {
                if (!movie.HasPreDBEntry)
                {
                    movie.HasPreDBEntry = true;
                    _movieService.UpdateMovie(movie);
                }

                if (movie.Monitored)
                {
                    //Maybe auto search each movie once?
                }
            }

            _eventAggregator.PublishEvent(new PreDBSyncCompleteEvent(haveNewReleases));
        }

        public bool HasReleases(Movie movie)
        {
            try
            {
                var results = GetResults("movies", movie.Title);

                foreach (PreDBResult result in results)
                {
                    var parsed = Parser.Parser.ParseMovieTitle(result.Title, true);
                    if (parsed == null)
                    {
                        parsed = new Parser.Model.ParsedMovieInfo { MovieTitle = result.Title, Year = 0 };
                    }

                    var match = _parsingService.Map(parsed, "", new MovieSearchCriteria { Movie = movie });

                    if (match != null && match.RemoteMovie.Movie != null && match.RemoteMovie.Movie.Id == movie.Id)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error while looking on predb.me.");
                return false;
            }
        }
    }
}
