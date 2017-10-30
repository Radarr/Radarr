using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SingleAlbumSearchMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SingleAlbumSearchMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum remoteAlbum, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            var singleAlbumSpec = searchCriteria as AlbumSearchCriteria;
            if (singleAlbumSpec == null)
            {
                return Decision.Accept();
            }
                
            if (Parser.Parser.CleanArtistName(singleAlbumSpec.AlbumTitle) != Parser.Parser.CleanArtistName(remoteAlbum.ParsedAlbumInfo.AlbumTitle))
            {
                _logger.Debug("Album does not match searched album title, skipping.");
                return Decision.Reject("Wrong album");
            }

            if (!remoteAlbum.ParsedAlbumInfo.AlbumTitle.Any())
            {
                _logger.Debug("Full discography result during single album search, skipping.");
                return Decision.Reject("Full artist pack");
            }

            return Decision.Accept();
        }
    }
}
