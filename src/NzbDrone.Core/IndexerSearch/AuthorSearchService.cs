using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class AuthorSearchService : IExecute<AuthorSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public AuthorSearchService(ISearchForNzb nzbSearchService,
            IProcessDownloadDecisions processDownloadDecisions,
            Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(AuthorSearchCommand message)
        {
            var decisions = _nzbSearchService.AuthorSearch(message.AuthorId, false, message.Trigger == CommandTrigger.Manual, false);
            var processed = _processDownloadDecisions.ProcessDecisions(decisions);

            _logger.ProgressInfo("Author search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }
    }
}
