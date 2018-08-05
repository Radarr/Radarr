using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
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


namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, bool shouldCheckQuality);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool shouldCheckQuality);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IDetectSample _detectSample;
        private readonly IQualityDefinitionService _qualitiesService;
        private readonly IConfigService _config;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IParsingService parsingService,
                                   IMediaFileService mediaFileService,
                                   IDiskProvider diskProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IDetectSample detectSample,
                                   IQualityDefinitionService qualitiesService,
                                   IConfigService config,
                                   IHistoryService historyService,
                                   Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _detectSample = detectSample;
            _qualitiesService = qualitiesService;
            _config = config;
            _historyService = historyService;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie)
        {
            return GetImportDecisions(videoFiles, movie, null, null, true, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, bool shouldCheckQuality = false)
        {
            return GetImportDecisions(videoFiles, movie, null, null, true, shouldCheckQuality);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), movie);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, movie, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, movie, downloadClientItem, folderInfo, sceneSource, shouldUseFolderName));
            }

            return decisions;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool shouldCheckQuality)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), movie);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, movie, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, movie, downloadClientItem, folderInfo, sceneSource, shouldUseFolderName, shouldCheckQuality));
            }

            return decisions;
        }

        private ImportDecision GetDecision(string file, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool shouldUseFolderName, bool shouldCheckQuality = false)
        {
            ImportDecision decision = null;

            try
            {
                var minimalInfo = shouldUseFolderName
                    ? folderInfo.JsonClone()
                    : _parsingService.ParseMinimalPathMovieInfo(file);

                LocalMovie localMovie = null;
                //var localMovie = _parsingService.GetLocalMovie(file, movie, shouldUseFolderName ? folderInfo : null, sceneSource);

                if (minimalInfo != null)
                {
                    //TODO: make it so media info doesn't ruin the import process of a new movie
                    var mediaInfo = _config.EnableMediaInfo ? _videoFileInfoReader.GetMediaInfo(file) : null;
                    var size = _diskProvider.GetFileSize(file);
                    var historyItems = _historyService.FindByDownloadId(downloadClientItem?.DownloadId ?? "");
                    var firstHistoryItem = historyItems.OrderByDescending(h => h.Date).FirstOrDefault();
                    var sizeMovie = new LocalMovie();
                    sizeMovie.Size = size;
                    localMovie = _parsingService.GetLocalMovie(file, minimalInfo, movie, new List<object>{mediaInfo, firstHistoryItem, sizeMovie}, sceneSource);
                    localMovie.Quality = GetQuality(folderInfo, localMovie.Quality, movie);
                    localMovie.Size = size;

                    _logger.Debug("Size: {0}", localMovie.Size);

                    decision = GetDecision(localMovie, downloadClientItem);

                }

                else
                {
                    localMovie = new LocalMovie();
                    localMovie.Path = file;

                    if (MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                    {
                        _logger.Warn("Unable to parse movie info from path {0}", file);
                    }

                    decision = new ImportDecision(localMovie, new Rejection("Unable to parse file"));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't import file. " + file);

                var localMovie = new LocalMovie { Path = file };
                decision = new ImportDecision(localMovie, new Rejection("Unexpected error processing file"));
            }

            //LocalMovie nullMovie = null;

            //decision = new ImportDecision(nullMovie, new Rejection("IMPLEMENTATION MISSING!!!"));

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
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on " + localMovie.Path);
                return new Rejection(string.Format("{0}: {1}", spec.GetType().Name, e.Message));
            }

            return null;
        }

        private bool ShouldUseFolderName(List<string> videoFiles, Movie movie, ParsedMovieInfo folderInfo)
        {
            if (folderInfo == null)
            {
                return false;
            }

            //if (folderInfo.FullSeason)
            //{
            //    return false;
            //}

            return videoFiles.Count(file =>
            {
                var size = _diskProvider.GetFileSize(file);
                var fileQuality = QualityParser.ParseQuality(file);
                //var sample = null;//_detectSample.IsSample(movie, GetQuality(folderInfo, fileQuality, movie), file, size, folderInfo.IsPossibleSpecialEpisode); //Todo to this

                return true;

                //if (sample)
                {
                    return false;
                }

                if (SceneChecker.IsSceneTitle(Path.GetFileName(file)))
                {
                    return false;
                }

                return true;
            }) == 1;
        }

        private QualityModel GetQuality(ParsedMovieInfo folderInfo, QualityModel fileQuality, Movie movie)
        {
            if (UseFolderQuality(folderInfo, fileQuality, movie))
            {
                _logger.Debug("Using quality from folder: {0}", folderInfo.Quality);
                return folderInfo.Quality;
            }

            return fileQuality;
        }

        private bool UseFolderQuality(ParsedMovieInfo folderInfo, QualityModel fileQuality, Movie movie)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.Quality.Quality == Quality.Unknown)
            {
                return false;
            }

            if (fileQuality.QualitySource == QualitySource.Extension)
            {
                return true;
            }

            if (fileQuality.QualitySource == QualitySource.MediaInfo)
            {
                return false;
            }

            if (new QualityModelComparer(movie.Profile).Compare(folderInfo.Quality, fileQuality) > 0)
            {
                return true;
            }

            return false;
        }

		private bool ShouldCheckQualityForParsedQuality(Quality quality)
		{
			List<Quality> shouldNotCheck = new List<Quality> { Quality.WORKPRINT, Quality.TELECINE, Quality.TELESYNC,
			Quality.DVDSCR, Quality.DVD, Quality.CAM, Quality.DVDR, Quality.Remux1080p, Quality.Remux2160p, Quality.REGIONAL
			};

			if (shouldNotCheck.Contains(quality))
			{
				return false;

			}

			return true;
		}
    }
}
