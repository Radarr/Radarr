using System.Linq;
using NLog;
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
        private readonly IDelayProfileService _delayProfileService;
        private readonly Logger _logger;

        public DelaySpecification(IPendingReleaseService pendingReleaseService,
                                  IUpgradableSpecification qualityUpgradableSpecification,
                                  IDelayProfileService delayProfileService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
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

            // Preferred word count 
            var title = subject.Release.Title;
            var preferredWords = subject.Movie.Profile?.PreferredTags;
            var preferredCount = 0;

            if (preferredWords == null)
            {
                preferredCount = 1;
                _logger.Debug("Preferred words is null, setting preffered count to 1.");
            }
            else
            {
                preferredCount = preferredWords.AsEnumerable().Count(w => title.ToLower().Contains(w.ToLower()));
            }

            if (delay == 0)
            {
                _logger.Debug("Profile does not require a waiting period before download for {0}.", subject.Release.DownloadProtocol);
                return Decision.Accept();
            }

            var comparer = new QualityModelComparer(profile);

            if (isPreferredProtocol && (subject.Movie.MovieFileId != 0 && subject.Movie.MovieFile != null) && (preferredCount > 0 || preferredWords == null))
            {
                    var upgradable = _qualityUpgradableSpecification.IsUpgradable(profile, subject.Movie.MovieFile.Quality, subject.ParsedMovieInfo.Quality);

                    if (upgradable)
                    {
                        var revisionUpgrade = _qualityUpgradableSpecification.IsRevisionUpgrade(subject.Movie.MovieFile.Quality, subject.ParsedMovieInfo.Quality);

                        if (revisionUpgrade)
                        {
                            _logger.Debug("New quality is a better revision for existing quality and preferred word count is {0}, skipping delay", preferredCount);
                            return Decision.Accept();
                        }
                    }
                
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            var bestQualityInProfile = profile.LastAllowedQuality();
            var isBestInProfile = comparer.Compare(subject.ParsedMovieInfo.Quality.Quality, bestQualityInProfile) >= 0;

            if (isBestInProfile && isPreferredProtocol && (preferredCount > 0  || preferredWords == null))
            {
                _logger.Debug("Quality is highest in profile for preferred protocol and preferred word count is {0}, will not delay.", preferredCount);
                return Decision.Accept();
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
