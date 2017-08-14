using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, IMediaFileService mediaFileService, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {

            foreach (var album in subject.Albums)
            {
                var trackFiles = _mediaFileService.GetFilesByAlbum(album.ArtistId, album.Id);

                if (trackFiles.Any())
                {
                    var lowestQuality = trackFiles.Select(c => c.Quality).OrderBy(c => c.Quality.Id).First();

                    if (!_qualityUpgradableSpecification.IsUpgradable(subject.Artist.Profile, lowestQuality, subject.ParsedAlbumInfo.Quality))
                    {
                        return Decision.Reject("Quality for existing file on disk is of equal or higher preference: {0}", lowestQuality);
                    }
                }

            }

            return Decision.Accept();
        }
    }
}
