using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public CutoffSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger, IMediaFileService mediaFileService)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
            _mediaFileService = mediaFileService;
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

                    _logger.Debug("Comparing file quality with report. Existing file is {0}", lowestQuality);

                    if (!_qualityUpgradableSpecification.CutoffNotMet(subject.Artist.Profile, lowestQuality, subject.ParsedAlbumInfo.Quality))
                    {
                        _logger.Debug("Cutoff already met, rejecting.");
                        return Decision.Reject("Existing file meets cutoff: {0}", subject.Artist.Profile.Value.Cutoff);
                    }

                }
            }

            return Decision.Accept();
        }
    }
}
