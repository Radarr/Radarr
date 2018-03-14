using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class ProtocolSpecification : IDecisionEngineSpecification
    {
        private readonly IDelayProfileService _delayProfileService;
        private readonly Logger _logger;

        public ProtocolSpecification(IDelayProfileService delayProfileService,
                                     Logger logger)
        {
            _delayProfileService = delayProfileService;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var delayProfile = _delayProfileService.BestForTags(subject.Movie.Tags);

            if (subject.Release.DownloadProtocol == DownloadProtocol.Usenet && !delayProfile.EnableUsenet)
            {
                _logger.Debug("[{0}] Usenet is not enabled for this movie", subject.Release.Title);
                return Decision.Reject("Usenet is not enabled for this movie");
            }

            if (subject.Release.DownloadProtocol == DownloadProtocol.Torrent && !delayProfile.EnableTorrent)
            {
                _logger.Debug("[{0}] Torrent is not enabled for this movie", subject.Release.Title);
                return Decision.Reject("Torrent is not enabled for this movie");
            }

            return Decision.Accept();
        }
    }
}
