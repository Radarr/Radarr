using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeAllowedSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeAllowedSpecification(UpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatService,
                                           Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var files = subject.Movie.MovieFiles.Value;

            foreach (var file in files)
            {
                if (file == null)
                {
                    _logger.Debug("File is no longer available, skipping this file.");
                    continue;
                }

                file.Movie = subject.Movie;
                var customFormats = _formatService.ParseCustomFormat(file);

                foreach (var qualityProfile in subject.Movie.QualityProfiles.Value)
                {
                    // Check to see if the existing file is valid for this profile. if not, don't count against this release
                    var qualityIndex = qualityProfile.GetIndex(file.Quality.Quality);
                    var qualityOrGroup = qualityProfile.Items[qualityIndex.Index];

                    if (!qualityOrGroup.Allowed)
                    {
                        continue;
                    }

                    _logger.Debug("Comparing file quality with report. Existing file is {0} [{1}]", file.Quality, customFormats.ConcatToString());

                    if (!_upgradableSpecification.IsUpgradeAllowed(qualityProfile,
                                                                   file.Quality,
                                                                   customFormats,
                                                                   subject.ParsedMovieInfo.Quality,
                                                                   subject.CustomFormats))
                    {
                        _logger.Debug("Upgrading is not allowed by the quality profile");

                        yield return Decision.Reject("Existing file and the Quality profile does not allow upgrades", qualityProfile.Id);
                        break;
                    }
                }
            }
        }
    }
}
