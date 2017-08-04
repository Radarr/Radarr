using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    class AlbumSearchService : IExecute<AlbumSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public AlbumSearchService(ISearchForNzb nzbSearchService,
            IProcessDownloadDecisions processDownloadDecisions,
            Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(AlbumSearchCommand message)
        {
            foreach (var albumId in message.AlbumIds)
            {
                var decisions =
                    _nzbSearchService.AlbumSearch(albumId, false, message.Trigger == CommandTrigger.Manual);
                var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                _logger.ProgressInfo("Album search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }
    }
}
