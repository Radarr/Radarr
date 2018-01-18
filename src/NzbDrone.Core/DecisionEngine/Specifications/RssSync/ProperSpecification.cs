using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class ProperSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public ProperSpecification(UpgradableSpecification qualityUpgradableSpecification, IConfigService configService, IMediaFileService mediaFileService, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                return Decision.Accept();
            }

            foreach (var album in subject.Albums)
            {
                var trackFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                if (trackFiles.Any())
                {
                    var lowestQuality = trackFiles.Select(c => c.Quality).OrderBy(c => c.Quality.Id).First();
                    var dateAdded = trackFiles[0].DateAdded;

                    _logger.Debug("Comparing file quality with report. Existing file is {0}", lowestQuality);

                    if (_qualityUpgradableSpecification.IsRevisionUpgrade(lowestQuality, subject.ParsedAlbumInfo.Quality))
                    {
                        if (dateAdded < DateTime.Today.AddDays(-7))
                        {
                            _logger.Debug("Proper for old file, rejecting: {0}", subject);
                            return Decision.Reject("Proper for old file");
                        }

                        if (!_configService.AutoDownloadPropers)
                        {
                            _logger.Debug("Auto downloading of propers is disabled");
                            return Decision.Reject("Proper downloading is disabled");
                        }
                    }

                }
            }

            return Decision.Accept();
        }
    }
}
