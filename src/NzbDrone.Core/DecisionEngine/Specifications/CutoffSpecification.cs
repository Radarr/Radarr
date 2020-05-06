using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;
        private readonly IPreferredWordService _preferredWordServiceCalculator;

        public CutoffSpecification(UpgradableSpecification upgradableSpecification,
                                   IPreferredWordService preferredWordServiceCalculator,
                                   Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var qualityProfile = subject.Artist.QualityProfile.Value;

            foreach (var file in subject.Albums.SelectMany(b => b.BookFiles.Value))
            {
                // Get a distinct list of all current track qualities for a given album
                var currentQualities = new List<QualityModel> { file.Quality };

                _logger.Debug("Comparing file quality with report. Existing files contain {0}", currentQualities.ConcatToString());

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                                                           currentQualities,
                                                           _preferredWordServiceCalculator.Calculate(subject.Artist, file.GetSceneOrFileName()),
                                                           subject.ParsedAlbumInfo.Quality,
                                                           subject.PreferredWordScore))
                {
                    _logger.Debug("Cutoff already met by existing files, rejecting.");

                    var qualityCutoffIndex = qualityProfile.GetIndex(qualityProfile.Cutoff);
                    var qualityCutoff = qualityProfile.Items[qualityCutoffIndex.Index];

                    return Decision.Reject("Existing files meets cutoff: {0}", qualityCutoff);
                }
            }

            return Decision.Accept();
        }
    }
}
