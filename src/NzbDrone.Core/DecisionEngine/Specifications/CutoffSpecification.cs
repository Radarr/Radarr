using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public CutoffSpecification(IUpgradableSpecification upgradableSpecification,
                                   ICustomFormatCalculationService formatService,
                                   Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var files = subject.Movie.MovieFiles.Value;

            if (files.Count == 0)
            {
                return new List<Decision> { Decision.Accept() };
            }

            return CalculateProfileRejections(subject, searchCriteria);
        }

        private IEnumerable<Decision> CalculateProfileRejections(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var profiles = subject.Movie.QualityProfiles.Value;
            var files = subject.Movie.MovieFiles.Value;

            files.ForEach(f => f.Movie = subject.Movie);

            // Must check to ensure each profile has a file that meets cutoff in order to reject
            foreach (var file in files)
            {
                var customFormats = _formatService.ParseCustomFormat(file);

                foreach (var profile in profiles)
                {
                    // Check to see if the existing file is valid for this profile. if not, don't count against this release
                    var qualityIndex = profile.GetIndex(file.Quality.Quality);
                    var qualityOrGroup = profile.Items[qualityIndex.Index];

                    if (!qualityOrGroup.Allowed)
                    {
                        continue;
                    }

                    _logger.Debug("Comparing file quality with report. Existing file is {0}", file.Quality);

                    if (!_upgradableSpecification.CutoffNotMet(profile,
                                                                file.Quality,
                                                                customFormats,
                                                                subject.ParsedMovieInfo.Quality))
                    {
                        //Record rejection for profile and go to next profile.
                        yield return Decision.Reject("Existing file meets cutoff for profile", profile.Id);
                        break;
                    }
                }
            }
        }
    }
}
