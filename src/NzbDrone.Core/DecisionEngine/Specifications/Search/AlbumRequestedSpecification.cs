using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;


namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class AlbumRequestedSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public AlbumRequestedSpecification(Logger logger)
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

            var criteriaAlbum = searchCriteria.Albums.Select(v => v.Id).ToList();
            var remoteAlbums = remoteAlbum.Albums.Select(v => v.Id).ToList();

            if (!criteriaAlbum.Intersect(remoteAlbums).Any())
            {
                _logger.Debug("Release rejected since the album wasn't requested: {0}", remoteAlbum.ParsedAlbumInfo);
                return Decision.Reject("Album wasn't requested");
            }

            return Decision.Accept();
        }
    }
}
