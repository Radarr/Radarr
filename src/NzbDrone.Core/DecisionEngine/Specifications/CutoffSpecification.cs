using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public CutoffSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Movie.MovieFile != null)
            {
                if (!_qualityUpgradableSpecification.CutoffNotMet(subject.Movie.Profile, subject.Movie.MovieFile.Quality, subject.ParsedMovieInfo.Quality))
                {
                    return Decision.Reject("Existing file meets cutoff: {0}", subject.Movie.Profile.Value.Cutoff);
                }
            }

            return Decision.Accept();
        }
    }
}
