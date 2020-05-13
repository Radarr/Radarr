using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

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

        public virtual Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                return Decision.Accept();
            }

            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                _logger.Debug("Propers are not preferred, skipping check");
                return Decision.Accept();
            }

            foreach (var book in subject.Books)
            {
                var bookFiles = _mediaFileService.GetFilesByBook(book.Id);

                foreach (var file in bookFiles)
                {
                    if (_qualityUpgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedBookInfo.Quality))
                    {
                        if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotUpgrade)
                        {
                            _logger.Debug("Auto downloading of propers is disabled");
                            return Decision.Reject("Proper downloading is disabled");
                        }

                        if (file.DateAdded < DateTime.Today.AddDays(-7))
                        {
                            _logger.Debug("Proper for old file, rejecting: {0}", subject);
                            return Decision.Reject("Proper for old file");
                        }
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
