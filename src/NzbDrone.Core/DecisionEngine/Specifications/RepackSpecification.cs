using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RepackSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public RepackSpecification(UpgradableSpecification upgradableSpecification, IConfigService configService, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (!subject.ParsedMovieInfo.Quality.Revision.IsRepack)
            {
                return Decision.Accept();
            }

            if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                _logger.Debug("Repacks are not preferred, skipping check");
                return Decision.Accept();
            }

            if (subject.Movie.MovieFileId != 0)
            {
                var file = subject.Movie.MovieFile;

                if (_upgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedMovieInfo.Quality))
                {
                    var releaseGroup = subject.ParsedMovieInfo.ReleaseGroup;
                    var fileReleaseGroup = file.ReleaseGroup;

                    if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotUpgrade)
                    {
                        _logger.Debug("Auto downloading of repacks is disabled");
                        return Decision.Reject("Repack downloading is disabled");
                    }

                    if (fileReleaseGroup.IsNullOrWhiteSpace())
                    {
                        return Decision.Reject("Unable to determine release group for the existing file");
                    }

                    if (releaseGroup.IsNullOrWhiteSpace())
                    {
                        return Decision.Reject("Unable to determine release group for this release");
                    }

                    if (!fileReleaseGroup.Equals(releaseGroup, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Debug(
                            "Release is a repack for a different release group. Release Group: {0}. File release group: {1}",
                            releaseGroup,
                            fileReleaseGroup);
                        return Decision.Reject(
                            "Release is a repack for a different release group. Release Group: {0}. File release group: {1}",
                            releaseGroup,
                            fileReleaseGroup);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
