using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports);
        List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
	private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IDecisionEngineSpecification> specifications, IParsingService parsingService, IConfigService configService, Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
		_configService = configService;
            _logger = logger;
        }

        public List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports)
        {
            return GetMovieDecisions(reports).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase)
        {
            return GetMovieDecisions(reports, searchCriteriaBase).ToList();
        }

        private IEnumerable<DownloadDecision> GetMovieDecisions(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteria = null)
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
                DownloadDecision decision = null;
                _logger.ProgressTrace("Processing release {0}/{1}", reportNumber, reports.Count);

                try
                {
                    
                    var parsedMovieInfo = Parser.Parser.ParseMovieTitle(report.Title, _configService.ParsingLeniency > 0);

                    MappingResult result = null;

                    if (parsedMovieInfo == null || parsedMovieInfo.MovieTitle.IsNullOrWhiteSpace())
                    {
                        _logger.Debug("{0} could not be parsed :(.", report.Title);
                        parsedMovieInfo = new ParsedMovieInfo
                        {
                            MovieTitle = report.Title,
                            Year = 1290,
                            Languages = new List<Language>{Language.Unknown},
                            Quality = new QualityModel(),
                        };

                        if (_configService.ParsingLeniency == ParsingLeniencyType.MappingLenient)
                        {
                            result = _parsingService.Map(parsedMovieInfo, report.ImdbId.ToString(), searchCriteria);
                        }

                        if (result == null || result.MappingResultType != MappingResultType.SuccessLenientMapping)
                        {
                            result = new MappingResult {MappingResultType = MappingResultType.NotParsable};
                            result.Movie = null; //To ensure we have a remote movie, else null exception on next line!
                            result.RemoteMovie.ParsedMovieInfo = parsedMovieInfo;
                        }
                        else
                        {
                            //Enhance Parsed Movie Info!
                            result.RemoteMovie.ParsedMovieInfo = Parser.Parser.ParseMinimalMovieTitle(parsedMovieInfo.MovieTitle,
                                result.RemoteMovie.Movie.Title, parsedMovieInfo.Year);
                        }

                    }
                    else
                    {
                        result = _parsingService.Map(parsedMovieInfo, report.ImdbId.ToString(), searchCriteria);
                    }
                    
                    result.ReleaseName = report.Title;
                    var remoteMovie = result.RemoteMovie;

                    remoteMovie.Release = report;
                    remoteMovie.MappingResult = result.MappingResultType;

                    if (result.MappingResultType != MappingResultType.Success && result.MappingResultType != MappingResultType.SuccessLenientMapping)
                    {
                        var rejection = result.ToRejection();
                        decision = new DownloadDecision(remoteMovie, rejection);

                    }
                    else
                    {
                        if (parsedMovieInfo.Quality.HardcodedSubs.IsNotNullOrWhiteSpace())
                        {
                            //remoteMovie.DownloadAllowed = true;
                            if (_configService.AllowHardcodedSubs)
                            {
                                decision = GetDecisionForReport(remoteMovie, searchCriteria);
                            }
                            else
                            {
                                var whitelisted = _configService.WhitelistedHardcodedSubs.Split(',');
                                _logger.Debug("Testing: {0}", whitelisted);
                                if (whitelisted != null && whitelisted.Any(t => (parsedMovieInfo.Quality.HardcodedSubs.ToLower().Contains(t.ToLower()) && t.IsNotNullOrWhiteSpace())))
                                {
                                    decision = GetDecisionForReport(remoteMovie, searchCriteria);
                                }
                                else
                                {
                                    decision = new DownloadDecision(remoteMovie, new Rejection("Hardcoded subs found: " + parsedMovieInfo.Quality.HardcodedSubs));
                                }
                            }
                        }
                        else
                        {
                            //remoteMovie.DownloadAllowed = true;
                            decision = GetDecisionForReport(remoteMovie, searchCriteria);
                        }

                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteMovie = new RemoteMovie { Release = report };
                    decision = new DownloadDecision(remoteMovie, new Rejection("Unexpected error processing release"));
                }

                reportNumber++;

                if (decision != null)
                {
                    if (decision.Rejections.Any())
                    {
                        _logger.Debug("Release rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
                    }

                    else
                    {
                        _logger.Debug("Release accepted");
                    }

                    yield return decision;
                }
            }
        }

        private DownloadDecision GetDecisionForReport(RemoteMovie remoteMovie, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, remoteMovie, searchCriteria))
                                         .Where(c => c != null);

            return new DownloadDecision(remoteMovie, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IDecisionEngineSpecification spec, RemoteMovie remoteMovie, SearchCriteriaBase searchCriteriaBase = null)
        {
            try
            {
                var result = spec.IsSatisfiedBy(remoteMovie, searchCriteriaBase);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason, spec.Type);
                }
            }
            catch (NotImplementedException e)
            {
                _logger.Trace("Spec " + spec.GetType().Name + " does not care about movies.");
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteMovie.Release.ToJson());
                e.Data.Add("parsed", remoteMovie.ParsedMovieInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on " + remoteMovie.Release.Title + ", with spec: " + spec.GetType().Name);
                return new Rejection(string.Format("{0}: {1}", spec.GetType().Name, e.Message));//TODO UPDATE SPECS!
            }

            return null;
        }
    }
}
