using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false);
        List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly IConfigService _configService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IRemoteMovieAggregationService _aggregationService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IDecisionEngineSpecification> specifications,
                                     IParsingService parsingService,
                                     IConfigService configService,
                                     ICustomFormatCalculationService formatCalculator,
                                     IRemoteMovieAggregationService aggregationService,
                                     Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _configService = configService;
            _formatCalculator = formatCalculator;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        public List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false)
        {
            return GetDecisions(reports, pushedRelease).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase)
        {
            return GetDecisions(reports, false, searchCriteriaBase).ToList();
        }

        private IEnumerable<DownloadDecision> GetDecisions(List<ReleaseInfo> reports, bool pushedRelease = false, SearchCriteriaBase searchCriteria = null)
        {
            if (reports.Any())
            {
                _logger.ProgressInfo("Processing {0} releases", reports.Count);
            }
            else
            {
                _logger.ProgressInfo("No results found");
            }

            var reportNumber = 1;

            foreach (var report in reports)
            {
                var decisions = new List<DownloadDecision>();
                _logger.ProgressTrace("Processing release {0}/{1}", reportNumber, reports.Count);
                _logger.Debug("Processing release '{0}' from '{1}'", report.Title, report.Indexer);

                try
                {
                    var parsedMovieInfo = Parser.Parser.ParseMovieTitle(report.Title);

                    if (parsedMovieInfo != null && !parsedMovieInfo.PrimaryMovieTitle.IsNullOrWhiteSpace())
                    {
                        var remoteMovie = _parsingService.Map(parsedMovieInfo, report.ImdbId.ToString(), report.TmdbId, searchCriteria);
                        remoteMovie.Release = report;

                        if (remoteMovie.Movie == null)
                        {
                            var reason = "Unknown Movie";

                            decisions.Add(new DownloadDecision(remoteMovie, 0, new Rejection(reason)));
                        }
                        else
                        {
                            _aggregationService.Augment(remoteMovie);

                            remoteMovie.CustomFormats = _formatCalculator.ParseCustomFormat(remoteMovie, remoteMovie.Release.Size);
                            remoteMovie.CustomFormatScore = remoteMovie?.Movie?.Profile?.CalculateCustomFormatScore(remoteMovie.CustomFormats) ?? 0;

                            remoteMovie.DownloadAllowed = remoteMovie.Movie != null;
                            decisions.AddRange(GetDecisionForReport(remoteMovie, searchCriteria));
                        }
                    }

                    if (searchCriteria != null)
                    {
                        if (parsedMovieInfo == null)
                        {
                            parsedMovieInfo = new ParsedMovieInfo
                            {
                                Languages = LanguageParser.ParseLanguages(report.Title),
                                Quality = QualityParser.ParseQuality(report.Title)
                            };
                        }

                        if (parsedMovieInfo.PrimaryMovieTitle.IsNullOrWhiteSpace())
                        {
                            var remoteMovie = new RemoteMovie
                            {
                                Release = report,
                                ParsedMovieInfo = parsedMovieInfo,
                                Languages = parsedMovieInfo.Languages
                            };

                            decisions.Add(new DownloadDecision(remoteMovie, 0, new Rejection("Unable to parse release")));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteMovie = new RemoteMovie { Release = report };
                    decisions.Add(new DownloadDecision(remoteMovie, 0, new Rejection("Unexpected error processing release")));
                }

                reportNumber++;

                foreach (var decision in decisions)
                {
                    var source = pushedRelease ? ReleaseSourceType.ReleasePush : ReleaseSourceType.Rss;

                    if (searchCriteria != null)
                    {
                        if (searchCriteria.InteractiveSearch)
                        {
                            source = ReleaseSourceType.InteractiveSearch;
                        }
                        else if (searchCriteria.UserInvokedSearch)
                        {
                            source = ReleaseSourceType.UserInvokedSearch;
                        }
                        else
                        {
                            source = ReleaseSourceType.Search;
                        }
                    }

                    decision.RemoteMovie.ReleaseSource = source;

                    if (decision.Rejections.Any())
                    {
                        _logger.Debug("Release '{0}' from '{1}' rejected for the following reasons: {2}", report.Title, report.Indexer, string.Join(", ", decision.Rejections));
                    }
                    else
                    {
                        _logger.Debug("Release '{0}' from '{1}' accepted", report.Title, report.Indexer);
                    }

                    yield return decision;
                }
            }
        }

        private List<DownloadDecision> GetDecisionForReport(RemoteMovie remoteMovie, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = _specifications.SelectMany(c => EvaluateSpec(c, remoteMovie, searchCriteria))
                                         .Where(c => c != null);

            var decisions = new List<DownloadDecision>();

            if (remoteMovie.Movie == null)
            {
                return new List<DownloadDecision> { new DownloadDecision(remoteMovie, 0, reasons.ToArray()) };
            }

            foreach (var profile in remoteMovie.Movie.QualityProfiles.Value)
            {
                decisions.Add(new DownloadDecision(remoteMovie, profile.Id, reasons.Where(x => x.ProfileId == profile.Id || x.ProfileId == 0).ToArray()));
            }

            return decisions;
        }

        private List<Rejection> EvaluateSpec(IDecisionEngineSpecification spec, RemoteMovie remoteMovie, SearchCriteriaBase searchCriteriaBase = null)
        {
            var rejections = new List<Rejection>();

            try
            {
                var results = spec.IsSatisfiedBy(remoteMovie, searchCriteriaBase);

                foreach (var result in results.Where(c => !c.Accepted))
                {
                    rejections.Add(new Rejection(result.Reason, result.ProfileId, spec.Type));
                }
            }
            catch (NotImplementedException)
            {
                _logger.Trace("Spec " + spec.GetType().Name + " does not care about movies.");
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteMovie.Release.ToJson());
                e.Data.Add("parsed", remoteMovie.ParsedMovieInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}, with spec: {1}", remoteMovie.Release.Title, spec.GetType().Name);
                rejections.Add(new Rejection($"{spec.GetType().Name}: {e.Message}"));
            }

            return rejections;
        }
    }
}
