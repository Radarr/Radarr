using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class ArtistSearchService : IExecute<ArtistSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public ArtistSearchService(ISearchForNzb nzbSearchService,
            IProcessDownloadDecisions processDownloadDecisions,
            Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(ArtistSearchCommand message)
        {
            var decisions = _nzbSearchService.ArtistSearch(message.ArtistId, false, message.Trigger == CommandTrigger.Manual, false);
            var processed = _processDownloadDecisions.ProcessDecisions(decisions);

            _logger.ProgressInfo("Artist search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }
    }
}
