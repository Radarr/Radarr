using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _qualityUpgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification qualityUpgradableSpecification,
                                        ICustomFormatCalculationService formatService,
                                        Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Movie.MovieFile == null)
            {
                return Decision.Accept();
            }

            var profile = subject.Movie.QualityProfile;
            var file = subject.Movie.MovieFile;
            file.Movie = subject.Movie;
            var customFormats = _formatService.ParseCustomFormat(file);
            _logger.Debug("Comparing file quality with report. Existing file is {0} [{1}]", file.Quality, customFormats.ConcatToString());

            if (!_qualityUpgradableSpecification.IsUpgradable(profile,
                                                              file.Quality,
                                                              customFormats,
                                                              subject.ParsedMovieInfo.Quality,
                                                              subject.CustomFormats))
            {
                return Decision.Reject("Quality for existing file on disk is of equal or higher preference: {0} [{1}]", file.Quality, customFormats.ConcatToString());
            }

            return Decision.Accept();
        }
    }
}
