using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CustomFormatAllowedbyProfileSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public CustomFormatAllowedbyProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var formats = subject.ParsedMovieInfo.Quality.CustomFormats.WithNone();
            _logger.Debug("Checking if report meets custom format requirements. {0}", formats.ToExtendedString());
            var notAllowedFormats = subject.Movie.Profile.FormatItems.Where(v => v.Allowed == false).Select(f => f.Format).ToList();
            var notWantedFormats = notAllowedFormats.Intersect(formats);
            if (notWantedFormats.Any())
            {
                _logger.Debug("Custom Formats {0} rejected by Movie's profile", notWantedFormats.ToExtendedString());
                return Decision.Reject("Custom Formats {0} not wanted in profile", notWantedFormats.ToExtendedString());
            }

            return Decision.Accept();
        }
    }
}
