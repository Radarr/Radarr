using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

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
        private readonly IRemoteBookAggregationService _aggregationService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IDecisionEngineSpecification> specifications,
            IParsingService parsingService,
            IRemoteBookAggregationService aggregationService,
            Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        public List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports)
        {
            return GetBookDecisions(reports).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase)
        {
            return GetBookDecisions(reports, searchCriteriaBase).ToList();
        }

        private IEnumerable<DownloadDecision> GetBookDecisions(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteria = null)
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
                _logger.Debug("Processing release '{0}' from '{1}'", report.Title, report.Indexer);

                try
                {
                    var parsedBookInfo = Parser.Parser.ParseBookTitle(report.Title);

                    if (parsedBookInfo == null)
                    {
                        if (searchCriteria != null)
                        {
                            parsedBookInfo = Parser.Parser.ParseBookTitleWithSearchCriteria(report.Title,
                                                                                              searchCriteria.Author,
                                                                                              searchCriteria.Books);
                        }
                        else
                        {
                            // try parsing fuzzy
                            parsedBookInfo = _parsingService.ParseAlbumTitleFuzzy(report.Title);
                        }
                    }

                    if (parsedBookInfo != null)
                    {
                        if (!parsedBookInfo.AuthorName.IsNullOrWhiteSpace())
                        {
                            var remoteBook = _parsingService.Map(parsedBookInfo, searchCriteria);

                            // try parsing again using the search criteria, in case it parsed but parsed incorrectly
                            if ((remoteBook.Author == null || remoteBook.Books.Empty()) && searchCriteria != null)
                            {
                                _logger.Debug("Author/Book null for {0}, reparsing with search criteria", report.Title);
                                var parsedBookInfoWithCriteria = Parser.Parser.ParseBookTitleWithSearchCriteria(report.Title,
                                                                                                                  searchCriteria.Author,
                                                                                                                  searchCriteria.Books);

                                if (parsedBookInfoWithCriteria != null && parsedBookInfoWithCriteria.AuthorName.IsNotNullOrWhiteSpace())
                                {
                                    remoteBook = _parsingService.Map(parsedBookInfoWithCriteria, searchCriteria);
                                }
                            }

                            remoteBook.Release = report;

                            if (remoteBook.Author == null)
                            {
                                decision = new DownloadDecision(remoteBook, new Rejection("Unknown Author"));

                                // shove in the searched author in case of forced download in interactive search
                                if (searchCriteria != null)
                                {
                                    remoteBook.Author = searchCriteria.Author;
                                    remoteBook.Books = searchCriteria.Books;
                                }
                            }
                            else if (remoteBook.Books.Empty())
                            {
                                decision = new DownloadDecision(remoteBook, new Rejection("Unable to parse books from release name"));
                                if (searchCriteria != null)
                                {
                                    remoteBook.Books = searchCriteria.Books;
                                }
                            }
                            else
                            {
                                _aggregationService.Augment(remoteBook);
                                remoteBook.DownloadAllowed = remoteBook.Books.Any();
                                decision = GetDecisionForReport(remoteBook, searchCriteria);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteBook = new RemoteBook { Release = report };
                    decision = new DownloadDecision(remoteBook, new Rejection("Unexpected error processing release"));
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

        private DownloadDecision GetDecisionForReport(RemoteBook remoteBook, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = new Rejection[0];

            foreach (var specifications in _specifications.GroupBy(v => v.Priority).OrderBy(v => v.Key))
            {
                reasons = specifications.Select(c => EvaluateSpec(c, remoteBook, searchCriteria))
                                                        .Where(c => c != null)
                                                        .ToArray();

                if (reasons.Any())
                {
                    break;
                }
            }

            return new DownloadDecision(remoteBook, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IDecisionEngineSpecification spec, RemoteBook remoteBook, SearchCriteriaBase searchCriteriaBase = null)
        {
            try
            {
                var result = spec.IsSatisfiedBy(remoteBook, searchCriteriaBase);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason, spec.Type);
                }
            }
            catch (NotImplementedException)
            {
                _logger.Trace("Spec " + spec.GetType().Name + " not implemented.");
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteBook.Release.ToJson());
                e.Data.Add("parsed", remoteBook.ParsedBookInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", remoteBook.Release.Title);
                return new Rejection($"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }
    }
}
