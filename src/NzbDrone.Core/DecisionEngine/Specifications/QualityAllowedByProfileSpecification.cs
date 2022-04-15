using System.Collections.Generic;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QualityAllowedByProfileSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public QualityAllowedByProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if report meets quality requirements. {0}", subject.ParsedMovieInfo.Quality);

            var profiles = subject.Movie.QualityProfiles.Value;

            foreach (var profile in profiles)
            {
                yield return Calculate(profile, subject);
            }
        }

        private Decision Calculate(Profile profile, RemoteMovie subject)
        {
            var qualityIndex = profile.GetIndex(subject.ParsedMovieInfo.Quality.Quality);
            var qualityOrGroup = profile.Items[qualityIndex.Index];

            if (!qualityOrGroup.Allowed)
            {
                _logger.Debug("Quality {0} rejected by Movie's quality profile: {1}", subject.ParsedMovieInfo.Quality, profile.Name);
                return Decision.Reject(string.Format("{0} is not wanted in profile", subject.ParsedMovieInfo.Quality.Quality), profile.Id);
            }

            return Decision.Accept();
        }
    }
}
