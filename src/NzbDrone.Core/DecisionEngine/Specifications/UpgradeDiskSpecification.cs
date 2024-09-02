using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification upgradableSpecification,
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
            var qualityProfile = subject.Movie.QualityProfile;

            var file = subject.Movie.MovieFile;

            if (file == null)
            {
                _logger.Debug("File is no longer available, skipping this file.");
                return Decision.Accept();
            }

            file.Movie = subject.Movie;
            var customFormats = _formatService.ParseCustomFormat(file);

            _logger.Debug("Comparing file quality with report. Existing file is {0} [{1}].", file.Quality, customFormats.ConcatToString());

            if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                    file.Quality,
                    _formatService.ParseCustomFormat(file),
                    subject.ParsedMovieInfo.Quality))
            {
                _logger.Debug("Cutoff already met, rejecting.");

                var qualityCutoffIndex = qualityProfile.GetIndex(qualityProfile.Cutoff);
                var qualityCutoff = qualityProfile.Items[qualityCutoffIndex.Index];

                return Decision.Reject("Existing file meets cutoff: {0} [{1}]", qualityCutoff, customFormats.ConcatToString());
            }

            var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(qualityProfile,
                file.Quality,
                customFormats,
                subject.ParsedMovieInfo.Quality,
                subject.CustomFormats);

            switch (upgradeableRejectReason)
            {
                case UpgradeableRejectReason.None:
                    return Decision.Accept();
                case UpgradeableRejectReason.BetterQuality:
                    return Decision.Reject("Existing file on disk is of equal or higher preference: {0}", file.Quality);

                case UpgradeableRejectReason.BetterRevision:
                    return Decision.Reject("Existing file on disk is of equal or higher revision: {0}", file.Quality.Revision);

                case UpgradeableRejectReason.QualityCutoff:
                    return Decision.Reject("Existing file on disk meets quality cutoff: {0}", qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                case UpgradeableRejectReason.CustomFormatCutoff:
                    return Decision.Reject("Existing file on disk meets Custom Format cutoff: {0}", qualityProfile.CutoffFormatScore);

                case UpgradeableRejectReason.CustomFormatScore:
                    return Decision.Reject("Existing file on disk has a equal or higher custom format score: {0}", qualityProfile.CalculateCustomFormatScore(customFormats));
            }

            return Decision.Accept();
        }
    }
}
