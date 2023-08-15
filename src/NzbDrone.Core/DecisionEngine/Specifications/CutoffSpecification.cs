using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public CutoffSpecification(IUpgradableSpecification upgradableSpecification,
                                   ICustomFormatCalculationService formatService,
                                   Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var profile = subject.Movie.QualityProfile;
            var file = subject.Movie.MovieFile;

            if (file != null)
            {
                file.Movie = subject.Movie;
                _logger.Debug("Comparing file quality with report. Existing file is {0}", file.Quality);

                var customFormats = _formatService.ParseCustomFormat(file);

                if (!_upgradableSpecification.CutoffNotMet(profile,
                                                           file.Quality,
                                                           customFormats,
                                                           subject.ParsedMovieInfo.Quality))
                {
                    _logger.Debug("Existing custom formats {0} meet cutoff",
                                  customFormats.ConcatToString());

                    var qualityCutoffIndex = profile.GetIndex(profile.Cutoff);
                    var qualityCutoff = profile.Items[qualityCutoffIndex.Index];

                    return Decision.Reject("Existing file meets cutoff: {0}", qualityCutoff);
                }
            }

            return Decision.Accept();
        }
    }
}
