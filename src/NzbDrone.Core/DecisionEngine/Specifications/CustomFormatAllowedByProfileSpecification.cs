using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    //TODO add tests for this!
    public class CustomFormatAllowedbyProfileSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public CustomFormatAllowedbyProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            //TODO make this work for None as well!
            _logger.Debug("Checking if report meets custom format requirements. {0}", subject.ParsedMovieInfo.Quality.CustomFormats.ToExtendedString());
            var notAllowedFormats = subject.Movie.Profile.Value.FormatItems.Where(v => v.Allowed == false).Select(f => f.Format).ToList();
            var notWantedFormats = notAllowedFormats.Intersect(subject.ParsedMovieInfo.Quality.CustomFormats);
            if (notWantedFormats.Any())
            {
                _logger.Debug("Custom Formats {0} rejected by Movie's profile", notWantedFormats.ToExtendedString());
                return Decision.Reject("Custom Formats {0} not wanted in profile", notWantedFormats.ToExtendedString());
            }

            return Decision.Accept();
        }
    }
}
