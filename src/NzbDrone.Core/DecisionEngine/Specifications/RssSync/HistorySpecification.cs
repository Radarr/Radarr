using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDecisionEngineSpecification
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

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return new List<Decision> { Decision.Accept() };
            }

            return Calculate(subject, searchCriteria);
        }

        private IEnumerable<Decision> Calculate(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var cdhEnabled = _configService.EnableCompletedDownloadHandling;

            _logger.Debug("Performing history status check on report");
            _logger.Debug("Checking current status of movie [{0}] in history", subject.Movie.Id);
            var mostRecent = _historyService.MostRecentForMovie(subject.Movie.Id);

            foreach (var profile in subject.Movie.QualityProfiles.Value)
            {
                if (mostRecent != null && mostRecent.EventType == MovieHistoryEventType.Grabbed)
                {
                    var customFormats = _formatService.ParseCustomFormat(mostRecent);

                    var cutoffUnmet = _upgradableSpecification.CutoffNotMet(profile,
                                                                            mostRecent.Quality,
                                                                            customFormats,
                                                                            subject.ParsedMovieInfo.Quality);

                    var upgradeable = _upgradableSpecification.IsUpgradable(profile,
                                                                            mostRecent.Quality,
                                                                            customFormats,
                                                                            subject.ParsedMovieInfo.Quality,
                                                                            subject.CustomFormats);

                    var recent = mostRecent.Date.After(DateTime.UtcNow.AddHours(-12));
                    if (!recent && cdhEnabled)
                    {
                        yield return Decision.Accept();
                        continue;
                    }

                    if (!cutoffUnmet)
                    {
                        if (recent)
                        {
                            yield return Decision.Reject(string.Format("Recent grab event in history already meets cutoff: {0}", mostRecent.Quality), profile.Id);
                        }
                        else
                        {
                            yield return Decision.Reject(string.Format("CDH is disabled and grab event in history already meets cutoff: {0}", mostRecent.Quality), profile.Id);
                        }
                    }
                    else if (!upgradeable)
                    {
                        if (recent)
                        {
                            yield return Decision.Reject(string.Format("Recent grab event in history is of equal or higher quality: {0}", mostRecent.Quality), profile.Id);
                        }
                        else
                        {
                            yield return Decision.Reject(string.Format("CDH is disabled and grab event in history is of equal or higher quality: {0}", mostRecent.Quality), profile.Id);
                        }
                    }
                    else
                    {
                        yield return Decision.Accept();
                    }
                }
                else
                {
                    yield return Decision.Accept();
                }
            }
        }
    }
}
