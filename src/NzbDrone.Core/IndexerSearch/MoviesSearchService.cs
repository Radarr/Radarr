using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class MovieSearchService : IExecute<MoviesSearchCommand>
    {
        private readonly IMovieService _seriesService;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public MovieSearchService(IMovieService seriesService,
                                   ISearchForNzb nzbSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _seriesService = seriesService;
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(MoviesSearchCommand message)
        {
            var series = _seriesService.GetMovie(message.MovieId);

            var downloadedCount = 0;

                if (!series.Monitored)
                {
                    _logger.Debug("Movie {0} is not monitored, skipping search", series.Title);
                }

            var decisions = _nzbSearchService.MovieSearch(message.MovieId, false);//_nzbSearchService.SeasonSearch(message.MovieId, season.SeasonNumber, false, message.Trigger == CommandTrigger.Manual);
                downloadedCount += _processDownloadDecisions.ProcessDecisions(decisions).Grabbed.Count;
            

            _logger.ProgressInfo("Movie search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
