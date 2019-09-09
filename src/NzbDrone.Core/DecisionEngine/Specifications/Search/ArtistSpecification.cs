using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class ArtistSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public ArtistSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteAlbum remoteAlbum, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            _logger.Debug("Checking if artist matches searched artist");

            if (remoteAlbum.Artist.Id != searchCriteria.Artist.Id)
            {
                _logger.Debug("Artist {0} does not match {1}", remoteAlbum.Artist, searchCriteria.Artist);
                return Decision.Reject("Wrong artist");
            }

            return Decision.Accept();
        }
    }
}
