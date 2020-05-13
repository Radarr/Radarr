using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification qualityUpgradableSpecification,
                                        IPreferredWordService preferredWordServiceCalculator,
                                        Logger logger)
        {
            _upgradableSpecification = qualityUpgradableSpecification;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Books.SelectMany(c => c.BookFiles.Value))
            {
                if (file == null)
                {
                    _logger.Debug("File is no longer available, skipping this file.");
                    continue;
                }

                _logger.Debug("Comparing file quality and language with report. Existing file is {0}", file.Quality);

                if (!_upgradableSpecification.IsUpgradable(subject.Author.QualityProfile,
                                                           new List<QualityModel> { file.Quality },
                                                           _preferredWordServiceCalculator.Calculate(subject.Author, file.GetSceneOrFileName()),
                                                           subject.ParsedBookInfo.Quality,
                                                           subject.PreferredWordScore))
                {
                    return Decision.Reject("Existing file on disk is of equal or higher preference: {0}", file.Quality);
                }
            }

            return Decision.Accept();
        }
    }
}
