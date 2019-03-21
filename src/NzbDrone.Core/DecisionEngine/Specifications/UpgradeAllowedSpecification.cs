using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeAllowedSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IMediaFileService _mediaFileService;
        private readonly ITrackService _trackService;
        private readonly Logger _logger;
        private readonly ICached<bool> _missingFilesCache;

        public UpgradeAllowedSpecification(UpgradableSpecification upgradableSpecification,
                                           Logger logger,
                                           ICacheManager cacheManager,
                                           IMediaFileService mediaFileService,
                                           ITrackService trackService)
        {
            _upgradableSpecification = upgradableSpecification;
            _mediaFileService = mediaFileService;
            _trackService = trackService;
            _missingFilesCache = cacheManager.GetCache<bool>(GetType());
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var qualityProfile = subject.Artist.QualityProfile.Value;
            var languageProfile = subject.Artist.LanguageProfile.Value;

            foreach (var album in subject.Albums)
            {
                var tracksMissing = _missingFilesCache.Get(album.Id.ToString(), () => _trackService.TracksWithoutFiles(album.Id).Any(),
                                                           TimeSpan.FromSeconds(30));

                var trackFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                if (!tracksMissing && trackFiles.Any())
                {
                    // Get a distinct list of all current track qualities and languages for a given album
                    var currentQualities = trackFiles.Select(c => c.Quality).Distinct().ToList();
                    var currentLanguages = trackFiles.Select(c => c.Language).Distinct().ToList();

                    _logger.Debug("Comparing file quality and language with report. Existing files contain {0} : {1}", currentQualities.ConcatToString(), currentLanguages.ConcatToString());

                    if (!_upgradableSpecification.IsUpgradeAllowed(qualityProfile,
                                                               languageProfile,
                                                               currentQualities,
                                                               currentLanguages,
                                                               subject.ParsedAlbumInfo.Quality,
                                                               subject.ParsedAlbumInfo.Language))
                    {
                        _logger.Debug("Upgrading is not allowed by the quality or language profile");

                        return Decision.Reject("Existing files and the Quality or Language profile does not allow upgrades");
                    }

                }
            }

            return Decision.Accept();
        }
    }
}
