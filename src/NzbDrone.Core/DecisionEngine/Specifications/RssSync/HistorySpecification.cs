using System;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public HistorySpecification(IHistoryService historyService,
                                    UpgradableSpecification upgradableSpecification,
                                    ICustomFormatCalculationService formatService,
                                    IConfigService configService,
                                    Logger logger)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return DownloadSpecDecision.Accept();
            }

            var cdhEnabled = _configService.EnableCompletedDownloadHandling;
            var qualityProfile = subject.Movie.QualityProfile;

            _logger.Debug("Performing history status check on report");

            _logger.Debug("Checking current status of movie [{0}] in history", subject.Movie.Id);
            var mostRecent = _historyService.MostRecentForMovie(subject.Movie.Id);

            if (mostRecent == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (mostRecent.EventType == MovieHistoryEventType.Grabbed)
            {
                var recent = mostRecent.Date.After(DateTime.UtcNow.AddHours(-12));

                if (!recent && cdhEnabled)
                {
                    return DownloadSpecDecision.Accept();
                }

                var customFormats = _formatService.ParseCustomFormat(mostRecent, subject.Movie);

                var cutoffUnmet = _upgradableSpecification.CutoffNotMet(
                    subject.Movie.QualityProfile,
                    mostRecent.Quality,
                    customFormats,
                    subject.ParsedMovieInfo.Quality);

                var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(
                    subject.Movie.QualityProfile,
                    mostRecent.Quality,
                    customFormats,
                    subject.ParsedMovieInfo.Quality,
                    subject.CustomFormats);

                if (!cutoffUnmet)
                {
                    if (recent)
                    {
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryRecentCutoffMet, "Recent grab event in history already meets cutoff: {0}", mostRecent.Quality);
                    }

                    return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCdhDisabledCutoffMet, "CDH is disabled and grab event in history already meets cutoff: {0}", mostRecent.Quality);
                }

                var rejectionSubject = recent ? "Recent" : "CDH is disabled and";

                switch (upgradeableRejectReason)
                {
                    case UpgradeableRejectReason.None:
                        return DownloadSpecDecision.Accept();

                    case UpgradeableRejectReason.BetterQuality:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherPreference, "{0} grab event in history is of equal or higher preference: {1}", rejectionSubject, mostRecent.Quality);

                    case UpgradeableRejectReason.BetterRevision:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherRevision, "{0} grab event in history is of equal or higher revision: {1}", rejectionSubject, mostRecent.Quality.Revision);

                    case UpgradeableRejectReason.QualityCutoff:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCutoffMet, "{0} grab event in history meets quality cutoff: {1}", rejectionSubject, qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                    case UpgradeableRejectReason.CustomFormatCutoff:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatCutoffMet, "{0} grab event in history meets Custom Format cutoff: {1}", rejectionSubject, qualityProfile.CutoffFormatScore);

                    case UpgradeableRejectReason.CustomFormatScore:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScore, "{0} grab event in history has an equal or higher Custom Format score: {1}", rejectionSubject, qualityProfile.CalculateCustomFormatScore(customFormats));

                    case UpgradeableRejectReason.MinCustomFormatScore:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScoreIncrement, "{0} grab event in history has Custom Format score within Custom Format score increment: {1}", rejectionSubject, qualityProfile.MinUpgradeFormatScore);

                    case UpgradeableRejectReason.UpgradesNotAllowed:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryUpgradesNotAllowed, "{0} grab event in history and Quality Profile '{1}' does not allow upgrades", rejectionSubject, qualityProfile.Name);
                }
            }

            if (subject.Movie is { HasFile: true })
            {
                MovieHistory availableUsableMovieHistoryForCustomFormatScore;

                var movieHistoryEventTypeForHistoryComparison = new[]
                {
                    MovieHistoryEventType.Grabbed,
                    MovieHistoryEventType.MovieFolderImported
                };

                if (movieHistoryEventTypeForHistoryComparison.Contains(mostRecent.EventType))
                {
                    availableUsableMovieHistoryForCustomFormatScore = mostRecent;
                }
                else
                {
                    availableUsableMovieHistoryForCustomFormatScore = _historyService.MostRecentForMovieInEventCollection(
                        subject.Movie.Id,
                        new ReadOnlyCollection<MovieHistoryEventType>(movieHistoryEventTypeForHistoryComparison));
                }

                if (availableUsableMovieHistoryForCustomFormatScore == null)
                {
                    return DownloadSpecDecision.Accept();
                }

                var mostRecentCustomFormat = _formatService.ParseCustomFormat(availableUsableMovieHistoryForCustomFormatScore, subject.Movie);
                var mostRecentCustomFormatScore = qualityProfile.CalculateCustomFormatScore(mostRecentCustomFormat);

                if (mostRecentCustomFormatScore > subject.CustomFormatScore)
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScore, "Quality Profile '{0}' has a higher Custom Format score than the report: {1}", qualityProfile.Name, subject.CustomFormatScore);
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
