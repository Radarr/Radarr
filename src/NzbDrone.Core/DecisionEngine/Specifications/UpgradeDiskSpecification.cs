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

                    if (!_upgradableSpecification.IsUpgradable(subject.Author.QualityProfile,
                                                               currentQualities,
                                                               _preferredWordServiceCalculator.Calculate(subject.Author, file.GetSceneOrFileName(), subject.Release?.IndexerId ?? 0),
                                                               subject.ParsedBookInfo.Quality,
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
