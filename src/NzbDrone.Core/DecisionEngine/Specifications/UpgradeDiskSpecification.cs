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
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ITrackService _trackService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;
        private readonly ICached<bool> _missingFilesCache;
        
        public UpgradeDiskSpecification(UpgradableSpecification qualityUpgradableSpecification,
                                        IMediaFileService mediaFileService,
                                        ITrackService trackService,
                                        ICacheManager cacheManager,
                                        IPreferredWordService preferredWordServiceCalculator,
                                        Logger logger)
        {
            _upgradableSpecification = qualityUpgradableSpecification;
            _mediaFileService = mediaFileService;
            _trackService = trackService;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
            _logger = logger;
            _missingFilesCache = cacheManager.GetCache<bool>(GetType());
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {

            foreach (var album in subject.Albums)
            {
                var tracksMissing = _missingFilesCache.Get(album.Id.ToString(), () => _trackService.TracksWithoutFiles(album.Id).Any(),
                                                           TimeSpan.FromSeconds(30));
                var trackFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                if (!tracksMissing && trackFiles.Any())
                {
                    var currentQualities = trackFiles.Select(c => c.Quality).Distinct().ToList();

                    if (!_upgradableSpecification.IsUpgradable(subject.Artist.QualityProfile,
                                                               currentQualities,
                                                               _preferredWordServiceCalculator.Calculate(subject.Artist, trackFiles[0].GetSceneOrFileName()),
                                                               subject.ParsedAlbumInfo.Quality,
                                                               subject.PreferredWordScore))
                    {
                        return Decision.Reject("Existing files on disk is of equal or higher preference: {0}", currentQualities.ConcatToString());
                    }
                }

            }

            return Decision.Accept();
        }
    }
}
