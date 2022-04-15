using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _qualityUpgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification qualityUpgradableSpecification,
                                        ICustomFormatCalculationService formatService,
                                        Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Movie.MovieFiles.Value.Count == 0)
            {
                yield return Decision.Accept();
                yield break;
            }

            var files = subject.Movie.MovieFiles.Value;

            foreach (var file in files)
            {
                file.Movie = subject.Movie;
                var customFormats = _formatService.ParseCustomFormat(file);

                foreach (var profile in subject.Movie.QualityProfiles.Value)
                {
                    yield return Calculate(profile, subject, file, customFormats);
                }
            }
        }

        private Decision Calculate(Profile profile, RemoteMovie subject, MovieFile file, List<CustomFormat> customFormats)
        {
            _logger.Debug("Comparing file quality with report for profile {2}. Existing file is {0} [{1}]", file.Quality, customFormats.ConcatToString(), profile.Name);

            // Check to see if the existing file is valid for this profile. if not, don't count against this release
            var qualityIndex = profile.GetIndex(file.Quality.Quality);
            var qualityOrGroup = profile.Items[qualityIndex.Index];

            if (!qualityOrGroup.Allowed)
            {
                return Decision.Accept();
            }

            if (!_qualityUpgradableSpecification.IsUpgradable(profile,
                                                              file.Quality,
                                                              customFormats,
                                                              subject.ParsedMovieInfo.Quality,
                                                              subject.CustomFormats))
            {
                // One file on disk is better for this profile than this release, skip to next profile
                return Decision.Reject("Quality for existing file(s) on disk is of equal or higher preference", profile.Id);
            }

            return Decision.Accept();
        }
    }
}
