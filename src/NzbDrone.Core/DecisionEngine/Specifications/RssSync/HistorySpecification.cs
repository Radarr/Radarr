using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IConfigService _configService;
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;

        public HistorySpecification(IHistoryService historyService,
                                           UpgradableSpecification qualityUpgradableSpecification,
                                           IConfigService configService,
                                           IPreferredWordService preferredWordServiceCalculator,
                                           Logger logger)
        {
            _historyService = historyService;
            _upgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return Decision.Accept();
            }

            var cdhEnabled = _configService.EnableCompletedDownloadHandling;

            _logger.Debug("Performing history status check on report");
            foreach (var album in subject.Albums)
            {
                _logger.Debug("Checking current status of album [{0}] in history", album.Id);
                var mostRecent = _historyService.MostRecentForAlbum(album.Id);

                if (mostRecent != null && mostRecent.EventType == HistoryEventType.Grabbed)
                {
                    var recent = mostRecent.Date.After(DateTime.UtcNow.AddHours(-12));

                    if (!recent && cdhEnabled)
                    {
                        continue;
                    }

                    // The artist will be the same as the one in history since it's the same album.
                    // Instead of fetching the artist from the DB reuse the known artist.
                    var preferredWordScore = _preferredWordServiceCalculator.Calculate(subject.Artist, mostRecent.SourceTitle);

                    var cutoffUnmet = _upgradableSpecification.CutoffNotMet(
                        subject.Artist.QualityProfile,
                        new List<QualityModel> { mostRecent.Quality },
                        preferredWordScore,
                        subject.ParsedAlbumInfo.Quality,
                        subject.PreferredWordScore);

                    var upgradeable = _upgradableSpecification.IsUpgradable(
                        subject.Artist.QualityProfile,
                        new List<QualityModel> { mostRecent.Quality },
                        preferredWordScore,
                        subject.ParsedAlbumInfo.Quality,
                        subject.PreferredWordScore);

                    if (!cutoffUnmet)
                    {
                        if (recent)
                        {
                            return Decision.Reject("Recent grab event in history already meets cutoff: {0}", mostRecent.Quality);
                        }

                        return Decision.Reject("CDH is disabled and grab event in history already meets cutoff: {0}", mostRecent.Quality);
                    }

                    if (!upgradeable)
                    {
                        if (recent)
                        {
                            return Decision.Reject("Recent grab event in history is of equal or higher quality: {0}", mostRecent.Quality);
                        }

                        return Decision.Reject("CDH is disabled and grab event in history is of equal or higher quality: {0}", mostRecent.Quality);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
