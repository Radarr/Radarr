using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DelaySpecification : IDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly Logger _logger;

        public DelaySpecification(IPendingReleaseService pendingReleaseService,
                                  IUpgradableSpecification qualityUpgradableSpecification,
                                  ICustomFormatCalculationService formatService,
                                  IDelayProfileService delayProfileService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _formatService = formatService;
            _delayProfileService = delayProfileService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null && searchCriteria.UserInvokedSearch)
            {
                _logger.Debug("Ignoring delay for user invoked search");
                return Decision.Accept();
            }

            var profile = subject.Movie.Profile;
            var delayProfile = _delayProfileService.BestForTags(subject.Movie.Tags);
            var delay = delayProfile.GetProtocolDelay(subject.Release.DownloadProtocol);
            var isPreferredProtocol = subject.Release.DownloadProtocol == delayProfile.PreferredProtocol;

            if (delay == 0)
            {
                _logger.Debug("Profile does not require a waiting period before download for {0}.", subject.Release.DownloadProtocol);
                return Decision.Accept();
            }

            var comparer = new QualityModelComparer(profile);

            var file = subject.Movie.MovieFile;

            if (isPreferredProtocol && (subject.Movie.MovieFileId != 0 && file != null))
            {
                var customFormats = _formatService.ParseCustomFormat(file);
                var upgradable = _qualityUpgradableSpecification.IsUpgradable(profile,
                                                                              file.Quality,
                                                                              customFormats,
                                                                              subject.ParsedMovieInfo.Quality,
                                                                              subject.CustomFormats);

                if (upgradable)
                {
                    var revisionUpgrade = _qualityUpgradableSpecification.IsRevisionUpgrade(subject.Movie.MovieFile.Quality, subject.ParsedMovieInfo.Quality);

                    if (revisionUpgrade)
                    {
                        _logger.Debug("New quality is a better revision for existing quality, skipping delay");
                        return Decision.Accept();
                    }
                }
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            if (delayProfile.BypassIfHighestQuality)
            {
                var bestQualityInProfile = profile.LastAllowedQuality();
                var isBestInProfile = comparer.Compare(subject.ParsedMovieInfo.Quality.Quality, bestQualityInProfile) >= 0;

                if (isBestInProfile && isPreferredProtocol)
                {
                    _logger.Debug("Quality is highest in profile for preferred protocol, will not delay.");
                    return Decision.Accept();
                }
            }

            var oldest = _pendingReleaseService.OldestPendingRelease(subject.Movie.Id);

            if (oldest != null && oldest.Release.AgeMinutes > delay)
            {
                return Decision.Accept();
            }

            if (subject.Release.AgeMinutes < delay)
            {
                _logger.Debug("Waiting for better quality release, There is a {0} minute delay on {1}", delay, subject.Release.DownloadProtocol);
                return Decision.Reject("Waiting for better quality release");
            }

            return Decision.Accept();
        }
    }
}
