using System.Linq;
using NLog;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DelaySpecification : IDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IDelayProfileService _delayProfileService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public DelaySpecification(IPendingReleaseService pendingReleaseService,
                                  IUpgradableSpecification qualityUpgradableSpecification,
                                  IDelayProfileService delayProfileService,
                                  IMediaFileService mediaFileService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _upgradableSpecification = qualityUpgradableSpecification;
            _delayProfileService = delayProfileService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null && searchCriteria.UserInvokedSearch)
            {
                _logger.Debug("Ignoring delay for user invoked search");
                return Decision.Accept();
            }

            var profile = subject.Artist.Profile.Value;
            var languageProfile = subject.Artist.LanguageProfile.Value;
            var delayProfile = _delayProfileService.BestForTags(subject.Artist.Tags);
            var delay = delayProfile.GetProtocolDelay(subject.Release.DownloadProtocol);
            var isPreferredProtocol = subject.Release.DownloadProtocol == delayProfile.PreferredProtocol;

            if (delay == 0)
            {
                _logger.Debug("Profile does not require a waiting period before download for {0}.", subject.Release.DownloadProtocol);
                return Decision.Accept();
            }

            var comparer = new QualityModelComparer(profile);
            var comparerLanguage = new LanguageComparer(languageProfile);

            if (isPreferredProtocol)
            {
                foreach (var album in subject.Albums)
                {
                    var trackFiles = _mediaFileService.GetFilesByAlbum(album.ArtistId, album.Id);

                    if (trackFiles.Any())
                    {
                        var lowestQuality = trackFiles.Select(c => c.Quality).OrderBy(c => c.Quality.Id).First();
                        var upgradable = _upgradableSpecification.IsUpgradable(profile,
                                                                               languageProfile,
                                                                               lowestQuality,
                                                                               trackFiles[0].Language,
                                                                               subject.ParsedAlbumInfo.Quality,
                                                                               subject.ParsedAlbumInfo.Language);
                        if (upgradable)
                        {
                            _logger.Debug("New quality is a better revision for existing quality, skipping delay");
                            return Decision.Accept();
                        }
                    }
                }
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            var bestQualityInProfile = profile.LastAllowedQuality();
            var isBestInProfile = comparer.Compare(subject.ParsedAlbumInfo.Quality.Quality, bestQualityInProfile) >= 0;
            var isBestInProfileLanguage = comparerLanguage.Compare(subject.ParsedAlbumInfo.Language, languageProfile.LastAllowedLanguage()) >= 0;

            if (isBestInProfile && isBestInProfileLanguage && isPreferredProtocol)
            {
                _logger.Debug("Quality and language is highest in profile for preferred protocol, will not delay");
                return Decision.Accept();
            }

            var albumIds = subject.Albums.Select(e => e.Id);

            var oldest = _pendingReleaseService.OldestPendingRelease(subject.Artist.Id, albumIds);

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
