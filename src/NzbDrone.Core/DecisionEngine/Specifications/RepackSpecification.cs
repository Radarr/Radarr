using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RepackSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public RepackSpecification(UpgradableSpecification upgradableSpecification, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (!subject.ParsedMovieInfo.Quality.Revision.IsRepack)
            {
                return Decision.Accept();
            }

            if (subject.Movie.MovieFileId != 0)
            {
                var file = subject.Movie.MovieFile;

                if (_upgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedMovieInfo.Quality))
                {
                    var releaseGroup = subject.ParsedMovieInfo.ReleaseGroup;
                    var fileReleaseGroup = file.ReleaseGroup;

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
                            releaseGroup, fileReleaseGroup);
                        return Decision.Reject(
                            "Release is a repack for a different release group. Release Group: {0}. File release group: {1}",
                            releaseGroup, fileReleaseGroup);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
