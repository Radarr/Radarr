using System;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class DiscographySpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public DiscographySpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedAlbumInfo.Discography)
            {
                _logger.Debug("Checking if all albums in discography release have released. {0}", subject.Release.Title);

                if (subject.Albums.Any(e => !e.ReleaseDate.HasValue || e.ReleaseDate.Value.After(DateTime.UtcNow)))
                {
                    _logger.Debug("Discography release {0} rejected. All albums haven't released yet.", subject.Release.Title);
                    return Decision.Reject("Discography release rejected. All albums haven't released yet.");
                }
            }

            return Decision.Accept();
        }
    }
}
