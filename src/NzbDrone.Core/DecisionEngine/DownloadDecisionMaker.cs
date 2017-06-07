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

//silv3r23
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
//END silv3r23

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
            if (searchCriteriaBase.Movie != null)
            {
                return GetMovieDecisions(reports, searchCriteriaBase).ToList();
            }

            return GetDecisions(reports, searchCriteriaBase).ToList();
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
                //silv3r23
                //get wanted Language for this movie
                var wantedLanguage = searchCriteria.Movie.Profile.Value.Language.ToString();
                _logger.ProgressTrace("DEV: Wanted Language: {0}", wantedLanguage);
                _logger.ProgressTrace("DEV: Title before cleanup: {0}", report.Title);
                //seperate Group from MovieTitle
                string[] stringSeparators = new string[] { "-" };
                string[] titleAndGroup = report.Title.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                //prepare MovieTitle for parsing
                _logger.ProgressTrace("DEV: Title w/o Group: {0}", titleAndGroup[0]);
                _logger.ProgressTrace("DEV: Group: {0}", String.IsNullOrEmpty(titleAndGroup[1]) ? "EmptyGroup" : titleAndGroup[1]);
                titleAndGroup[0] = titleAndGroup[0].Replace(" ", ".").Replace(":", ".").Replace("(", ".").Replace(")", ".").Replace("[", ".").Replace("]", ".").Replace("..", ".").Replace("ß", "ss");
                //Remove RemoveDiacritics
                titleAndGroup[0] = RemoveDiacritics(titleAndGroup[0]);

                //check if there is a year in the movie title
                string year = @"\d{4}";
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(year);
                string yearInTitle = r.Match(titleAndGroup[0]).ToString();
                //if true, get year and put language after year
                if (yearInTitle != "" || yearInTitle != null)
                {
                    _logger.ProgressTrace("DEV: Year found in title: {0}", yearInTitle);
                    //check if wanted language is found in Title String
                    if (titleAndGroup[0].IndexOf(wantedLanguage) != -1)
                    {
                        //remove wanted language from movie title and place language after the year
                        //titleAndGroup[0] = titleAndGroup[0].ToLower().Replace(wantedLanguage.ToLower(), "").Replace("..", ".");

                        titleAndGroup[0] = Regex.Replace(titleAndGroup[0], wantedLanguage, "", RegexOptions.IgnoreCase);
                        titleAndGroup[0] = titleAndGroup[0].Replace("..", ".");

                        _logger.ProgressTrace("DEV: Language '{0}' has been removed: {1}", wantedLanguage, titleAndGroup[0]);

                        string[] stringSeparatorsTemp = new string[] { yearInTitle };
                        string[] titleTemp = titleAndGroup[0].Split(stringSeparatorsTemp, StringSplitOptions.RemoveEmptyEntries);
                        titleAndGroup[0] = "";
                        int i = 0;
                        foreach (string ttemp in titleTemp)
                        {
                            _logger.ProgressTrace("DEV-Temp0: Array-Position : {0}", ttemp);
                            if (i == 1) //because the year is the first seperator
                            {
                                titleAndGroup[0] = titleAndGroup[0] + yearInTitle + "." + wantedLanguage + ttemp;
                                _logger.ProgressTrace("DEV-Temp1: New Title : {0}", titleAndGroup[0]);
                            }
                            else titleAndGroup[0] += ttemp;
                            _logger.ProgressTrace("DEV-Temp2: New Title : {0}", titleAndGroup[0]);
                            i++;
                        }
                        _logger.ProgressTrace("DEV: Language has been placed to the right order: {0}", titleAndGroup[0]);
                    }
                    else _logger.ProgressTrace("DEV: No Language found, seems to be an English RLS: {0}", titleAndGroup[0]);
                }
                else _logger.ProgressTrace("DEV: NO Year found in title: {0}", titleAndGroup[0]);


                report.Title = titleAndGroup[0] + "-" + titleAndGroup[1];
                _logger.ProgressTrace("DEV: Title after cleanup: {0}", report.Title);

                //END silv3r23
                DownloadDecision decision = null;
                _logger.ProgressTrace("Processing release {0}/{1}", reportNumber, reports.Count);

                try
                {
                    // silv3r23
                    //var parsedMovieInfo = Parser.Parser.ParseMovieTitle(report.Title);

                    //result from indexer
                    var parsedMovieInfo = Parser.Parser.ParseMovieTitle(RemoveDiacritics(Parser.Parser.ReplaceGermanUmlauts(report.Title.Replace(":", " ").Replace(",", ""))));
                    _logger.ProgressTrace("DEV: Looking for: '{0}'", parsedMovieInfo.MovieTitle);
                    //END silv3r23
                    if (parsedMovieInfo != null && !parsedMovieInfo.MovieTitle.IsNullOrWhiteSpace())
                    {
                        RemoteMovie remoteMovie = _parsingService.Map(parsedMovieInfo, report.ImdbId.ToString(), searchCriteria);
                        remoteMovie.Release = report;

                        if (remoteMovie.Movie == null)
                        {
                            decision = new DownloadDecision(remoteMovie, new Rejection("Unknown movie. Movie found does not match wanted movie."));
                        }
                        else
                        {
                            if (parsedMovieInfo.Quality.HardcodedSubs.IsNotNullOrWhiteSpace())
                            {
                                remoteMovie.DownloadAllowed = true;
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
                                remoteMovie.DownloadAllowed = true;
                                decision = GetDecisionForReport(remoteMovie, searchCriteria);
                            }

                        }
                    }
                    else
                    {
                        _logger.Trace("{0} could not be parsed :(.", report.Title);
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

        //silv3r23
        public static String RemoveDiacritics(String s)
        {
            String normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
        //END silv3r23

        private IEnumerable<DownloadDecision> GetDecisions(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteria = null)
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
                    var parsedEpisodeInfo = Parser.Parser.ParseTitle(report.Title);

                    if (parsedEpisodeInfo == null || parsedEpisodeInfo.IsPossibleSpecialEpisode)
                    {
                        var specialEpisodeInfo = _parsingService.ParseSpecialEpisodeTitle(report.Title, report.TvdbId, report.TvRageId, searchCriteria);

                        if (specialEpisodeInfo != null)
                        {
                            parsedEpisodeInfo = specialEpisodeInfo;
                        }
                    }

                    if (parsedEpisodeInfo != null && !parsedEpisodeInfo.SeriesTitle.IsNullOrWhiteSpace())
                    {
                        var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, report.TvdbId, report.TvRageId, searchCriteria);
                        remoteEpisode.Release = report;

                        if (remoteEpisode.Series == null)
                        {
                            //remoteEpisode.DownloadAllowed = true; //Fuck you :)
                            //decision = GetDecisionForReport(remoteEpisode, searchCriteria);
                            decision = new DownloadDecision(remoteEpisode, new Rejection("Unknown release. Series not Found."));
                        }
                        else if (remoteEpisode.Episodes.Empty())
                        {
                            decision = new DownloadDecision(remoteEpisode, new Rejection("Unable to parse episodes from release name"));
                        }
                        else
                        {
                            remoteEpisode.DownloadAllowed = remoteEpisode.Episodes.Any();
                            decision = GetDecisionForReport(remoteEpisode, searchCriteria);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteEpisode = new RemoteEpisode { Release = report };
                    decision = new DownloadDecision(remoteEpisode, new Rejection("Unexpected error processing release"));
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

        private DownloadDecision GetDecisionForReport(RemoteMovie remoteEpisode, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, remoteEpisode, searchCriteria))
                                         .Where(c => c != null);

            return new DownloadDecision(remoteEpisode, reasons.ToArray());
        }

        private DownloadDecision GetDecisionForReport(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, remoteEpisode, searchCriteria))
                                         .Where(c => c != null);

            return new DownloadDecision(remoteEpisode, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IDecisionEngineSpecification spec, RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteriaBase = null)
        {
            try
            {
                var result = spec.IsSatisfiedBy(remoteEpisode, searchCriteriaBase);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason, spec.Type);
                }
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteEpisode.Release.ToJson());
                e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on " + remoteEpisode.Release.Title + ", with spec: " + spec.GetType().Name);
                //return new Rejection(string.Format("{0}: {1}", spec.GetType().Name, e.Message));//TODO UPDATE SPECS!
                //return null;
            }

            return null;
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
