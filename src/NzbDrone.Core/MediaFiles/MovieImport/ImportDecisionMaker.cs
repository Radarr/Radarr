using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;


namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAggregationService _aggregationService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectSample _detectSample;
        private readonly IQualityDefinitionService _qualitiesService;
        private readonly IConfigService _config;
        private readonly IHistoryService _historyService;
        private readonly IParsingService _parsingService;
        private readonly ICached<string> _warnedFiles;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IMediaFileService mediaFileService,
                                   IAggregationService aggregationService,
                                   IDiskProvider diskProvider,
                                   IDetectSample detectSample,
                                   IQualityDefinitionService qualitiesService,
                                   IConfigService config,
                                   IHistoryService historyService,
                                   IParsingService parsingService,
                                   ICacheManager cacheManager,
                                   Logger logger)
        {
            _specifications = specifications;
            _mediaFileService = mediaFileService;
            _aggregationService = aggregationService;
            _diskProvider = diskProvider;
            _detectSample = detectSample;
            _qualitiesService = qualitiesService;
            _config = config;
            _historyService = historyService;
            _parsingService = parsingService;
            _warnedFiles = cacheManager.GetCache<string>(this.GetType());
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie)
        {
            return GetImportDecisions(videoFiles, movie, null, null, true);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), movie);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            ParsedMovieInfo downloadClientItemInfo = null;

            if (downloadClientItem != null)
            {
                downloadClientItemInfo = Parser.Parser.ParseMovieTitle(downloadClientItem.Title, false);
                downloadClientItemInfo = _parsingService.EnhanceMovieInfo(downloadClientItemInfo);
            }

            var nonSampleVideoFileCount = GetNonSampleVideoFileCount(newFiles, movie, downloadClientItemInfo, folderInfo);

            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                var localMovie = new LocalMovie
                {
                    Movie = movie,
                    DownloadClientMovieInfo = downloadClientItemInfo,
                    FolderMovieInfo = folderInfo,
                    Path = file,
                    SceneSource = sceneSource
                };

                decisions.AddIfNotNull(GetDecision(localMovie, downloadClientItem, nonSampleVideoFileCount > 1));
            }

            return decisions;
        }

        private ImportDecision GetDecision(LocalMovie localMovie, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            ImportDecision decision = null;

            var fileMovieInfo = Parser.Parser.ParseMoviePath(localMovie.Path, false);

            if (fileMovieInfo != null)
            {
                fileMovieInfo = _parsingService.EnhanceMovieInfo(fileMovieInfo);
            }            

            localMovie.FileMovieInfo = fileMovieInfo;
            localMovie.Size = _diskProvider.GetFileSize(localMovie.Path);

            try
            {
                _aggregationService.Augment(localMovie, otherFiles);

                if (localMovie.Movie == null)
                {
                    decision = new ImportDecision(localMovie, new Rejection("Invalid movie"));
                }
                else
                {
                    decision = GetDecision(localMovie, downloadClientItem);
                }
            }
            catch (AugmentingFailedException)
            {
                decision = new ImportDecision(localMovie, new Rejection("Unable to parse file"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't import file. {0}", localMovie.Path);

                decision = new ImportDecision(localMovie, new Rejection("Unexpected error processing file"));
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", localMovie.Path);
            }
            else if (decision.Rejections.Any())
            {
                _logger.Debug("File rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("File accepted");
            }

            return decision;
        }

        private ImportDecision GetDecision(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localMovie, downloadClientItem))
                                         .Where(c => c != null);

            return new ImportDecision(localMovie, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localMovie, downloadClientItem);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (NotImplementedException e)
            {
                _logger.Warn(e, "Spec " + spec.ToString() + " currently does not implement evaluation for movies.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't evaluate decision on {0}", localMovie.Path);
                return new Rejection($"{spec.GetType().Name}: {ex.Message}");
            }

            return null;
        }

        private int GetNonSampleVideoFileCount(List<string> videoFiles, Movie movie, ParsedMovieInfo downloadClientItemInfo, ParsedMovieInfo folderInfo)
        {
            return videoFiles.Count(file =>
            {
                var sample = _detectSample.IsSample(movie, file, false);

                if (sample == DetectSampleResult.Sample)
                {
                    return false;
                }

                return true;
            });
        }
    }
}
